using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Expenses.Api.Households
{
    public static class AcceptInvitationToHousehold
    {
        [FunctionName("AcceptInvitationToHousehold")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get", 
                "post", 
                Route = "households/accept/{from}/{to}/{rowKey}")
            ]HttpRequestMessage req,
            string from,
            string to,
            string rowKey,
            [Table("ExpensesApp")] CloudTable table,
            [Table("ExpensesApp", "household_{from}", "household_{from}")] Household household,
            [Table("ExpensesApp", "user_{from}", "user_{from}")] UserLogInData invitersLogInData,
            [Table("ExpensesApp", "user_{to}", "user_{to}")] UserLogInData invitedUserLogInData,
            [Table("ExpensesApp", "household_{to}", "user_{to}")] UserDetails invitedUser,
            [Table("ExpensesApp", "message_{to}", "{rowKey}")] Message message,
            TraceWriter log)
        {
            if (from == null)
            {
                log.Info("AcceptInvitationToHousehold response: BadRequest - inviter's login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass an inviter's login on the query string or in the request body");
            }
            if (to == null)
            {
                log.Info("AcceptInvitationToHousehold response: BadRequest - receiver's login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a receiver's login on the query string or in the request body");
            }

            if (!(await SetThatInvterHasAGroup(invitersLogInData, table, log))
                || !(await SetThatReceiverIsConfirmedAndAddHisWalletsAndCategories(household, invitedUser, table, log))
                || !(await SetUserBelongsToHousehold(household.PartitionKey, invitedUserLogInData, table, log))
                || !(await DeleteInvitationMessage(message, table, log)))
            {
                log.Info("AcceptInvitationToHousehold response: Problem with activating user");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.InternalServerError,
                    value: "Problem with activating user");
            }

            return req.CreateResponse(HttpStatusCode.OK, household.PartitionKey);
        }

        private static async Task<bool> SetThatInvterHasAGroup(UserLogInData invitersLogInData, CloudTable table, TraceWriter log)
        {
            try
            {
                log.Info("SetThatInvterHasAGroup");
                invitersLogInData.BelongsToGroup = true;
                var updateOp = TableOperation.Replace(invitersLogInData);
                await table.ExecuteAsync(updateOp);
            }
            catch (Exception ex)
            {
                log.Error("Cannot set that invter has a group", ex);
                return false;
            }
            return true;
        }

        private static async Task<bool> DeleteInvitationMessage(Message message, CloudTable table, TraceWriter log)
        {
            try
            {
                log.Info("DeleteInvitationMessage");
                var deleteTableOperation = TableOperation.Delete(message);
                await table.ExecuteAsync(deleteTableOperation);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot delete invitation message", ex);
                return false;
            }
        }

        private static async Task<bool> SetUserBelongsToHousehold(string partitionKey, UserLogInData invitedUser, CloudTable table, TraceWriter log)
        {
            try
            {
                log.Info("SetUserBelongsToHousehold");
                invitedUser.HouseholdId = partitionKey;
                invitedUser.BelongsToGroup = true;
                var updateTableOperation = TableOperation.Replace(invitedUser);
                await table.ExecuteAsync(updateTableOperation);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot edit user's household", ex);
                return false;
            }
        }

        private static async Task<bool> SetThatReceiverIsConfirmedAndAddHisWalletsAndCategories(Household household, UserDetails to, CloudTable table, TraceWriter log)
        {
            try
            {
                log.Info("SetThatReceiverIsConfirmedAndAddHisWallets");
                var userWallets = JsonConvert.DeserializeObject<List<Wallet>>(to.Wallets);
                
                // members
                var members = JsonConvert.DeserializeObject<List<Member>>(household.Members);
                var member = members.First(x => x.Login == to.Login);
                member.Uncorfirmed = null;
                var walletAggregated = Aggregation.MergeWallets(userWallets);
                member.WalletSummary = walletAggregated;
                member.Tmr = CalorieRateCalculator.Tmr(to.Sex.Value, to.Weight.Value, to.Height.Value, AgeUtil.GetAge(to.DateOfBirth.Value), to.Pal.Value);
                household.Members = JsonConvert.SerializeObject(members);

                // wallets
                var householdMoney = JsonConvert.DeserializeObject<List<Money>>(household.MoneyAggregated);
                var merged = Aggregation.MergeWallets(householdMoney, userWallets);
                household.MoneyAggregated = JsonConvert.SerializeObject(merged);

                // categories
                var householdCategories = JsonConvert.DeserializeObject<List<Category>>(household.CategoriesAggregated);
                var newUserCategories = JsonConvert.DeserializeObject<List<Category>>(to.Categories);
                var categoriesMerged = Aggregation.MergeCategories(members, householdCategories, newUserCategories);
                household.CategoriesAggregated = JsonConvert.SerializeObject(categoriesMerged);

                var updateTableOperation = TableOperation.Replace(household);
                await table.ExecuteAsync(updateTableOperation);

                var userAsHouseholdMember = new UserDetails(to);
                userAsHouseholdMember.PartitionKey = household.PartitionKey;
                var insertTableOperation = TableOperation.Insert(userAsHouseholdMember);
                await table.ExecuteAsync(insertTableOperation);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot add user to a household", ex);
                return false;
            }
        }
    }
}
