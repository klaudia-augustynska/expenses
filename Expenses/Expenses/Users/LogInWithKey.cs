using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;

namespace Expenses.Api.Users
{
    public static class LogInWithKey
    {
        [FunctionName("LogInWithKey")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get", "post", 
                Route = "users/loginwithkey/{login}")
            ]HttpRequestMessage req, 
            string login,
            [Table("ExpensesApp")] CloudTable table,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] UserLogInData entity,
            TraceWriter log)
        {
            if (login == null)
            {
                log.Info("LogIn response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");

            }

            if (entity == null)
            {
                log.Info($"LogIn response: BadRequest - no such user");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist"
                    );
            }

            string key = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "code", true) == 0)
                .Value;

            if (entity.Key != key)
            {
                log.Info($"LogIn response: BadRequest - key doesn't belong to this user");
                log.Info($"-----------------was: {key}");
                log.Info($"but should have been: {entity.Key}");
                int i = 0;
                for (; i < key.Length; ++i)
                    if (entity.Key.Length < i + 1 || entity.Key[i] != key[i])
                        break;
                log.Info($"at char: {i}");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Given key doesn't belong to this user"
                    );
            }

            log.Info($"LogIn response: OK - user {login} has been authenticated");

            UserDetails userDetails = null;
            try
            {
                var rertieveTableOperation = TableOperation.Retrieve<UserDetails>(entity.HouseholdId, entity.PartitionKey);
                var result = await table.ExecuteAsync(rertieveTableOperation);
                userDetails = result?.Result as UserDetails;
            }
            catch (Exception ex)
            {
                log.Error($"LogIn response: InternalServerError. Couldn't retreieve UserDetails PK={entity.HouseholdId}, RK={entity.PartitionKey}", ex);
                return req.CreateResponse(
                    statusCode: HttpStatusCode.InternalServerError,
                    value: "Couldn't retreieve UserDetails"
                    );
            }

            bool userBelongsToHousehold = false;
            try
            {
                var retrieveOperation = TableOperation.Retrieve<Household>(entity.HouseholdId, entity.HouseholdId);
                var result = await table.ExecuteAsync(retrieveOperation);
                if (result != null && result.Result != null && result.Result is Household)
                    userBelongsToHousehold = true;
            }
            catch (Exception ex)
            {
                log.Error($"LogIn response: InternalServerError. Couldn't retreieve Household PK={entity.HouseholdId}, RK={entity.PartitionKey}", ex);
                return req.CreateResponse(
                    statusCode: HttpStatusCode.InternalServerError,
                    value: "Couldn't retreieve Household"
                    );
            }

            var responseDto = new LogInResponseDto()
            {
                Key = entity.Key,
                Configured = userDetails != null,
                HouseholdId = entity.HouseholdId,
                BelongsToHousehold = userBelongsToHousehold
            };
            return req.CreateResponse(HttpStatusCode.OK, responseDto);
        }
    }
}
