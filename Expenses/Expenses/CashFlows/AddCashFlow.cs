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
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Expenses.Api.CashFlows
{
    public static class AddCashFlow
    {
        [FunctionName("AddCashFlow")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "post", 
                Route = "cashflows/add/{login}/")
            ]HttpRequestMessage req, 
            string login,
            [Table("ExpensesApp")]ICollector<Cashflow> outTable,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] UserLogInData user,
            [Queue("expenses-addcashflow")] CloudQueue queue,
            [Table("ExpensesApp")] CloudTable table,
            TraceWriter log)
        {
            AddCashFlowDto dto = null;
            try
            {
                log.Info($"json dto: " + req.Content);
                dto = await req.Content.ReadAsDeserializedJson<AddCashFlowDto>();
            }
            catch
            {
                log.Info("AddCashFlow response: BadRequest - cannot read dto object");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a valid dto object in the request content");
            }
            if (login == null)
            {
                log.Info("AddCashFlow response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }
            if (user == null)
            {
                log.Info($"AddCashFlow response: BadRequest - user does not exist");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist"
                    );
            }

            var cashflowBase = new Cashflow()
            {
                DateTime = dto.DateTime,
                CategoryGuid = dto.CategoryGuid,
                Amount = JsonConvert.SerializeObject(dto.Amount),
                Details = JsonConvert.SerializeObject(dto.Details),
                WalletGuid = dto.WalletGuid
            };
            var dateTimeInverted = RowKeyUtils.GetInvertedDateString(dto.DateTime);
            var guid = Guid.NewGuid();

            if (user.BelongsToGroup)
            {

                var cashflowHousehold = new Cashflow(cashflowBase)
                {
                    PartitionKey = user.HouseholdId,
                    RowKey = $"householdCashflow_{dateTimeInverted}_{guid}"
                };
                outTable.Add(cashflowHousehold);
                log.Info($"Added cashflowHousehold PK={cashflowHousehold.PartitionKey} RK={cashflowHousehold.RowKey}");

            }
            var cashflowUser = new Cashflow(cashflowBase)
            {
                PartitionKey = user.HouseholdId,
                RowKey = $"userCashflow_{login}_{dateTimeInverted}_{guid}"
            };
            outTable.Add(cashflowUser);
            log.Info($"Added cashflowHousehold PK={cashflowUser.PartitionKey} RK={cashflowUser.RowKey}");

            //var cashflowHouseholdCategory = new Cashflow(cashflowBase)
            //{
            //    PartitionKey = user.HouseholdId,
            //    RowKey = $"householdCategoryCashflow_{dto.CategoryGuid}_{dateTimeInverted}_{guid}"
            //};
            //outTable.Add(cashflowHouseholdCategory);
            //log.Info($"Added cashflowHousehold PK={cashflowHouseholdCategory.PartitionKey} RK={cashflowHouseholdCategory.RowKey}");

            //var cashflowUserCategory = new Cashflow(cashflowBase)
            //{
            //    PartitionKey = user.HouseholdId,
            //    RowKey = $"userCategoryCashflow_{login}_{dto.CategoryGuid}_{dateTimeInverted}_{guid}"
            //};
            //outTable.Add(cashflowUserCategory);
            //log.Info($"Added cashflowHousehold PK={cashflowUserCategory.PartitionKey} RK={cashflowUserCategory.RowKey}");

            var addMessageDto = new AddMessageToAddCashflowQueueDto()
            {
                Amount = dto.Amount,
                HouseholdPk = user.HouseholdId,
                HouseholdRk = user.HouseholdId,
                WalletGuid = dto.WalletGuid,
                CategoryGuid = dto.CategoryGuid,
                UserBelongsToGroup = user.BelongsToGroup,
                Login = login
            };

            if (user.BelongsToGroup)
            {
                var message = JsonConvert.SerializeObject(addMessageDto);
                await queue.AddMessageAsync(new CloudQueueMessage(message));
                log.Info($"Enqueued message {message}");
            }
            else
            {
                log.Info("User does not belong to a group. Only his wallet will be updated");
                await UpdateUsersWallet(addMessageDto, table, log);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static async Task UpdateUsersWallet(AddMessageToAddCashflowQueueDto dto, CloudTable table, TraceWriter log)
        {
            var retrievePersonWhoPay = TableOperation.Retrieve<UserDetails>(dto.HouseholdPk, $"user_{dto.Login}");
            var operationResult = table.Execute(retrievePersonWhoPay);
            if (operationResult.Result != null && operationResult.Result is UserDetails)
            {
                var personWhoPay = operationResult.Result as UserDetails;

                log.Info($"DequeueAddCashflow - user who pay retreived. Wallets: {JsonConvert.SerializeObject(personWhoPay.Wallets)}");
                DequeueAddCashflow.UpdateWallets(personWhoPay, dto, log);
                var updateOperation = TableOperation.Replace(personWhoPay);
                await table.ExecuteAsync(updateOperation);
                log.Info($"DequeueAddCashflow - updated wallets of person who pay. Wallets: {JsonConvert.SerializeObject(personWhoPay.Wallets)}");
            }
        }
    }
}
