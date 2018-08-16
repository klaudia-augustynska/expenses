using Expenses.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.ApiRepository.Interfaces
{
    public interface ICashFlows
    {
        Task<HttpResponseMessage> Add(string login, string key, AddCashFlowDto dto);
        Task<HttpResponseMessage> GetSummary(string householdId, string login, DateTime dateFrom, DateTime dateTo, string key);
    }
}
