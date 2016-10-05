namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class VerifyEmailFormViewModel : VerifyEmailViewModel
    {
        public VerifyEmailFormViewModel()
        {
        }

        public VerifyEmailFormViewModel(VerifyEmailViewModel existing)
        {
            InvalidSpToken = existing.InvalidSpToken;
            Errors = existing.Errors;
            LoginEnabled = existing.LoginEnabled;
            LoginUri = existing.LoginUri;
        }

        public string StateToken { get; set; }
    }
}
