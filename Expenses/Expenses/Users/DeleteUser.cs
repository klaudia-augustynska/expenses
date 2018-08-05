using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;

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

            TableOperation tableOperation = TableOperation.Delete(entity);
            await table.ExecuteAsync(tableOperation);

            var dbUser = $"user_{login}";
            log.Info($"DeleteUser response: Deleted entity with PK={dbUser} and RK={dbUser}");
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
