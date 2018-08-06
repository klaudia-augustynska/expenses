using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Expenses.Api.Users
{
    public static class GetSalt
    {
        [FunctionName("GetSalt")]
        public static HttpResponseMessage Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "get", 
                "post", 
                Route = "users/salt/{login}/")
            ]HttpRequestMessage req,
            string login,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] User entity,
            TraceWriter log)
        {
            log.Info("Request to GetSalt");

            var dbUser = $"user_{login}";

            if (login == null)
            {
                log.Info("GetSalt response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }

            string salt;
            if (entity == null)
            {
                salt = HashUtil.GenerateSalt();
                log.Info($"GetSalt response: no entity with PK={dbUser} and RK={dbUser} in the database. Responding with fake salt: {salt}");
            }
            else
            {
                salt = entity.Salt;
                log.Info($"GetSalt response: successfully found entity with PK={dbUser} and RK={dbUser} in the database. Responding with corresponding salt: {salt}");
            }
            //return req.CreateResponse(HttpStatusCode.OK, salt);
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(salt)
            };
        }
    }
}
