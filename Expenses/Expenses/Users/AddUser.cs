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
            var apiUrl = new Uri($"https://{site}.scm.azurewebsites.net/api");
            var siteUrl = new Uri($"https://{site}.azurewebsites.net");
            string JWT;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    log.Info("Request to Kudu API for a token");
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {base64Auth}");
                    var result = httpClient.GetAsync($"{apiUrl}/functions/admin/token").Result;
                    JWT = result.Content.ReadAsStringAsync().Result.Trim('"');
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to get token at endpoint " + $"{apiUrl}/functions/admin/token", ex);
                throw;
            }

            if (string.IsNullOrEmpty(JWT))
            {
                log.Error("Downloaded JWT token but it's empty");
                throw new Exception("Downloaded JWT token but it's empty");
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    log.Info("Request to Key management API for a new key");
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + JWT);
                    var result = httpClient.GetAsync($"{siteUrl}/admin/functions/test/keys").Result;
                    var responseDto = await result.Content.ReadAsDeserializedJson<ResponseForCreatingOrUpdatingTheKey>();
                    return responseDto.value;
                }
            } catch (Exception ex)
            {
                log.Error("Failed to get a new key at endpoint " + $"{siteUrl}/admin/functions/test/keys", ex);
                throw;
            }
        }

        private static bool FunctionRunsLocally => 
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

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
