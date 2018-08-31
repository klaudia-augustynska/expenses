using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Expenses.Api.Messages
{
    public static class GetNewMessages
    {
        [FunctionName("GetNewMessages")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get",
                "post", 
                Route = "messages/getnew/{login}/{dateFrom}")
            ]HttpRequestMessage req, 
            string login,
            string dateFrom,
            [Table("ExpensesApp", "message_{login}")] IQueryable<Message> messages,
            TraceWriter log)
        {
            if (login == null)
            {
                log.Info("GetNewMessages response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }
            if (dateFrom == null)
            {
                log.Info("GetNewMessages response: BadRequest - no datefrom specified");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "No datefrom specified");
            }

            try
            {
                var dateToCompare = RowKeyUtils.ConvertInvertedDateToDateFrom(RowKeyUtils.InvertDateTimeString(dateFrom));
                log.Info("GetNewMessages, date to compare is " + dateToCompare);
                var list = messages.Where(x => x.RowKey.CompareTo(dateToCompare) < 0).ToList();
                log.Info($"GetNewMessages, found {list.Count} items");
                var listDto = list.Select(x => new GetNewMessagesResponseDto()
                {
                    From = JsonConvert.DeserializeObject<UserShort>(x.From),
                    Topic = x.Topic,
                    Content = x.Content,
                    RowKey = x.RowKey
                }).ToList();
                return req.CreateResponse(
                    statusCode: HttpStatusCode.OK,
                    value: JsonConvert.SerializeObject(listDto));
            }
            catch (Exception ex)
            {
                log.Error("GetNewMessages response: InternalServerError - problem with loading messages", ex);
                return req.CreateResponse(
                    statusCode: HttpStatusCode.InternalServerError,
                    value: "Problem with loading messages");
            }
        }
    }
}
