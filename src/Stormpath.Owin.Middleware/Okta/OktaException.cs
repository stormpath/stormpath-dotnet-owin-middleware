using System;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Okta
{
    public class OktaException : InvalidOperationException
    {
        public OktaException(string message)
            : base(message) { }

        public IDictionary<string, object> Body { get; set; } = new Dictionary<string, object>();
    }
}
