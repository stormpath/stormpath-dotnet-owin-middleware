namespace Stormpath.Owin.Middleware
{
    internal sealed class ExternalLoginResult
    {
        public dynamic Account { get; set; }

        public bool IsNewAccount { get; set; }
    }
}
