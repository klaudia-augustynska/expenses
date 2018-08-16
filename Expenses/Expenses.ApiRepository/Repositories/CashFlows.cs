using Expenses.ApiRepository.Interfaces;
using Expenses.Common;
using Expenses.Model.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    }
}
