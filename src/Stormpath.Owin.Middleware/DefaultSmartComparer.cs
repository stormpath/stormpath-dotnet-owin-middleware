using System;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware
{
    internal sealed class DefaultSmartComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            if (x.Equals(y))
            {
                return true;
            }

            if (x is string && y is string)
            {
                return CompareStringsOrdinal(x, y);
            }

            if (IsIntegerLike(x) && IsIntegerLike(y))
            {
                return IntegersEquals(x, y);
            }

            return false;
        }

        public int GetHashCode(object obj)
            => obj.GetHashCode();

        /// <summary>
        /// By default, always compare strings with an Ordinal comparison.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The second string.</param>
        /// <returns><c>true</c> if the strings are equal.</returns>
        private static bool CompareStringsOrdinal(object x, object y)
            => ((string)x).Equals((string)y, StringComparison.Ordinal);

        private static bool IsIntegerLike(object value)
            => value is byte || value is short || value is int || value is long;

        /// <summary>
        /// Compares two integer-like types, regardless of the size of the underlying type.
        /// </summary>
        /// <remarks>
        /// This is necessary because the Stormpath SDK deserializes integers as <c>long</c> under the hood.
        /// </remarks>
        /// <param name="obj1">An integer-like type.</param>
        /// <param name="obj2">An integer-like type.</param>
        /// <returns><c>true</c> if the values are identical.</returns>
        private static bool IntegersEquals(object obj1, object obj2)
        {
            var long1 = (long)Convert.ChangeType(obj1, typeof(long));
            var long2 = (long)Convert.ChangeType(obj2, typeof(long));
            return long1 == long2;
        }
    }
}