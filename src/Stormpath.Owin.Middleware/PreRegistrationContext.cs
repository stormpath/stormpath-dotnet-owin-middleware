using System.Collections.Generic;
using System.Collections.ObjectModel;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreRegistrationContext : HandlerContext
    {
        public PreRegistrationContext(IOwinEnvironment environment, ICompatibleOktaAccount account)
            : base(environment)
        {
            Account = account;
        }

        public PreRegistrationContext(IOwinEnvironment environment, ICompatibleOktaAccount account, IDictionary<string, string> postData)
            : base(environment)
        {
            Account = account;
            PostData = new ReadOnlyDictionary<string, string>(postData);
        }

        public ICompatibleOktaAccount Account { get; }

        public IReadOnlyDictionary<string, string> PostData { get; }

        public PreRegistrationResult Result { get; set; }
    }
}
