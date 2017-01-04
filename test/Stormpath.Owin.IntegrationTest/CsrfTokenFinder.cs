using System;

namespace Stormpath.Owin.IntegrationTest
{
    public class CsrfTokenFinder
    {
        public CsrfTokenFinder(string source)
        {
            FindToken(source);
        }

        public string Token { get; private set; }

        private void FindToken(string source)
        {
            const string preamble = "type=\"hidden\" value=\"";

            var startIndex = source.IndexOf(preamble, StringComparison.Ordinal) + preamble.Length;
            var endIndex = source.IndexOf("\"/>", startIndex, StringComparison.Ordinal) - startIndex;
            Token = source.Substring(startIndex, endIndex);
        }
    }
}
