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
    class Categories : RepositoryBase, ICategories
    {
        public Categories(string host, string path, string apiRepositoryName = "categories") 
            : base(host, path, apiRepositoryName)
        {
        }

        public async Task<HttpResponseMessage> GetAll(string login, string key)
        {
            var uri = BaseUri.Append("getall", login);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.GetAsync(uri);
            }
        }

        public async Task<HttpResponseMessage> Add(string login, string key, string categoryName, Dictionary<string,double> factor = null)
        {
            var uri = BaseUri.Append("getall", login);
            var dto = new AddCategoryDto()
            {
                Name = categoryName,
                Factor = factor
            };
            var content = new StringContent(JsonConvert.SerializeObject(dto));
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.PostAsync(uri, content);
            }
        }
    }
}
