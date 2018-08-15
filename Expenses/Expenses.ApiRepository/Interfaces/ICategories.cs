using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository.Interfaces
{
    public interface ICategories
    {
        Task<HttpResponseMessage> GetAll(string login, string key);

        Task<HttpResponseMessage> Add(string login, string key, string categoryName, Dictionary<string, double> factor = null);
    }
}
