namespace Stormpath.Owin.Common.View
{
    using System.Threading.Tasks;

    public class Login : StormpathBaseView<ViewModel.LoginViewModel>
    {
#line 1 "Login.cshtml"

#line default
#line hidden
        #line hidden
        public Login()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#line 5 "Login.cshtml"
 foreach (var field in Model.Form.Fields)
{

#line default
#line hidden

            WriteLiteral("    <p>");
#line 7 "Login.cshtml"
  Write(field.Name);

#line default
#line hidden
            WriteLiteral("</p>\r\n");
#line 8 "Login.cshtml"
}

#line default
#line hidden

        }
        #pragma warning restore 1998
    }
}
