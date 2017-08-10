using System;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class StringExtensions
    {
        public static string Join(this IEnumerable<string> stringsToJoin, string separator)
        {
            if (stringsToJoin == null)
            {
                throw new ArgumentNullException(nameof(stringsToJoin));
            }

            return string.Join(separator, stringsToJoin);
        }
    }
}
