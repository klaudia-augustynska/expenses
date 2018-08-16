using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void Run(
            [QueueTrigger("expensesaddcashflow")]string myQueueItem,
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

                UpdateHousehold(household, dto, table, log);

                log.Info("DequeueAddCashflow - household updated");

                UpdateUsersDetails(household, dto, table, log);

                log.Info("DequeueAddCashflow - users from the household updated");
            }
            log.Info("end of DequeueAddCashflow");
        }

        /// <summary>
        /// tu wystarczy zdj¹æ sumê tego co maj¹ wszyscy
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="table"></param>
        /// <param name="log"></param>
        private static void UpdateHousehold(Household household, AddMessageToAddCashflowQueueDto dto, CloudTable table, TraceWriter log)
        {
            var moneyList = JsonConvert.DeserializeObject<List<Money>>(household.MoneyAggregated);
            var money = moneyList.First(x => x.Currency == dto.Amount.Currency);
            money.Amount -= dto.Amount.Amount;
            household.MoneyAggregated = JsonConvert.SerializeObject(moneyList);

            var updateOperation = TableOperation.Replace(household);
            table.Execute(updateOperation);
        }

        /// <summary>
        /// tu trzeba zastosowaæ przelicznik
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="table"></param>
        /// <param name="log"></param>
        private static void UpdateUsersDetails(Household household, AddMessageToAddCashflowQueueDto dto, CloudTable table, TraceWriter log)
        {
            var members = JsonConvert.DeserializeObject<List<Member>>(household.Members);
            var categories = JsonConvert.DeserializeObject<List<Category>>(household.CategoriesAggregated);
            foreach (var member in members)
            {
                UpdateUserDetails(member.Login, categories, dto, table, log);
            }
        }

        private static void UpdateUserDetails(string login, List<Category> categories, AddMessageToAddCashflowQueueDto dto, CloudTable table, TraceWriter log)
        {
            decimal amountForThisUser = CalculateHowMuchEachUserShouldPay(categories, dto, login, log);
            
            var retrieveUserDetails = TableOperation.Retrieve<UserDetails>(dto.HouseholdPk, $"user_{login}");
            var operationResult = table.Execute(retrieveUserDetails);
            if (operationResult.Result != null && operationResult.Result is UserDetails)
            {
                var userDetails = operationResult.Result as UserDetails;
                var wallets = JsonConvert.DeserializeObject<List<Wallet>>(userDetails.Wallets);
                var wallet = wallets.First(x => x.Guid == dto.WalletGuid);
                wallet.Money.Amount -= amountForThisUser;
                userDetails.Wallets = JsonConvert.SerializeObject(wallets);

                var updateOperation = TableOperation.Replace(userDetails);
                table.Execute(updateOperation);
            }
        }

        private static decimal CalculateHowMuchEachUserShouldPay(List<Category> categories, AddMessageToAddCashflowQueueDto dto, string login, TraceWriter log)
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
            decimal amountForThisUser = 0;
            foreach (var category in categories)
            {
                decimal oneHundred = 100;
                var factor = (decimal)category.Factor[login];
                amountForThisUser += categoryDict[category] * factor / oneHundred;
            }
            return amountForThisUser;
        }
    }
}
