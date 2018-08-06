using Expenses.ApiRepository.Interfaces;
using Expenses.Common;
using Expenses.Model.Dto;
using Newtonsoft.Json;
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
            var uri = BaseUri.Append("add", login);
            var content = new StringContent(JsonConvert.SerializeObject(new AddUserDto()
            {
                HashedPassword = hashedPassword,
                Salt = salt
            }));
            using (var httpClient = new HttpClient())
            {
                return await httpClient.PostAsync(uri, content);
            }
        }

        public async Task<HttpResponseMessage> LogIn(string login, string hashedPassword)
        {
            var uri = BaseUri.Append("login", login);
            var content = new StringContent(JsonConvert.SerializeObject(new LogInDto()
            {
                HashedPassword = hashedPassword
            }));
            using (var httpClient = new HttpClient())
            {
                return await httpClient.PostAsync(uri, content);
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

        public async Task<HttpResponseMessage> Delete(string login, string key)
        {
            var uri = BaseUri.Append("delete", login);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.GetAsync(uri);
            }
        }

        public async Task<HttpResponseMessage> ConfigureUser(string login, string key, ConfigureUserDto configureUserDto)
        {
            var uri = BaseUri.Append("configure", login);
            var content = new StringContent(JsonConvert.SerializeObject(configureUserDto));
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.PostAsync(uri, content);
            }
        }
    }
}
