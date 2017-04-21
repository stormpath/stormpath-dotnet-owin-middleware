using System;
using System.Security.Cryptography;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class CodeGenerator
    {
        public static string GetCode()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenData = new byte[26];
                rng.GetBytes(tokenData);

                return Convert.ToBase64String(tokenData)
                    .Replace("=", string.Empty)
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }
    }
}
