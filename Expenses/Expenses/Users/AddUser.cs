using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Expenses.Common;
using Expenses.Model.Dto;
using Expenses.Model.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Expenses.Api.Users
{
    public static class AddUser
    {
        [FunctionName("AddUser")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger( 
                AuthorizationLevel.Anonymous,
                "post", 
                Route = "users/add/{login}/")
            ]HttpRequestMessage req, 
            string login,
            [Table("ExpensesApp")]ICollector<User> outTable,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] User entity,
            TraceWriter log)
        {
            log.Info("Request to AddUser");

            var dbUser = $"user_{login}";
            AddUserDto addUserDto = null;
            try
            {
                addUserDto = await req.Content.ReadAsDeserializedJson<AddUserDto>();
            }
            catch
            {
                log.Info("AddUser response: BadRequest - cannot read dto object");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a valid dto object in the request content");
            }

            if (login == null)
            {
                log.Info("AddUser response: BadRequest - login is null");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass a login on the query string or in the request body");
            }
            if (addUserDto == null
                || string.IsNullOrWhiteSpace(addUserDto.HashedPassword)
                || string.IsNullOrWhiteSpace(addUserDto.Salt))
            {
                log.Info("AddUser response: BadRequest - dto object content is not valid");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "Please pass correct values to the dto object");
            }
            if (entity != null)
            {
                log.Info($"AddUser response: BadRequest - entity with PK={dbUser} and RK={dbUser} already exists");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login already exists"
                    );
            }

            var key = await RequestANewKeyForUser(dbUser, log);

            outTable.Add(new User()
            {
                PartitionKey = dbUser,
                RowKey = dbUser,
                Login = login,
                PasswordHash = addUserDto.HashedPassword,
                Key = key,
                Salt = addUserDto.Salt
            });

            log.Info($"AddUser response: Created - entity with PK={dbUser} and RK={dbUser}");
            return req.CreateResponse(HttpStatusCode.Created);
        }

        /// <summary>
        /// https://github.com/Azure/azure-functions-host/wiki/Key-management-API#post
        /// </summary>
        /// <param name="dbUser">user name stored as PK, used for naming the keys</param>
        /// <returns></returns>
        private static async Task<string> RequestANewKeyForUser(string dbUser, TraceWriter log)
        {
            var hostAddress = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            var keyname = $"key_{dbUser}";
            var uri = new Uri("http://" + hostAddress)
                .Append($"/admin/host/keys/{keyname}");
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(uri, content: null);
                    if (response.StatusCode == HttpStatusCode.Created
                        || response.StatusCode == HttpStatusCode.OK)
                    {
                        return await Task.Run(async () =>
                        {
                            var content = await response.Content
                                .ReadAsDeserializedJson<ResponseForCreatingOrUpdatingTheKey>();
                            return content.value;
                        });
                    }
                    else
                    {
                        throw new Exception($"Service responsible for generating keys responded with StatusCode = {response.StatusCode.ToString()}");
                    }
                }
                throw new Exception("Unspecified error occured while generating new key");
            }
            catch (Exception ex)
            {
                log.Error($"An error occured while generating a new key for the user {dbUser}", ex);
                throw;
            }
        }

        /// <summary>
        /// http://json2csharp.com/
        /// </summary>
        public class ResponseForCreatingOrUpdatingTheKey
        {
            public string name { get; set; }
            public string value { get; set; }
            public List<Link> links { get; set; }

            public class Link
            {
                public string rel { get; set; }
                public string href { get; set; }
            }
        }
    }
}
