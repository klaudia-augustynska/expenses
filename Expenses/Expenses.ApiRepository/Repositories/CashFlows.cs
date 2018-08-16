using Expenses.ApiRepository.Interfaces;
using Expenses.Common;
using Expenses.Model.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository.Repositories
{
    class CashFlows : RepositoryBase, ICashFlows
    {
        public CashFlows(string host, string path, string apiRepositoryName = "cashflows") 
            : base(host, path, apiRepositoryName)
        {
        }

        public async Task<HttpResponseMessage> Add(string login, string key, AddCashFlowDto dto)
        {
            var uri = BaseUri.Append("add", login);
            var content = new StringContent(JsonConvert.SerializeObject(dto));
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.PostAsync(uri, content);
            }
        }

        public async Task<HttpResponseMessage> GetSummary(string householdId, string login, DateTime dateFrom, DateTime dateTo, string key)
        {
            var dateFormat = "yyyy.MM.dd";
            var dateFromString = dateFrom.ToString(dateFormat, CultureInfo.InvariantCulture);
            var dateToString = dateTo.ToString(dateFormat, CultureInfo.InvariantCulture);
            var uri = BaseUri.Append("summary", householdId, login, dateFromString, dateToString);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.GetAsync(uri);
            }
        }
    }
}
