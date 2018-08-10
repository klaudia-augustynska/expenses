using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Model;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Expenses.Api.Households
{
    public static class GetMembers
    {
        [FunctionName("GetMembers")]
        public static HttpResponseMessage Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get",
                "post", 
                Route = "households/members/{householdId}")
            ]HttpRequestMessage req,
            string householdId,
            [Table("ExpensesApp", "{householdId}", "{householdId}")] Household household,
            TraceWriter log)
        {
            if (householdId == null)
            {
                log.Info("GetMembers response: BadRequest - householdId is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass householdId on the query string or in the request body");
            }
            if (household == null)
            {
                log.Info("GetMembers response: BadRequest - no such household");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "No such household");
            }

            var dtoResponse = new GetMembersResponseDto()
            {
                Members = new List<GetMembersResponseDto.MemberDto>()
            };
            if (household.Members != null)
            {
                var members = JsonConvert.DeserializeObject<List<Member>>(household.Members);
                if (members != null)
                {
                    var confirmedMembers = members.Where(x => x.Uncorfirmed != true);
                    foreach (var member in confirmedMembers)
                    {
                        dtoResponse.Members.Add(new GetMembersResponseDto.MemberDto()
                        {
                            Name = member.Login,
                            WalletSummary = member.WalletSummary
                        });
                    }
                }
            }

            return req.CreateResponse(HttpStatusCode.OK, dtoResponse);
        }
    }
}
