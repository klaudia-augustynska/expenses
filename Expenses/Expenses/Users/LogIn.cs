using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Dto;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
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
            [Table("ExpensesApp", "user_{login}", "user_{login}")] User entity,
            TraceWriter log)
        {
            log.Info("Request to LogIn");

            LogInDto logInDto = null;
            try
            {
                logInDto = JsonConvert.DeserializeObject<LogInDto>(await req.Content.ReadAsStringAsync());
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
            return req.CreateResponse(HttpStatusCode.OK, entity.Key);
        }
    }
}
