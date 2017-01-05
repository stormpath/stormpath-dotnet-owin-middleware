using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;

namespace Stormpath.Owin.IntegrationTest
{
    public class CsrfToken
    {
        public static async Task<string> GetTokenForRoute(HttpClient server, string path)
        {
            var pageRequest = new HttpRequestMessage(HttpMethod.Get, path);
            pageRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            var pageResponse = await server.SendAsync(pageRequest);
            pageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            pageResponse.Content.Headers.ContentType.MediaType.Should().Be("text/html");

            var pageContent = await pageResponse.Content.ReadAsStringAsync();
            return FindTokenInHtml(pageContent);
        }

        private static string FindTokenInHtml(string source)
        {
            const string preamble = "type=\"hidden\" value=\"";

            var startIndex = source.IndexOf(preamble, StringComparison.Ordinal) + preamble.Length;
            var endIndex = source.IndexOf("\"/>", startIndex, StringComparison.Ordinal) - startIndex;
            return source.Substring(startIndex, endIndex);
        }
    }
}
