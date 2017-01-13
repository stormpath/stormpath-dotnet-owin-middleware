namespace Stormpath.Owin.Views.Precompiled
{
    using System.Threading.Tasks;

    public class Register : BaseView<Stormpath.Owin.Abstractions.ViewModel.RegisterFormViewModel>
    {
        #line hidden
        public Register()
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
    <meta content=""Sign up for an account"" name=""description"">
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

        .view {
            border: 1px solid rgb(183, 194, 205);
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""view register-view""></div>
    </div>

    <script language=""javascript"">
        window.onload = function onDocumentReady() {
            var stormpath = window.st");
            WriteLiteral("ormpath = new Stormpath({\r\n                container: document.getElementsByClassName(\'register-view\')[0]\r\n            });\r\n\r\n            stormpath.once(\'loggedIn\', function () {\r\n                window.location = \'");
#line 47 "Register.cshtml"
                              Write(Model.NextUri);

#line default
#line hidden
            WriteLiteral("\';\r\n            });\r\n\r\n            stormpath.showRegistration();\r\n        };\r\n    </script>\r\n    <script language=\"javascript\" src=\"http://localhost:3000/js/app.js\"></script>\r\n</body>\r\n</html>");
        }
        #pragma warning restore 1998
    }
}
