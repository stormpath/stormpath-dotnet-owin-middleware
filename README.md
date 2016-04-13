# Stormpath OWIN Middleware for .NET

This library provides middleware that plugs into any OWIN pipeline and makes it easy to add user authentication to your .NET application, with pre-built functionality for login, signup, authorization, and social login.

[Stormpath](https://stormpath.com) is a User Management API that reduces development time with instant-on, scalable user infrastructure. Stormpath's intuitive API and expert support make it easy for developers to authenticate, manage and secure users and roles in any application.

## Supported Platforms

* .NET Framework 4.5.1 and newer
* DNX Full CLR 4.5.1 (`dnx451`) and newer
* DNX Core CLR (`dotnet5.4`/`netstandard1.3`)

## Prebuilt Packages

### ASP.NET Core? Easy!
If you are building an application with ASP.NET Core 1.0, head over and grab the [Stormpath ASP.NET Core integration](https://github.com/stormpath/stormpath-aspnetcore) package instead, which includes this library and conveniently plugs right into the ASP.NET Core pipeline.

### Other Frameworks

We're working on support for [ASP.NET 4.x](https://github.com/stormpath/stormpath-dotnet-owin-middleware/issues/4) and [Nancy](https://github.com/stormpath/stormpath-dotnet-owin-middleware/issues/5) right now. If you'd like to be notified when those packages are released, subscribe to the linked issues or send an email to support@stormpath.com. In the meantime, follow the guide below, which covers adding this middleware to any OWIN-compatible pipeline.

## Quickstart

This example will demonstrate how to set up a web server using [Nowin](https://github.com/Bobris/Nowin), but the concepts apply to any OWIN-compatible pipeline.

1. **[Sign up](https://api.stormpath.com/register) for Stormpath**

2. **Get Your Key File**

  [Download your key file](https://support.stormpath.com/hc/en-us/articles/203697276-Where-do-I-find-my-API-key-) from the Stormpath Console.

3. **Store Your Key as Environment Variables**

  Open your key file and grab the **API Key ID** and **API Key Secret**, then run these commands in PowerShell (or the Windows Command Prompt) to save them as environment variables:

  ```
  setx STORMPATH_CLIENT_APIKEY_ID "[value-from-properties-file]"
  setx STORMPATH_CLIENT_APIKEY_SECRET "[value-from-properties-file]"
  ```

4. **Store Your Stormpath Application Href in an Environment Variable**

  Grab the `href` (called **REST URL** in the Stormpath Console UI) of your Application. It should look something like this:

  `https://api.stormpath.com/v1/applications/q42unYAj6PDLxth9xKXdL`

  Save this as an environment variable:

  ```
  setx STORMPATH_APPLICATION_HREF "[your Application href]"
  ```

5. **Set Up a Server**

  Skip this step if you are adding Stormpath to an existing application.

  If you don't have an OWIN server set up already, install these packages to get started with a simple [Nowin](https://github.com/Bobris/Nowin) server:

  ```
  PM> install-package Microsoft.Owin.Hosting
  PM> install-package Nowin
  ```

  Add a `Program` class to your project:

  ```csharp
  using System;
  using System.IO;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.Owin.Hosting;
  using Owin;
  using Stormpath.Owin.Middleware;
  using Stormpath.Owin.Common;

  namespace MyServer
  {
      using AppFunc = Func<IDictionary<string, object>, Task>;

      static class Program
      {
          static void Main(string[] args)
          {
              var options = new StartOptions
              {
                  ServerFactory = "Nowin",
                  Port = 8080,
              };

              using (WebApp.Start<Startup>(options))
              {
                  Console.WriteLine("Running a http server on port 8080");
                  Console.ReadKey();
              }
          }
      }

      public class Startup
      {
          public void Configuration(IAppBuilder app)
          {
              // Add a sample middleware that responds to GET /foo
              app.Use(new Func<AppFunc, AppFunc>(next => (async env =>
              {
                  if (env["owin.RequestPath"] as string == "/foo")
                  {
                      using (var writer = new StreamWriter(env["owin.ResponseBody"] as Stream))
                      {
                          await writer.WriteAsync("<h1>Hello from OWIN!</h1>");
                          await writer.FlushAsync();
                      }
                  }

                  await next.Invoke(env);
              })));
          }
      }
  }
  ```

6. **Install the Middleware Packages**

    Using the Package Explorer or the Package Manager Console, install the `Stormpath.Owin.Middleware` and `Stormpath.Owin.Views.Precompiled` packages:

    ```
    PM> install-package Stormpath.Owin.Middleware
    PM> install-package Stormpath.Owin.Views.Precompiled
    ```

7. **Initialize the Stormpath Middleware**

  At the **top** of your `Configuration` method, create an instance of `StormpathMiddleware`:

  ```csharp
  // Initialize the Stormpath middleware
  var stormpath = StormpathMiddleware.Create(new StormpathMiddlewareOptions()
  {
      LibraryUserAgent = "nowin/0.22",
      ViewRenderer = new SimpleViewRenderer()
  });
  ```

  Then, add it to the OWIN pipeline:

  ```csharp
  app.Use(stormpath);
  ```

8. **Set Up the View Renderer**

  The `Stormpath.Owin.Views.Precompiled` package includes a set of pre-built views that you can use without taking a dependency on Razor. You'll need to write a small rendering class to provide these to the middleware:

  ```csharp
  public class SimpleViewRenderer : IViewRenderer
  {
      public Task RenderAsync(string viewName, object viewModel, IOwinEnvironment context, CancellationToken cancellationToken)
      {
          var view = Stormpath.Owin.Views.Precompiled.ViewResolver.GetView(viewName);
          if (view == null)
          {
              // Or, hook into your existing view rendering implementation
              throw new Exception($"View '{viewName}' not found.");
          }

          return view.ExecuteAsync(viewModel, context.Response.Body);
      }
  }
  ```

8. **That's It!**

  Compile and run your project, and use a browser to access `http://localhost:8080/login`. You should see a login view.

More documentation is coming soon, including details on how you can protect your own routes. If you need help using this package, feel free to reach out to support@stormpath.com anytime.
