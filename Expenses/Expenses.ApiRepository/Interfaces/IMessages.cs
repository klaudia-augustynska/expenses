using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository.Interfaces
{
    public interface IMessages
    {
        Task<HttpResponseMessage> GetNew(string login, DateTime dateFrom, string key);
    }
}
