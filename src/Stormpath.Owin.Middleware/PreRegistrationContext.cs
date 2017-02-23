using System.Collections.Generic;
using System.Collections.ObjectModel;
using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreRegistrationContext : HandlerContext
    {
        public PreRegistrationContext(IOwinEnvironment environment, dynamic account)
            : base(environment)
        {
            Account = account;
        }

        public PreRegistrationContext(IOwinEnvironment environment, dynamic account, IDictionary<string, string> postData)
            : base(environment)
        {
            Account = account;
            PostData = new ReadOnlyDictionary<string, string>(postData);
        }

        public dynamic Account { get; }

        public IReadOnlyDictionary<string, string> PostData { get; }

        public PreRegistrationResult Result { get; set; }
    }
}
