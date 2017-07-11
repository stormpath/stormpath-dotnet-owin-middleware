using System;

namespace Stormpath.Owin.Middleware.Model
{
    public sealed class MeGroupModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Status { get; } = "ENABLED"; // Groups in Okta are always enabled

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }
    }
}
