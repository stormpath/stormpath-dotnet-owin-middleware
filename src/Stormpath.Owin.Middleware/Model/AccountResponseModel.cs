using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stormpath.SDK.Application;
using Stormpath.SDK.CustomData;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Group;
using Stormpath.SDK.Provider;
using Stormpath.SDK.Tenant;

namespace Stormpath.Owin.Middleware.Model
{
    public class AccountResponseModel
    {
        public string Href { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string GivenName { get; set; }

        public string MiddleName { get; set; }

        public string Surname { get; set; }

        public string FullName { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public string Status { get; set; }

        public IReadOnlyCollection<IApplication> Applications { get; set; } = null;

        public ICustomData CustomData { get; set; } = null;

        public IDirectory Directory { get; set; } = null;

        public IReadOnlyCollection<IGroupMembership> GroupMemberships { get; set; } = null;

        public IReadOnlyCollection<IGroup> Groups { get; set; } = null;

        public IProviderData ProviderData { get; set; } = null;

        public ITenant Tenant { get; set; } = null;
    }
}