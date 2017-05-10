namespace Stormpath.Owin.Middleware.Okta
{
    public class ShimApiKey
    {
        public string Id { get; set; }

        public string Secret { get; set; }

        public string Status { get; set; }

        public User User { get; set; }
    }
}
