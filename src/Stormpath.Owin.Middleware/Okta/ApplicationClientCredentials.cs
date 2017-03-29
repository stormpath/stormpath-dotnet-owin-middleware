using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware.Okta
{
    public class ApplicationClientCredentials
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string token_endpoint_auth_method { get; set; }
    }
}
