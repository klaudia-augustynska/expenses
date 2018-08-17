using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Expenses.Model;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Expenses.Api.CashFlows
{
    public static class DequeueAddCashflow
    {
        [FunctionName("DequeueAddCashflow")]
        public static async Task Run(
            [QueueTrigger("expenses-addcashflow")]string myQueueItem,
            [Table("ExpensesApp")] CloudTable table,
            TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            var dto = JsonConvert.DeserializeObject<AddMessageToAddCashflowQueueDto>(myQueueItem);

            var retrieveHousehold = TableOperation.Retrieve<Household>(dto.HouseholdPk, dto.HouseholdRk);
            var operationResult = table.Execute(retrieveHousehold);
            if (operationResult.Result != null && operationResult.Result is Household)
            {
                var household = operationResult.Result as Household;

                log.Info("DequeueAddCashflow - household retreived");


                TableBatchOperation tableOperations = new TableBatchOperation();

                UpdateHousehold(household, dto, log, tableOperations);

                log.Info("DequeueAddCashflow - household updated");

                UpdateUsersDetails(household, dto, table, log, tableOperations);
                

                await table.ExecuteBatchAsync(tableOperations);

                log.Info("DequeueAddCashflow - users from the household updated");
            }
            log.Info("end of DequeueAddCashflow");
        }

        /// <summary>
        /// tu wystarczy zdj¹æ sumê tego co maj¹ wszyscy
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="log"></param>
        private static void UpdateHousehold(Household household, AddMessageToAddCashflowQueueDto dto, TraceWriter log, TableBatchOperation tableOperations)
        {
            var moneyList = JsonConvert.DeserializeObject<List<Money>>(household.MoneyAggregated);
            var money = moneyList.First(x => x.Currency == dto.Amount.Currency);
            money.Amount -= dto.Amount.Amount;
            household.MoneyAggregated = JsonConvert.SerializeObject(moneyList);

            var updateOperation = TableOperation.Replace(household);
            tableOperations.Add(updateOperation);
        }

        /// <summary>
        /// tu trzeba zastosowaæ przelicznik
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="table"></param>
        /// <param name="log"></param>
        private static void UpdateUsersDetails(Household household, AddMessageToAddCashflowQueueDto dto, CloudTable table, TraceWriter log, TableBatchOperation tableOperations)
        {
            var categories = JsonConvert.DeserializeObject<List<Category>>(household.CategoriesAggregated);

            List<UserDetails> userDetailsList = GetUserDetailsList(household.PartitionKey, table, log);

            Dictionary<string, Dictionary<string, decimal>> result = WhoPaysToWho(dto, categories, userDetailsList, log);


            var personWhoPay = userDetailsList.First(x => x.Wallets.Contains(dto.WalletGuid.ToString()));
            var wallets = JsonConvert.DeserializeObject<List<Wallet>>(personWhoPay.Wallets);
            var wallet = wallets.First(x => x.Guid == dto.WalletGuid);
            wallet.Money.Amount -= dto.Amount.Amount;
            personWhoPay.Wallets = JsonConvert.SerializeObject(wallets);

            foreach (var userDetails in userDetailsList)
            {
                if (result.ContainsKey(userDetails.Login))
                {
                    var dictWithUpdates = result[userDetails.Login];
                    var chargesList = JsonConvert.DeserializeObject<Dictionary<string, List<Money>>>(userDetails.Charges);
                    bool anyChange = false;
                    foreach (var keyvalue in dictWithUpdates)
                    {
                        if (!chargesList.ContainsKey(keyvalue.Key))
                        {
                            chargesList.Add(keyvalue.Key, new List<Money>());
                        }
                        var listItem = chargesList[keyvalue.Key].FirstOrDefault(x => x.Currency == dto.Amount.Currency);
                        if (listItem == null)
                        {
                            chargesList[keyvalue.Key].Add(new Money()
                            {
                                Amount = keyvalue.Value,
                                Currency = dto.Amount.Currency
                            });
                        }
                        else
                        {
                            listItem.Amount += keyvalue.Value;
                        }
                        anyChange = true;
                    }
                    if (anyChange || userDetails == personWhoPay)
                    {
                        userDetails.Charges = JsonConvert.SerializeObject(chargesList);
                        var operation = TableOperation.Replace(userDetails);
                        tableOperations.Add(operation);
                    }
                }
            }

        }

        private static List<UserDetails> GetUserDetailsList(string partitionKey, CloudTable table, TraceWriter log)
        {
            var pkFilter = TableQuery.GenerateFilterCondition(nameof(Household.PartitionKey), QueryComparisons.Equal, partitionKey);
            var rkFilter1 = TableQuery.GenerateFilterCondition(nameof(Household.RowKey), QueryComparisons.GreaterThan, "user_");
            var rkFilter2 = TableQuery.GenerateFilterCondition(nameof(Household.RowKey), QueryComparisons.LessThan, "user`");
            var combined = TableQuery.CombineFilters(
                    TableQuery.CombineFilters(pkFilter, TableOperators.And, rkFilter1),
                    TableOperators.And,
                    rkFilter2
                );
            var query = new TableQuery<UserDetails>().Where(combined);

            var result = table.ExecuteQuery(query);
            return result.ToList();
        }

        /// <summary>
        /// returns dictionary with compound key of two logins and the value is how much one login is charged to other login
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="categories"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, decimal>> WhoPaysToWho(AddMessageToAddCashflowQueueDto dto, List<Category> categories, List<UserDetails> userDetailsList, TraceWriter log)
        {
            var howMuchWasSpentInEachCategory = HowMuchWasSpentInEachCategory(categories, dto);
            var guidAsString = dto.WalletGuid.ToString();
            var personWhoPaid = userDetailsList.First(x => x.Wallets.Contains(guidAsString));

            Dictionary<string, decimal> loginAndHowMuchShouldPay = new Dictionary<string, decimal>();
            foreach (var userDetails in userDetailsList)
            {
                var login = userDetails.Login;
                loginAndHowMuchShouldPay.Add(
                    key: login,
                    value: CalculateHowMuchUserShouldPay(howMuchWasSpentInEachCategory, login, log));
            }

            // actually, the `personWhoPaid` paid 100% and the rest paid 0
            // so the others are in debt to `personWhoPaid`


            var result = new Dictionary<string, Dictionary<string, decimal>>();
            var loginAndHowMuchShouldPayWithoutPersonWhoPaid = loginAndHowMuchShouldPay.Where(x => x.Key != personWhoPaid.Login).ToDictionary(i => i.Key, i => i.Value);
            result.Add(personWhoPaid.Login, loginAndHowMuchShouldPayWithoutPersonWhoPaid);
            foreach (var keyvalue in loginAndHowMuchShouldPayWithoutPersonWhoPaid)
            {
                var key = keyvalue.Key;
                var debtDictionary = new Dictionary<string, decimal>()
                {
                    { personWhoPaid.Login , loginAndHowMuchShouldPay[key] * -1 }
                };
                result.Add(key, debtDictionary);
            }
            return result;
        }
        
        /// <summary>
        /// how much was spent in each category
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        private static Dictionary<Category, decimal> HowMuchWasSpentInEachCategory(List<Category> categories, AddMessageToAddCashflowQueueDto dto)
        {
            var categoryDict = new Dictionary<Category, decimal>();
            foreach (var category in categories)
            {
                categoryDict.Add(category, 0);
            }
            var generalCategory = categories.First(x => x.Guid == dto.CategoryGuid);
            categoryDict[generalCategory] = dto.Amount.Amount;
            if (dto.Details != null)
            {
                foreach (var detail in dto.Details)
                {
                    categoryDict[generalCategory] -= detail.Amount;
                    var detailCategory = categories.First(x => x.Guid == detail.CategoryGuid);
                    categoryDict[detailCategory] += detail.Amount;
                }
            }
            return categoryDict;
        }

        private static decimal CalculateHowMuchUserShouldPay(Dictionary<Category, decimal> categoryDict, string login, TraceWriter log)
        {
            decimal amountForThisUser = 0;
            decimal oneHundred = 100;
            foreach (var keyValuePair in categoryDict)
            {
                var category = keyValuePair.Key;
                var value = keyValuePair.Value;
                var factor = (decimal)category.Factor[login];
                amountForThisUser += value * factor / oneHundred;
            }
            return amountForThisUser;
        }
    }
}
