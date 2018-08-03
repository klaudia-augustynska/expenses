using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository
{
    public class Users
    {
        public void Add(string login, string password)
        {
            var url = $"http://localhost:7071/api/users/add/{login}/{password}";
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetStringAsync(new Uri(url)).Result;

                var releases = JArray.Parse(response);
            }
        }
    }
}
