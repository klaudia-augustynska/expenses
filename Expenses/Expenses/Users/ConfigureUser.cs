using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Expenses.Model.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Expenses.Api.Users
{
    public static class ConfigureUser
    {
        [FunctionName("ConfigureUser")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "post", 
                Route = "users/configure/{login}/")
            ]HttpRequestMessage req,
            string login,
            [Table("ExpensesApp")] CloudTable table,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] UserLogInData entity,
            TraceWriter log)
        {
            log.Info("Request to ConfigureUser");

            ConfigureUserDto dto = null;
            try
            {
                dto = await req.Content.ReadAsDeserializedJson<ConfigureUserDto>();
            }
            catch
            {
                log.Info("ConfigureUser response: BadRequest - cannot read dto object");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a valid dto object in the request content");
            }
            if (login == null)
            {
                log.Info("ConfigureUser response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }
            if (entity == null)
            {
                log.Info($"ConfigureUser response: BadRequest - no such user");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist"
                    );
            }

            var userDetails = new UserDetails()
            {
                PartitionKey = entity.HouseholdId,
                RowKey = entity.PartitionKey,
                Height = dto.Height,
                Name = dto.Name,
                Sex = dto.Sex == Model.Enums.Sex.Female ? true : false,
                Wallets = JsonConvert.SerializeObject(dto.Wallets),
                Weight = dto.Weight,
                Login = entity.Login,
                Categories = JsonConvert.SerializeObject(DefaultCategoryUtils.GetDefaultCategories(entity.Login)),
                DateOfBirth = dto.DateOfBirth,
                Pal = dto.Pal,
                Charges = JsonConvert.SerializeObject(new Dictionary<string, List<Money>>())
            };
            TableOperation insertTableOperation = TableOperation.Insert(userDetails);
            await table.ExecuteAsync(insertTableOperation);
            
            var dbUser = $"user_{login}";
            log.Info($"ConfigureUser response: Updated entity with PK={userDetails.PartitionKey} and RK={userDetails.RowKey}");
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
