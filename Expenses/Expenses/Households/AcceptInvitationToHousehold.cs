using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Model;
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
            [Table("ExpensesApp", "user_{to}", "user_{to}")] User invitedUser,
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

            if (!(await SetThatReceiverIsConfirmed(household, to, table, log))
                || !(await SetUserBelongsToHousehold(household.PartitionKey, invitedUser, table, log))
                || !(await DeleteInvitationMessage(message, table, log)))
            {
                log.Info("AcceptInvitationToHousehold response: Problem with activating user");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.InternalServerError,
                    value: "Problem with activating user");
            }

            return req.CreateResponse(HttpStatusCode.OK);
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

        private static async Task<bool> SetUserBelongsToHousehold(string partitionKey, User invitedUser, CloudTable table, TraceWriter log)
        {
            try
            {
                log.Info("SetUserBelongsToHousehold");
                invitedUser.HouseholdId = partitionKey;
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

        private static async Task<bool> SetThatReceiverIsConfirmed(Household household, string to, CloudTable table, TraceWriter log)
        {
            try
            {
                log.Info("SetThatReceiverIsConfirmed");
                var members = JsonConvert.DeserializeObject<List<Member>>(household.Members);
                members.First(x => x.Login == to).Uncorfirmed = null;
                household.Members = JsonConvert.SerializeObject(members);

                var updateTableOperation = TableOperation.Replace(household);
                await table.ExecuteAsync(updateTableOperation);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot activate user", ex);
                return false;
            }
        }
    }
}
