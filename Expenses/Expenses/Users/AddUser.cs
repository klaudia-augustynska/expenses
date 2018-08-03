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
    public static class AddUser
    {
        [FunctionName("AddUser")]
        public static HttpResponseMessage Run(
            [HttpTrigger( 
                AuthorizationLevel.Function,
                "get", 
                "post", 
                Route = "users/add/{login}/{password}")
            ]HttpRequestMessage req, 
            string login,
            string password,
            [Table("ExpensesApp")]ICollector<User> outTable,
            [Table("ExpensesApp", "{login}", "{password}")] User entity,
            TraceWriter log)
        {
            if (login == null)
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            if (password == null)
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a password on the query string or in the request body");
            if (entity != null)
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login and password already exists"
                    );

            outTable.Add(new User()
            {
                PartitionKey = login,
                RowKey = password,
                Login = login,
                Password = password
            });

            return req.CreateResponse(HttpStatusCode.Created);
        }
    }
}
