using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
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
    public static class AddCategory
    {
        [FunctionName("AddCategory")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "post", 
                Route = "categories/add/{login}/")
            ]HttpRequestMessage req,
            string login,
            [Table("ExpensesApp")] CloudTable table,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] UserLogInData entity,
            TraceWriter log)
        {
            if (entity == null)
            {
                log.Info($"AddCategory response: BadRequest - no such user");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login does not exist"
                    );
            }


            AddCategoryDto dto = null;
            try
            {
                dto = await req.Content.ReadAsDeserializedJson<AddCategoryDto>();
            }
            catch
            {
                log.Info("AddCategory response: BadRequest - cannot read dto object");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a valid dto object in the request content");
            }


            var newCategory = new Category()
            {
                Name = dto.Name,
                Factor = dto.Factor
            };

            var householdId = entity.HouseholdId;
            var retrieveHouseholdOperation = TableOperation.Retrieve<Household>(householdId, householdId);
            var householdResult = await table.ExecuteAsync(retrieveHouseholdOperation);
            if (householdResult.Result != null)
            {
                var household = householdResult.Result as Household;

                if (newCategory.Factor == null)
                {
                    var members = JsonConvert.DeserializeObject<List<Member>>(household.Members);
                    var factor = new Dictionary<string, double>();
                    foreach (var member in members)
                    {
                        if (member.Login == login)
                        {
                            factor.Add(member.Login, 100);
                        }
                        else
                        {
                            factor.Add(member.Login, 0);
                        }
                    }
                    newCategory.Factor = factor;
                }

                var categories = JsonConvert.DeserializeObject<List<Category>>(household.CategoriesAggregated);
                categories.Add(newCategory);
                household.CategoriesAggregated = JsonConvert.SerializeObject(categories);
                var updateTableOperation = TableOperation.Replace(household);
                await table.ExecuteAsync(updateTableOperation);
            }
            else
            {
                var retrieveUsersOwnCategories = TableOperation.Retrieve<UserDetails>(householdId, entity.PartitionKey);
                var userDetailsResult = await table.ExecuteAsync(retrieveUsersOwnCategories);
                if (userDetailsResult.Result != null)
                {
                    var userDetails = userDetailsResult.Result as UserDetails;
                    var categories = JsonConvert.DeserializeObject<List<Category>>(userDetails.Categories);
                    
                    if (newCategory.Factor == null)
                    {
                        newCategory.Factor = new Dictionary<string, double>()
                        {
                            { login, 100 }
                        };
                    }

                    categories.Add(newCategory);
                    userDetails.Categories = JsonConvert.SerializeObject(categories);
                    var updateTableOperation = TableOperation.Replace(userDetails);
                    await table.ExecuteAsync(updateTableOperation);
                }
                else
                {
                    log.Info($"AddCategory response: InternalServerError - user does not have categories neither in household nor in user detailed info");
                    return req.CreateResponse(
                        statusCode: HttpStatusCode.InternalServerError,
                        value: "Cannot get categories"
                        );
                }
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
