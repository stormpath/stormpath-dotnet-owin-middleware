namespace Stormpath.Owin.Views.Precompiled
{
    using System.Threading.Tasks;

    public class Login : BaseView<Stormpath.Owin.Abstractions.ViewModel.LoginFormViewModel>
    {
        #line hidden
        public Login()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            WriteLiteral(@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <title>Log In</title>
    <meta content=""Log into your account"" name=""description"">
    <meta content=""width=device-width"" name=""viewport"">
    <style>
        html, body {
            height: 100%;
        }

        body {
            margin: 0;
        }

        .container {
            height: 100%;
            padding: 0;
            margin: 0;
            display: -webkit-box;
            display: -moz-box;
            display: -ms-flexbox;
            display: -webkit-flex;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .view { border: 1px solid rgb(183, 194, 205); }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""view login-view""></div>
    </div>

    <script language=""javascript"">
        window.onload = function onDocumentReady() {
            var stormpath = window.stormpath = new Stormpath({");
            WriteLiteral("\n                container: document.getElementsByClassName(\'login-view\')[0]\r\n            });\r\n\r\n            stormpath.once(\'loggedIn\', function() {\r\n                window.location = \'");
#line 45 "Login.cshtml"
                              Write(Model.NextUri);

#line default
#line hidden
            WriteLiteral("\';\r\n            });\r\n\r\n            stormpath.showLogin();\r\n        };\r\n    </script>\r\n    <script language=\"javascript\" src=\"http://localhost:3000/js/app.js\"></script>\r\n</body>\r\n</html>");
        }
        #pragma warning restore 1998
    }
}
