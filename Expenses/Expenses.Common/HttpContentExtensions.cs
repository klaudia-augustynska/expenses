using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Common
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsDeserializedJson<T>(this HttpContent httpContent) where T : class
        {
            var contentString = await httpContent.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(contentString);
        }
    }
}
