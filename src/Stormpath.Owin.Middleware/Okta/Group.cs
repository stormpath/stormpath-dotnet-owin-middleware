using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Okta
{
    public class Group
    {
        public string Id { get; set; }
        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? LastUpdated { get; set; }
        public DateTimeOffset? LastMembershipUpdated { get; set; }
        public List<string> ObjectClass { get; set; }
        public string Type { get; set; }
        public GroupProfile Profile { get; set; }

        [JsonProperty("_links")]
        public GroupLinks _links { get; set; }
    }

    public class GroupProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class GroupLinks
    {
        public List<Link> Logo { get; set; }
        public Link Users { get; set; }
        public Link Apps { get; set; }
    }
}
