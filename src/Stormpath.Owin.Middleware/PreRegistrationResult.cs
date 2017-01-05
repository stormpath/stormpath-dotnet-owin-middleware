namespace Stormpath.Owin.Middleware
{
    public sealed class PreRegistrationResult
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }
    }
}
