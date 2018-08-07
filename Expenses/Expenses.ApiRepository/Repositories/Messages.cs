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
    class Messages : RepositoryBase, IMessages
    {
        public Messages(string host, string path) 
            : base(host, path, "messages")
        {
        }

        public async Task<HttpResponseMessage> GetNew(string login, DateTime dateFrom, string key)
        {
            var uri = BaseUri.Append("getnew", login);
            var content = new StringContent(JsonConvert.SerializeObject(new GetNewMessagesDto()
            {
                DateTime = dateFrom
            }));
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.PostAsync(uri, content);
            }
        }
    }
}
