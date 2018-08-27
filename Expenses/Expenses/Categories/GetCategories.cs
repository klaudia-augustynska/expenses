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
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Expenses.Api.Categories
{
    public static class GetCategories
    {
        [FunctionName("GetCategories")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                "post",
                Route = "categories/getall/{login}/")
            ]HttpRequestMessage req,
            string login,
            [Table("ExpensesApp")] CloudTable table,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] UserLogInData entity,
            TraceWriter log)
        {
            if (login == null)
            {
                log.Info("GetCategories response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");

            }
            if (entity == null)
            {
                log.Info($"GetCategories response: BadRequest - user does not exist");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist"
                    );
            }

            var dtoResponse = await GetCategoriesDtoBuilder.Build(entity, table);
            if (dtoResponse == null)
            {
                log.Info($"GetCategories response: InternalServerError - user does not have categories neither in household nor in user detailed info");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.InternalServerError,
                    value: "Cannot get categories"
                    );
            }

            return req.CreateResponse(HttpStatusCode.OK, dtoResponse);
        }
    }
}
