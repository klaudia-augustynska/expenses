using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Expenses.Api.Households
{
    public static class InviteToHousehold
    {
        [FunctionName("InviteToHousehold")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get", 
                "post", 
                Route = "households/invite/{invitersLogin}/{receiverLogin}")
            ]HttpRequestMessage req, 
            string invitersLogin,
            string receiverLogin,
            [Table("ExpensesApp")] CloudTable table,
            [Table("ExpensesApp", "user_{invitersLogin}", "user_{invitersLogin}")] User invitersEntity,
            [Table("ExpensesApp", "user_{receiverLogin}", "user_{receiverLogin}")] User receiverEntity,
            TraceWriter log)
        {

            if (invitersLogin == null)
            {
                log.Info("InviteToHousehold response: BadRequest - inviter's login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass an inviter's login on the query string or in the request body");
            }
            if (receiverLogin == null)
            {
                log.Info("InviteToHousehold response: BadRequest - invited person's login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass an invited person's login on the query string or in the request body");
            }
            if (invitersEntity == null)
            {
                log.Info($"InviteToHousehold response: BadRequest - no such user: {invitersLogin}");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: $"User with login {invitersLogin} does not exist"
                    );
            }
            if (receiverEntity == null)
            {
                log.Info($"InviteToHousehold response: BadRequest - no such user: {receiverEntity}");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: $"User with login {receiverEntity} does not exist"
                    );
            }
            if (!string.IsNullOrEmpty(receiverEntity.HouseholdId))
            {
                log.Info($"InviteToHousehold response: BadRequest - user {receiverEntity} already belongs to a household");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: $"User with login {receiverEntity} already belongs to a household"
                    );
            }

            if (await InsertInvitationMessage(
                inviter: invitersEntity, 
                receiver: receiverEntity, 
                table: table, log: log) == null)
            {
                return req.CreateResponse(
                    statusCode: HttpStatusCode.InternalServerError,
                    value: $"Can't insert invitation message"
                    );
            }
            if (await UpsertHousehold(
                source: invitersEntity,
                notConfirmedMember: receiverEntity,
                table: table, log: log) == null)
            {
                return req.CreateResponse(
                    statusCode: HttpStatusCode.InternalServerError,
                    value: $"Can't upsert household"
                    );
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static async Task<TableResult> InsertInvitationMessage(User inviter, User receiver, CloudTable table, TraceWriter log)
        {
            var date = DateTime.UtcNow;
            var rowKey = RowKeyUtils.GetInvertedDateString(date);

            var message = new Message()
            {
                PartitionKey = $"message_{receiver.Login}",
                RowKey = rowKey,
                From = JsonConvert.SerializeObject(new UserShort()
                {
                    Name = inviter.Name,
                    Login = inviter.Login
                }),
                Topic = $"{inviter.Name} invites you to household",
                Content = $"/api/households/accept/{inviter.HouseholdId ?? inviter.Login}/{receiver.Login}/{rowKey}"
            };

            log.Info("InviteToHousehold: proceeding with inserting invitation message");
            try
            {
                TableOperation insertOperation = TableOperation.Insert(message);
                return await table.ExecuteAsync(insertOperation);
            }
            catch (Exception ex)
            {
                log.Error("InviteToHousehold: failed to insert invitation message", ex);
                return null;
            }
        }

        private static async Task<TableResult> UpsertHousehold(User source, User notConfirmedMember, CloudTable table, TraceWriter log)
        {
            if (string.IsNullOrEmpty(source.HouseholdId))
            {
                log.Info($"InviteToHousehold: the inviter does not belong to any household yet. Creating a new one.");
                var householdPk = $"household_{source.Login}";
                await CreateNewHousehold(
                    newHouseholdPk: householdPk,
                    owner: source.Login,
                    wallets: source.Wallets,
                    notConfirmedMemberLogin: notConfirmedMember.Login,
                    table: table, log: log);
                return await UpdateUserHouseholdInfo(
                    user: source, 
                    household: householdPk, 
                    table: table, log: log);
            }
            else
            {
                log.Info($"InviteToHousehold: the inviter belongs to a household {source.HouseholdId}");
                return await UpdateExistingHousehold(
                    householdPk: $"{source.HouseholdId}",
                    notConfirmedMemberLogin: notConfirmedMember.Login,
                    table: table, log: log);
            }
        }

        private static async Task<TableResult> UpdateUserHouseholdInfo(User user, string household, CloudTable table, TraceWriter log)
        {
            user.HouseholdId = household;
            try
            {
                log.Info($"InviteToHousehold: proceeding to add household {household} to user {user.Login}");
                TableOperation updateOperation = TableOperation.Replace(user);
                return await table.ExecuteAsync(updateOperation);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to edit user {user.Login}", ex);
                return null;
            }
        }

        private static async Task<TableResult> CreateNewHousehold(string newHouseholdPk, string owner, string wallets, string notConfirmedMemberLogin, CloudTable table, TraceWriter log)
        {
            string money = null;
            try
            {
                log.Info("InviteToHousehold: converting user wallets to household money");
                var walletsList = JsonConvert.DeserializeObject<List<Wallet>>(wallets);
                var moneyList = Aggregation.MergeWallets(walletsList);
                money = JsonConvert.SerializeObject(moneyList);
            }
            catch (Exception ex)
            {
                log.Error("InviteToHousehold: cannot convert user wallets to household money", ex);
                return null;
            }

            var household = new Household()
            {
                PartitionKey = newHouseholdPk,
                RowKey = newHouseholdPk,
                Members = JsonConvert.SerializeObject(new List<Member>()
                {
                    new Member()
                    {
                        Login = owner
                    },
                    new Member()
                    {
                        Login = notConfirmedMemberLogin,
                        Uncorfirmed = true
                    }
                }),
                MoneyAggregated = money
            };

            try
            {
                log.Info($"InviteToHousehold: adding new household {newHouseholdPk}");
                TableOperation insertTableOperation = TableOperation.InsertOrReplace(household);
                return await table.ExecuteAsync(insertTableOperation);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to add household {newHouseholdPk}", ex);
                return null;
            }
        }

        private static async Task<TableResult> UpdateExistingHousehold(string householdPk, string notConfirmedMemberLogin, CloudTable table, TraceWriter log)
        {
            Household household = null;

            try
            {
                log.Info($"InviteToHousehold: retrieving household {householdPk}");
                var retrieveTableOperation = TableOperation.Retrieve<Household>(householdPk, householdPk);
                household = (await table.ExecuteAsync(retrieveTableOperation)).Result as Household;
                if (household == null)
                    throw new Exception($"household {householdPk} should not be null");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to retreive household {householdPk}", ex);
                return null;
            }

            try
            {
                log.Info($"InviteToHousehold: deserializing household {householdPk} members");
                var list = JsonConvert.DeserializeObject<List<Member>>(household.Members);

                log.Info($"InviteToHousehold: overriding household {householdPk} members' list");
                list.Add(new Member()
                {
                    Login = notConfirmedMemberLogin,
                    Uncorfirmed = true
                });
                household.Members = JsonConvert.SerializeObject(list);
                var replaceTableOperation = TableOperation.Replace(household);
                return await table.ExecuteAsync(replaceTableOperation);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to override household {householdPk}", ex);
                return null;
            }
        }
    }
}
