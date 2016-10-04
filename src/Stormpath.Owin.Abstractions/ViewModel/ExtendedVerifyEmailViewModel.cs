namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class ExtendedVerifyEmailViewModel : VerifyEmailViewModel
    {
        public ExtendedVerifyEmailViewModel()
        {
        }

        public ExtendedVerifyEmailViewModel(VerifyEmailViewModel existing)
        {
            InvalidSpToken = existing.InvalidSpToken;
            Errors = existing.Errors;
            LoginEnabled = existing.LoginEnabled;
            LoginUri = existing.LoginUri;
        }

        public string StateToken { get; set; }
    }
}
