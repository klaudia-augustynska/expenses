using System;
using System.Collections.Generic;
using System.Globalization;
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
using Newtonsoft.Json;

namespace Expenses.Api.CashFlows
{
    public static class GetCashSummary
    {
        [FunctionName("GetCashSummary")]
        public static HttpResponseMessage Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get", "post", 
                Route = "cashflows/summary/{householdId}/{login}/{dateFrom}/{dateTo}")
            ]HttpRequestMessage req,
            string householdId,
            string login,
            string dateFrom,
            string dateTo,
            [Table("ExpensesApp", "{householdId}", "{householdId}")] Household household,
            [Table("ExpensesApp", "{householdId}", "user_{login}")] UserDetails userDetails,
            [Table("ExpensesApp", "{householdId}")] IQueryable<Cashflow> cashflows,
            TraceWriter log)
        {
            log.Info("GetCashSummary function processed a request.");

            if (householdId == null)
            {
                log.Info("LogIn response: BadRequest - householdId is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a householdId on the query string or in the request body");
            }
            if (login == null)
            {
                log.Info("LogIn response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }
            if (dateFrom == null)
            {
                log.Info("LogIn response: BadRequest - dateFrom is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a dateFrom on the query string or in the request body");
            }
            if (dateTo == null)
            {
                log.Info("LogIn response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a dateTo on the query string or in the request body");
            }
            if (household == null)
            {
                log.Info($"LogIn response: BadRequest - given household does not exist");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Given household does not exist"
                    );
            }

            var responseDto = new GetCashSummaryResponseDto()
            {
                HouseholdMoney = JsonConvert.DeserializeObject<List<Money>>(household.MoneyAggregated),
                HouseholdExpenses = GetHouseholdExpenses(dateFrom, dateTo, cashflows),
                UserWallets = JsonConvert.DeserializeObject <List<Wallet>>(userDetails.Wallets),
                UserExpenses = GetUserExpenses(dateFrom, dateTo, cashflows, login)
            };

            return req.CreateResponse(HttpStatusCode.OK, responseDto);
        }

        private static List<Money> GetUserExpenses(string dateFrom, string dateTo, IQueryable<Cashflow> cashflows, string login)
        {
            var prefix = $"userCashflow_{login}";
            return GetExpenses(dateFrom, dateTo, cashflows, prefix);
        }

        private static List<Money> GetHouseholdExpenses(string dateFrom, string dateTo, IQueryable<Cashflow> cashflows)
        {
            var prefix = "householdCashflow";
            return GetExpenses(dateFrom, dateTo, cashflows, prefix);
        }

        private static List<Money> GetExpenses(string dateFrom, string dateTo, IQueryable<Cashflow> cashflows, string prefix)
        {
            var dateFromInverted = $"{prefix}_{InvertDateString(dateFrom)}";
            var dateToInverted = $"{prefix}_{InvertDateString(dateTo)}";

            var list = cashflows
                .Where(x => x.RowKey.CompareTo(dateFromInverted) < 0)
                .Where(x => x.RowKey.CompareTo(dateToInverted) > 0)
                .ToList();

            return list
                .Select(x => JsonConvert.DeserializeObject<Money>(x.Amount))
                .GroupBy(x => x.Currency, x => x.Amount, (x, y) => new Money() { Currency = x, Amount = y.Sum() })
                .ToList();
        }

        private static string InvertDateString(string date)
        {
            var dateTimeFormt = "yyyy.MM.dd";
            DateTime dateTimeFrom = DateTime.ParseExact(date, dateTimeFormt, CultureInfo.InvariantCulture);
            return RowKeyUtils.GetInvertedDateString(dateTimeFrom);
        }
    }
}
