using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class Link
    {
        public string Href { get; set; }
        public string Method { get; set; }
    }
}
