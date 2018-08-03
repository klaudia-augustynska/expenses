using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository.Interfaces
{
    public interface IUsers
    {
        /// <summary>
        /// HttpStatusCode.Created jeśli sukces
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns>HttpStatusCode.Created jeśli sukces</returns>
        Task<HttpResponseMessage> Add(string login, string password);
    }
}
