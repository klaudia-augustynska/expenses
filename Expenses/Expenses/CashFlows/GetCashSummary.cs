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
            log.Info($"GetCashSummary function processed a request for login: {login} and period from {dateFrom} to {dateTo}.");

            if (householdId == null)
            {
                log.Info("GetCashSummary response: BadRequest - householdId is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a householdId on the query string or in the request body");
            }
            if (login == null)
            {
                log.Info("GetCashSummary response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }
            if (dateFrom == null)
            {
                log.Info("GetCashSummary response: BadRequest - dateFrom is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a dateFrom on the query string or in the request body");
            }
            if (dateTo == null)
            {
                log.Info("GetCashSummary response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a dateTo on the query string or in the request body");
            }

            var responseDto = new GetCashSummaryResponseDto()
            {
                UserWallets = JsonConvert.DeserializeObject <List<Wallet>>(userDetails.Wallets),
                UserExpenses = GetUserExpenses(dateFrom, dateTo, cashflows, login, householdId, log),
                UserCharges = JsonConvert.DeserializeObject<Dictionary<string, List<Money>>>(userDetails.Charges)
            };
            if (household != null)
            {
                responseDto.HouseholdMoney = JsonConvert.DeserializeObject<List<Money>>(household.MoneyAggregated);
                responseDto.HouseholdExpenses = GetHouseholdExpenses(dateFrom, dateTo, cashflows, householdId, log);
            }

            return req.CreateResponse(HttpStatusCode.OK, responseDto);
        }

        private static List<Money> GetUserExpenses(string dateFrom, string dateTo, IQueryable<Cashflow> cashflows, string login, string pk, TraceWriter log)
        {
            var prefix = $"userCashflow_{login}";
            return GetExpenses(dateFrom, dateTo, cashflows, prefix, pk, log);
        }

        private static List<Money> GetHouseholdExpenses(string dateFrom, string dateTo, IQueryable<Cashflow> cashflows, string pk, TraceWriter log)
        {
            var prefix = "householdCashflow";
            return GetExpenses(dateFrom, dateTo, cashflows, prefix, pk, log);
        }

        static List<Money> GetExpenses(string dateFrom, string dateTo, IQueryable<Cashflow> cashflows, string prefix, string pk, TraceWriter log)
        {
            var dateFromInverted = $"{prefix}_{RowKeyUtils.ConvertInvertedDateToDateFrom(RowKeyUtils.InvertDateString(dateFrom))}";
            var dateToInverted = $"{prefix}_{RowKeyUtils.ConvertInvertedDateToDateTo(RowKeyUtils.InvertDateString(dateTo))}";
            log.Info($"GetCashSummary: getting expenses with prefix: {prefix} from date {dateFromInverted} to date {dateToInverted}");

            var list = cashflows
                .Where(x => x.PartitionKey.Equals(pk))
                .Where(x => x.RowKey.CompareTo(dateFromInverted) < 0)
                .Where(x => x.RowKey.CompareTo(dateToInverted) > 0)
                .ToList();

            log.Info($"GetCashSummary: for prefix {prefix} found {list.Count} rows.");

            var result = list
                .Select(x => JsonConvert.DeserializeObject<Money>(x.Amount))
                .GroupBy(x => x.Currency, x => x.Amount, (x, y) => new Money() { Currency = x, Amount = y.Sum() })
                .ToList();

            log.Info($"GetCashSummary: for prefix {prefix} result contains {result.Count} rows.");

            return result;
        }
    }
}
