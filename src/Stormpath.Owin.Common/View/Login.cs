namespace Stormpath.Owin.Common.View
{
    using System.Threading.Tasks;

    public class Login : StormpathBaseView<Stormpath.Owin.Common.ViewModel.LoginViewModel>
    {
        #line hidden
        public Login()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
#line 1 "Login.cshtml"
 foreach (var field in Model.Form.Fields)
{

#line default
#line hidden

            WriteLiteral("    <p>");
#line 3 "Login.cshtml"
  Write(field.Name);

#line default
#line hidden
            WriteLiteral("</p>\r\n");
#line 4 "Login.cshtml"
}

#line default
#line hidden

        }
        #pragma warning restore 1998
    }
}
