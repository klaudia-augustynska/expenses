using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
                Route = "users/login/{login}/{password}")
            ]HttpRequestMessage req,
            string login,
            string password,
            [Table("ExpensesApp", "user_{login}", "{password}")] User entity,
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
            if (password == null)
            {
                log.Info("LogIn response: BadRequest - password is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a password on the query string or in the request body");
            }
            if (entity == null)
            {
                log.Info($"LogIn response: BadRequest - no such entity entity with PK=user_{login} and RK={password}");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login and password does not exist"
                    );
            }

            log.Info($"LogIn response: OK - user {login} has been authenticated");
            return req.CreateResponse(HttpStatusCode.OK, entity.Key);
        }
    }
}
