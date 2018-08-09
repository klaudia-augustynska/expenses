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

namespace Expenses.Api.Users
{
    public static class DeleteUser
    {
        [FunctionName("DeleteUser")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get", 
                "post", 
                Route = "users/delete/{login}/")
            ]HttpRequestMessage req,
            string login,
            [Table("ExpensesApp")] CloudTable table,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] User entity,
            TraceWriter log)
        {
            if (login == null)
            {
                log.Info("DeleteUser response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }
            if (entity == null)
            {
                log.Info($"DeleteUser response: BadRequest - no such user");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist"
                    );
            }

            // usuñ wp³yw tej osoby na gospodarstwo
            // - usuñ z listy cz³onków
            // - usuñ z zagregowanych œrodków

            if (!string.IsNullOrEmpty(entity.HouseholdId))
                await DeleteInfluenceOnHousehold(entity.HouseholdId, entity.Login, entity.Wallets, table, log);

            TableOperation tableOperation = TableOperation.Delete(entity);
            await table.ExecuteAsync(tableOperation);

            var dbUser = $"user_{login}";
            log.Info($"DeleteUser response: Deleted entity with PK={dbUser} and RK={dbUser}");
            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static async Task DeleteInfluenceOnHousehold(string householdId, string login, string wallets, CloudTable table, TraceWriter log)
        {
            var retrieveTableOperation = TableOperation.Retrieve<Household>(householdId, householdId);
            var exec = await table.ExecuteAsync(retrieveTableOperation);
            
            if (exec.Result != null)
            {
                var household = exec.Result as Household;

                // members
                var members = JsonConvert.DeserializeObject<List<Member>>(household.Members);
                var itemToRemove = members.FirstOrDefault(x => x.Login == login);
                if (itemToRemove != null)
                {
                    members.Remove(itemToRemove);
                }
                household.Members = JsonConvert.SerializeObject(members);

                // money
                var money = JsonConvert.DeserializeObject<List<Money>>(household.MoneyAggregated);
                var userWallets = JsonConvert.DeserializeObject<List<Wallet>>(wallets);
                var excluded = Aggregation.ExcludeWallets(money, userWallets);
                household.MoneyAggregated = JsonConvert.SerializeObject(excluded);

                var replaceTableOperation = TableOperation.Replace(household);
                await table.ExecuteAsync(replaceTableOperation);
            }
            else
            {
                log.Warning("DeleteInfluenceOnHousehold: household should not be null");
            }
        }
    }
}
