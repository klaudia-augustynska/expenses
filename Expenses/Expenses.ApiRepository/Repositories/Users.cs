using Expenses.ApiRepository.Interfaces;
using Expenses.Common;
using System.Net.Http;
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

        public async Task<HttpResponseMessage> LogIn(string login, string password)
        {
            var uri = BaseUri.Append("login", login, password);
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetAsync(uri);
            }
        }
    }
}
