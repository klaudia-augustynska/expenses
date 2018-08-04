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

        public async Task<HttpResponseMessage> Add(string login, string hashedPassword, string salt)
        {
            var hashedPasswordConverted = HashUtil.CutOffQueryStringSpecialCharacters(hashedPassword);
            var saltConverted = HashUtil.CutOffQueryStringSpecialCharacters(salt);
            var uri = BaseUri.Append("add", login, hashedPasswordConverted, saltConverted);
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetAsync(uri);
            }
        }

        public async Task<HttpResponseMessage> LogIn(string login, string hashedPassword)
        {
            var uri = BaseUri.Append("login", login, hashedPassword);
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetAsync(uri);
            }
        }

        public async Task<HttpResponseMessage> GetSalt(string login)
        {
            var uri = BaseUri.Append("salt", login);
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetAsync(uri);
            }
        }
    }
}
