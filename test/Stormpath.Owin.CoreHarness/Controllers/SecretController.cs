using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace Stormpath.Owin.CoreHarness.Controllers
{
    [Route("api/[controller]")]
    [Authorize("Bearer")]
    public class SecretController
    {
        public string Get()
        {
            return "hello secure world!";
        }
    }
}
