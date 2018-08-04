using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
using Expenses.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Expenses.Api.Users
{
    public static class LogIn
    {
        [FunctionName("LogIn")]
        public static HttpResponseMessage Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "get", 
                "post",
                Route = "users/login/{login}/{hashedPasswordConverted}")
            ]HttpRequestMessage req,
            string login,
            string hashedPasswordConverted,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] User entity,
            TraceWriter log)
        {
            log.Info("Request to LogIn");

            if (login == null)
            {
                log.Info("LogIn response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");

            }
            if (hashedPasswordConverted == null)
            {
                log.Info("LogIn response: BadRequest - hashedPassword is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a hashedPassword on the query string or in the request body");
            }
            var hashedPassword = HashUtil.RetrieveQueryStringSpecialCharacters(hashedPasswordConverted);
            if (entity == null
                || entity.PasswordHash != hashedPassword)
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
