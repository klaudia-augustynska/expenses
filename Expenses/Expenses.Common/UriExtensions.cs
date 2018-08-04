using System;
using System.Linq;

namespace Expenses.Common
{
    /// <summary>
    /// https://stackoverflow.com/a/7993235
    /// </summary>
    public static class UriExtensions
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
