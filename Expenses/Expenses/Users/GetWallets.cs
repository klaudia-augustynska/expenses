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
using Newtonsoft.Json;

namespace Expenses.Api.Users
{
    public static class GetWallets
    {
        [FunctionName("GetWallets")]
        public static HttpResponseMessage Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get", 
                "post", 
                Route = "users/wallets/{householdId}/{login}/")
            ]HttpRequestMessage req, 
            string householdId,
            string login,
            [Table("ExpensesApp", "{householdId}", "user_{login}")] UserDetails userDetails, 
            TraceWriter log)
        {
            log.Info("GetWallets processed a request.");

            if (login == null)
            {
                log.Info("GetWallets response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }
            if (householdId == null)
            {
                log.Info("GetWallets response: BadRequest - householdId is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a householdId on the query string or in the request body");
            }
            if (userDetails == null)
            {
                log.Info($"GetWallets response: BadRequest - User with given login does not exist");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist"
                    );
            }

            var wallets = userDetails.Wallets;
            var response = JsonConvert.DeserializeObject<List<Wallet>>(wallets);

            return req.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
