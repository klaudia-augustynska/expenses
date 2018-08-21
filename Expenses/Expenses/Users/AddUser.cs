using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
            [Table("ExpensesApp")]ICollector<UserLogInData> outTable,
            [Table("ExpensesApp", "user_{login}", "user_{login}")] UserLogInData entity,
            TraceWriter log)
        {
            log.Info("Request to AddUser");

            var dbUser = $"user_{login}";
            var householdId = $"household_{login}";
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
                log.Info($"AddUser response: BadRequest - entity with PK={entity.PartitionKey} and RK={entity.RowKey} already exists");
                return req.CreateResponse(
                    statusCode: HttpStatusCode.BadRequest,
                    value: "User with given login already exists"
                    );
            }

            var key = await RequestANewKeyForUser(dbUser, log);

            var newUser = new UserLogInData()
            {
                PartitionKey = dbUser,
                RowKey = dbUser,
                Login = login,
                PasswordHash = addUserDto.HashedPassword,
                Key = key,
                Salt = addUserDto.Salt,
                HouseholdId = householdId,
                BelongsToGroup = false
            };
            outTable.Add(newUser);

            log.Info($"AddUser response: Created - entity with PK={newUser.PartitionKey} and RK={newUser.RowKey}");
            return req.CreateResponse(HttpStatusCode.Created);
        }

        /// <summary>
        /// https://github.com/Azure/azure-functions-host/wiki/Key-management-API#post
        /// </summary>
        /// <param name="dbUser">user name stored as PK, used for naming the keys</param>
        /// <returns></returns>
        private static async Task<string> RequestANewKeyForUser(string dbUser, TraceWriter log)
        {
            var protocol = FunctionRunsLocally ? "http://" : "https://";
            var site = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
            var keyname = $"key_{dbUser}";
            var username = "klaudiaaugustynska";
            var password = "malybiednymis123";
            var base64Auth = Convert.ToBase64String(Encoding.Default.GetBytes($"{username}:{password}"));
            var siteUrl = new Uri(protocol + Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME"));
            string JWT = null;

            if (!FunctionRunsLocally)
            {
                var apiUrl = new Uri($"https://{site}.scm.azurewebsites.net/api");

                Uri jwtEndpoint = apiUrl.Append("functions", "admin", "token");
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        log.Info("Request to Kudu API for a token");
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {base64Auth}");
                        var response = await httpClient.GetAsync(jwtEndpoint);
                        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                        {
                            JWT = response.Content.ReadAsStringAsync().Result.Trim('"');
                        }
                        else
                        {
                            throw new Exception($"Status code = {response.StatusCode}, message = {response.RequestMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Failed to get token at endpoint {jwtEndpoint}", ex);
                    throw;
                }

                if (string.IsNullOrEmpty(JWT))
                {
                    log.Error("Downloaded JWT token but it's empty");
                    throw new Exception("Downloaded JWT token but it's empty");
                }
            }

            Uri endpoint = siteUrl.Append($"/admin/host/keys/{keyname}");
            try
            {
                using (var httpClient = new HttpClient())
                {
                    log.Info("Request to Key management API for a new key");
                    if (!FunctionRunsLocally)
                        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + JWT);
                    var response = await httpClient.PostAsync(endpoint, null);
                    if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseDto = await response.Content.ReadAsDeserializedJson<ResponseForCreatingOrUpdatingTheKey>();
                        return responseDto.value;
                    }
                    else
                    {
                        throw new Exception($"Status code = {response.StatusCode}, message = {response.RequestMessage}");
                    }
                }
            } catch (Exception ex)
            {
                log.Error($"Failed to get a new key at endpoint {endpoint}", ex);
                throw;
            }
        }

        private static bool FunctionRunsLocally => 
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

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
