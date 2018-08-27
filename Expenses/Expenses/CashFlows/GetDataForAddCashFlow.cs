using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Api.Categories;
using Expenses.Model;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Expenses.Api.CashFlows
{
    public static class GetDataForAddCashFlow
    {
        [FunctionName("GetDataForAddCashFlow")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                "post",
                Route = "cashflows/getdata/{householdId}/{login}/")
            ]HttpRequestMessage req,
            string householdId,
            string login,
            [Table("ExpensesApp")] CloudTable table,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] UserLogInData userLogInData,
            [Table("ExpensesApp", "{householdId}", "user_{login}")] UserDetails userDetails,
            TraceWriter log)
        {
            log.Info("GetDataForAddCashFlow processed a request.");


            if (login == null)
            {
                log.Info("GetDataForAddCashFlow response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }
            if (householdId == null)
            {
                log.Info("GetDataForAddCashFlow response: BadRequest - householdId is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a householdId on the query string or in the request body");
            }
            if (userLogInData == null)
            {
                log.Info($"GetDataForAddCashFlow response: BadRequest - user does not exist");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist"
                    );
            }
            if (userDetails == null)
            {
                log.Info($"GetDataForAddCashFlow response: BadRequest - User with given login does not exist");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist"
                    );
            }

            var categoriesDto = await GetCategoriesDtoBuilder.Build(userLogInData, table);
            var walletsDto = GetWallets(userDetails);

            if (categoriesDto == null || walletsDto == null)
            {
                log.Info($"GetDataForAddCashFlow response: InternalServerError - user does not have categories neither in household nor in user detailed info, or there is a problem with his wallets");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.InternalServerError,
                    value: "Cannot get categories or wallets"
                    );
            }

            var responseDto = new GetDataForAddCashFlowResponseDto()
            {
                Categories = categoriesDto.Categories,
                Wallets = walletsDto
            };

            return req.CreateResponse(HttpStatusCode.OK, responseDto);
        }

        private static List<Wallet> GetWallets(UserDetails userDetails)
        {
            var wallets = userDetails.Wallets;
            var response = JsonConvert.DeserializeObject<List<Wallet>>(wallets);
            return response;
        }
    }
}
