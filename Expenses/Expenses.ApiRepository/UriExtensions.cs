using System;
using System.Linq;

namespace Expenses.ApiRepository
{
    /// <summary>
    /// https://stackoverflow.com/a/7993235
    /// </summary>
    static class UriExtensions
    {
        public static Uri Append(this Uri uri, params string[] paths)
        {
            return new Uri(
                paths.Aggregate
                (
                    uri.AbsoluteUri, (current, path) => 
                        string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))
                )
            );
        }
    }
}
