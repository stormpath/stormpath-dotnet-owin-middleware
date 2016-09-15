using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.IntegrationTest
{
    public class OwinTestFixture
    {
        public OwinTestFixture()
        {
            TestKey = Guid.NewGuid().ToString();
        }

        public StormpathOwinOptions Options { get; set; }

        public IClient Client { get; private set; }

        public string ApplicationHref { get; private set; }

        public string TestKey { get; }

        public void Configure(IApplicationBuilder app)
        {
            var options = Options ?? new StormpathOwinOptions();
            options.LibraryUserAgent = "Stormpath.Owin.IntegrationTest";
            options.ViewRenderer = new NullViewRenderer();

            var stormpathMiddleware = StormpathMiddleware.Create(options);
            Client = stormpathMiddleware.GetClient();
            ApplicationHref = stormpathMiddleware.Configuration.Application.Href;

            app.UseOwin(addToPipeline =>
            {
                addToPipeline(next =>
                {
                    stormpathMiddleware.Initialize(next);
                    return stormpathMiddleware.Invoke;
                });
            });

            app.Run(async (context) =>
            {
                if (context.Request.Path == "/")
                {
                    await context.Response.WriteAsync("Hello World!");
                }
            });
        }
    }
}
