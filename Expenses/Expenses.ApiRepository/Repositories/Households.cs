using Expenses.ApiRepository.Interfaces;
using Expenses.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository.Repositories
{
    class Households : RepositoryBase, IHouseholds
    {
        public Households(string host, string path, string apiRepositoryName = "households") 
            : base(host, path, apiRepositoryName)
        {
        }

        public async Task<HttpResponseMessage> Invite(string invitersLogin, string invitedLogin, string key)
        {
            var uri = BaseUri.Append("invite", invitersLogin, invitedLogin);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.GetAsync(uri);
            }
        }

        public async Task<HttpResponseMessage> AcceptInvitation(string from, string to, string rowKey, string key)
        {
            var uri = BaseUri.Append("accept", from, to, rowKey);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.GetAsync(uri);
            }
        }

        public async Task<HttpResponseMessage> GetMembers(string householdId, string key)
        {
            var uri = BaseUri.Append("members", householdId);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-functions-key", key);
                return await httpClient.GetAsync(uri);
            }
        }
    }
}
