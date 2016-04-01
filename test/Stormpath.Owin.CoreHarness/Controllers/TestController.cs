using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Stormpath.SDK;

namespace Stormpath.Owin.CoreHarness
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [FromServices]
        public SDK.Client.IClient StormpathClient { get; set; }

        public async Task<string> Get()
        {
            await StormpathClient.GetApplications().FirstOrDefaultAsync();

            return "hello world";
        }
    }
}
