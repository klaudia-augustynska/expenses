using Expenses.Model;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Api.Categories
{
    static class GetCategoriesDtoBuilder
    {
        public static async Task<GetCategoriesResponseDto> Build(
            UserLogInData entity,
            CloudTable table)
        {
            var dtoResponse = new GetCategoriesResponseDto();
            var householdId = entity.HouseholdId;
            var retrieveHouseholdOperation = TableOperation.Retrieve<Household>(householdId, householdId);
            var householdResult = await table.ExecuteAsync(retrieveHouseholdOperation);
            if (householdResult.Result != null)
            {
                var household = householdResult.Result as Household;
                var categories = JsonConvert.DeserializeObject<List<Category>>(household.CategoriesAggregated);
                dtoResponse.Categories = categories;
            }
            else
            {
                var retrieveUsersOwnCategories = TableOperation.Retrieve<UserDetails>(householdId, entity.PartitionKey);
                var userDetailsResult = await table.ExecuteAsync(retrieveUsersOwnCategories);
                if (userDetailsResult.Result != null)
                {
                    var userDetails = userDetailsResult.Result as UserDetails;
                    var categories = JsonConvert.DeserializeObject<List<Category>>(userDetails.Categories);
                    dtoResponse.Categories = categories;
                }
                else
                {
                    return null;
                }
            }
            return dtoResponse;
        }
    }
}
