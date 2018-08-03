using Expenses.ApiRepository.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository.Repositories
{
    class Users : RepositoryBase, IUsers
    {
        public Users(string host, string path)
            : base(host, path, apiRepositoryName: "users")
        {

        }

        public async Task<HttpResponseMessage> Add(string login, string password)
        {
            var uri = BaseUri.Append("add", login, password);
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetAsync(uri);
            }
        }
    }
}
