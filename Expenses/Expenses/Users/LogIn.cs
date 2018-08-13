using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Expenses.Api.Users
{
    public static class LogIn
    {
        [FunctionName("LogIn")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "post",
                Route = "users/login/{login}/")
            ]HttpRequestMessage req,
            string login,
            [Table("ExpensesApp")] CloudTable table,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] UserLogInData entity,
            TraceWriter log)
        {
            log.Info("Request to LogIn");

            LogInDto logInDto = null;
            try
            {
                logInDto = await req.Content.ReadAsDeserializedJson<LogInDto>();
            }
            catch
            {
                log.Info("LogIn response: BadRequest - cannot read dto object");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a valid dto object in the request content");
            }

            if (login == null)
            {
                log.Info("LogIn response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");

            }
            if (logInDto == null
                || string.IsNullOrWhiteSpace(logInDto.HashedPassword))
            {
                log.Info("LogIn response: BadRequest - dto object content is not valid");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass correct values to the dto object");
            }
            if (entity == null
                || entity.PasswordHash != logInDto.HashedPassword)
            {
                log.Info($"LogIn response: BadRequest - wrong credentials");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist or the password was incorrect"
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
