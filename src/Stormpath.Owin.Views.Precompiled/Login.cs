namespace Stormpath.Owin.Views.Precompiled
{
#line 1 "Login.cshtml"
using System

#line default
#line hidden
    ;
#line 2 "Login.cshtml"
using System.Linq

#line default
#line hidden
    ;
#line 3 "Login.cshtml"
using Stormpath.Owin.Abstractions

#line default
#line hidden
    ;
#line 4 "Login.cshtml"
using Stormpath.Owin.Abstractions.ViewModel

#line default
#line hidden
    ;
    using System.Threading.Tasks;

    public class Login : BaseView<LoginFormViewModel>
    {
        #line hidden
        public Login()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#line 6 "Login.cshtml"
  
    bool hasSocialProviders = Model.AccountStores.Any();
    AccountStoreViewModel facebookProvider = Model.AccountStores.FirstOrDefault(store => store.Provider.ProviderId.Equals("facebook", StringComparison.OrdinalIgnoreCase));
    AccountStoreViewModel googleProvider = Model.AccountStores.FirstOrDefault(store => store.Provider.ProviderId.Equals("google", StringComparison.OrdinalIgnoreCase));
    AccountStoreViewModel githubProvider = Model.AccountStores.FirstOrDefault(store => store.Provider.ProviderId.Equals("github", StringComparison.OrdinalIgnoreCase));
    AccountStoreViewModel linkedInProvider = Model.AccountStores.FirstOrDefault(store => store.Provider.ProviderId.Equals("linkedin", StringComparison.OrdinalIgnoreCase));
    AccountStoreViewModel[] samlProviders = Model.AccountStores.Where(store => store.Provider.ProviderId.Equals("saml", StringComparison.OrdinalIgnoreCase)).ToArray();

#line default
#line hidden

            WriteLiteral(@"<!DOCTYPE html>
<!--[if lt IE 7]>      <html class=""no-js lt-ie9 lt-ie8 lt-ie7""> <![endif]-->
<!--[if IE 7]>         <html class=""no-js lt-ie9 lt-ie8""> <![endif]-->
<!--[if IE 8]>         <html class=""no-js lt-ie9""> <![endif]-->
<!--[if gt IE 8]><!-->
<html lang=""en"" class=""no-js"">
<!--<![endif]-->
<head>
    <meta charset=""utf-8"">
    <title>Log In</title>
    <meta content=""Log into your account!"" name=""description"">
    <meta content=""width=device-width"" name=""viewport"">
    <link href=""//fonts.googleapis.com/css?family=Open+Sans:300italic,300,400italic,400,600italic,600,700italic,700,800italic,800"" rel=""stylesheet"" type=""text/css"">
    <link href=""//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css"" rel=""stylesheet"">
    <style>
        html,
body {
  height: 100%;
}

@media (max-width: 767px) {
  html,
  body {
    padding: 0 4px;
  }
}

body {
  margin-left: auto;
  margin-right: auto;
}

body,
div,
p,
a,
label {
  font-family: 'Open Sans';
  font-size: 14px;
  font-weight: 400;
  color: #484848;
}

a {
  color: #0072dd;
}

p {
  line-height: 21px;
}

.container {
  max-width: 620px;
}

.logo {
  margin: 34px auto 25px auto;
  display: block;
}

.btn-sp-green {
  height: 45px;
  line-height: 22.5px;
  padding: 0 40px;
  color: #fff;
  font-size: 17px;
  background: -webkit-linear-gradient(#42c41a 50%, #2dbd00 50%);
  background: linear-gradient(#42c41a 50%, #2dbd00 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#2dbd00, endColorstr=#42c41a);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#2dbd00, endColorstr=#42c41a)';
}

.btn-sp-green:hover,
.btn-sp-green:focus {
  color: #fff;
  background: -webkit-linear-gradient(#43cd1a 50%, #2ec700 50%);
  background: linear-gradient(#43cd1a 50%, #2ec700 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#2ec700, endColorstr=#43cd1a);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#2ec700, endColorstr=#43cd1a)';
}

.btn-social {
  height: 37px;
  line-height: 18.5px;
  color: #fff;
  font-size: 16px;
  border-radius: 3px;
}

.btn-social:hover,
.btn-social:focus {
  color: #fff;
}

.btn-facebook {
  background: -webkit-linear-gradient(#4c6fc5 50%, #3d63c0 50%);
  background: linear-gradient(#4c6fc5 50%, #3d63c0 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#3d63c0, endColorstr=#4c6fc5);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#3d63c0, endColorstr=#4c6fc5)';
}

.btn-facebook:hover,
.btn-facebook:focus {
  color: #fff;
  background: -webkit-linear-gradient(#4773de 50%, #3767db 50%);
  background: linear-gradient(#4773de 50%, #3767db 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#3767db, endColorstr=#4773de);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#3767db, endColorstr=#4773de)';
}

.btn-google {
  background: -webkit-linear-gradient(#e05b4b 50%, #dd4b39 50%);
  background: linear-gradient(#e05b4b 50%, #dd4b39 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#dd4b39, endColorstr=#e05b4b);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#dd4b39, endColorstr=#e05b4b)';
}

.btn-google:hover,
.btn-google:focus {
  color: #fff;
  background: -webkit-linear-gradient(#ea604e 50%, #e8503c 50%);
  background: linear-gradient(#ea604e 50%, #e8503c 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#e8503c, endColorstr=#ea604e);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#e8503c, endColorstr=#ea604e)';
}

.btn-linkedin {
  background: -webkit-linear-gradient(#007cbc 50%, #0077B5 50%);
  background: linear-gradient(#007cbc 50%, #0077B5 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5)';
}

.btn-linkedin:hover,
.btn-linkedin:focus {
  color: #fff;
  background: -webkit-linear-gradient(#007cbc 50%, #0077B5 50%);
  background: linear-gradient(#007cbc 50%, #0077B5 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5)';
}

.btn-github {
  background: -webkit-linear-gradient(#848282 50%, #7B7979 50%);
  background: linear-gradient(#848282 50%, #7B7979 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#848282, endColorstr=#7B7979);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#848282, endColorstr=#7B7979)';
}

.btn-github:hover,
.btn-github:focus {
  color: #fff;
  background: -webkit-linear-gradient(#8C8888 50%, #848080 50%);
  background: linear-gradient(#8C8888 50%, #848080 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#8C8888, endColorstr=#848080);
  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#8C8888, endColorstr=#848080)';
}

.btn-register {
  font-size: 16px;
}

.form-control {
  font-size: 15px;
  box-shadow: none;
}

.form-control::-webkit-input-placeholder {
  color: #aaadb0;
}

.form-control::-moz-placeholder {
  color: #aaadb0;
}

.form-control:-ms-input-placeholder {
  color: #aaadb0;
}

.form-control::placeholder {
  color: #aaadb0;
}

.form-control:focus {
  box-shadow: inset 0 1px 1px rgba(0, 0, 0, 0.075), 0 0 6px rgba(0, 132, 255, 0.4);
}

.view .header {
  padding: 34px 0;
}

.view .header,
.view .header a {
  font-weight: 300;
  font-size: 21px;
}

.view input[type='text'],
.view input[type='password'],
.view input[type='email'],
.view input[type='color'],
.view input[type='date'],
.view input[type='datetime']
.view input[type='datetime-local'],
.view input[type='email'],
.view input[type='month'],
.view input[type='number'],
.view input[type='range'],
.view input[type='search'],
.view input[type='tel'],
.view input[type='time'],
.view input[type='url'],
.view input[type='week']{
  background-color: #f6f6f6;
  height: 45px;
}

.view a.forgot,
.view a.to-login {
  float: right;
  padding: 17px 0;
  font-size: 13px;
}

.view form button {
  display: block;
  float: right;
  margin-bottom: 25px;
}

.view form label {
  height: 45px;
  line-height: 45px;
}

.box {
  box-shadow: 0 0px 3px 1px rgba(0, 0, 0, 0.1);
  border: 1px solid #cacaca;
  border-radius: 3px;
  padding: 0 30px;
}

.sp-form .has-error,
.sp-form .has-error .help-block {
  color: #ec3e3e;
  font-weight: 600;
}

.sp-form .has-error input[type='text'],
.sp-form .has-error input[type='password'] {
  border-color: #ec3e3e;
}

.sp-form .form-group {
  margin-bottom: 21px;
}

.sp-form input[type='text'],
.sp-form input[type='password'] {
  position: relative;
}

.sp-form .help-block {
  font-size: 12px;
  position: absolute;
  top: 43px;
}

.verify-view .box {
  padding-bottom: 30px;
}

.verify-view .box .header {
  padding-bottom: 20px;
}

.unverified-view .box {
  padding-bottom: 30px;
}

.unverified-view .box .header {
  padding-bottom: 25px;
}

.login-view .box {
  background-color: #f6f6f6;
  padding: 0;
}

.login-view label {
  margin-bottom: 7px;
}

.login-view .header p {
  margin-top: 2em;
}

.login-view .email-password-area {
  background-color: white;
  border-top-left-radius: 3px;
  border-bottom-left-radius: 3px;
}

@media (min-width: 767px) {
  .login-view .email-password-area {
    padding: 0 30px;
  }
}

.login-view .email-password-area label {
  height: 14px;
  line-height: 14px;
}

.login-view .email-password-area input[type='checkbox'] {
  visibility: hidden;
}

.login-view .email-password-area input[type='checkbox'] + label {
  position: relative;
  padding-left: 8px;
  line-height: 16px;
  font-size: 13px;
}

.login-view .email-password-area input[type='checkbox'] + label:after {
  position: absolute;
  left: -16px;
  width: 16px;
  height: 16px;
  border: 1px solid #cacaca;
  background-color: #f6f6f6;
  content: '';
}

.login-view .email-password-area input[type='checkbox']:checked + label:after {
  background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAA2ZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowRTVBQUVGMzJEODBFMjExODQ2N0NBMjk4MjdCNDBCNyIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDo0RTY4NUM4NURGNEYxMUUyQUE5QkExOTlGODU3RkFEOCIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo0RTY4NUM4NERGNEYxMUUyQUE5QkExOTlGODU3RkFEOCIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M2IChXaW5kb3dzKSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjQxNDQ4M0NEM0JERkUyMTE4MEYwQjNBRjIwMUNENzQxIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOkZDMEMxNjY2OUVCMUUyMTFBRjVDQkQ0QjE5MTNERDU2Ii8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+3YY4qgAAALlJREFUeNpi/P//PwMlgImBQjDwBrCgCwQHB+NUfObMGT9mZuboe/fuheM0ABu4fv060/fv32cBNTNycHBE4nUBNs0/f/7cAWSeMzQ0rCA5DICaNwKj+qGRkVEFUYF47ty5GWfPns2EsjsYGRlFgM5OJzoQ//37t5eLi2sRMMDec3Jypn79+lVXX1//H9HRaGJisvr379/nuLm5lwKdP9vMzOwZyekAaEA3EF8G4hZCYcQ4mhcYAAIMAJGST/dDIpNQAAAAAElFTkSuQmCC);
  background-position: -1px -1px;
}

@media (min-width: 767px) {
  .login-view .email-password-area.small {
    border-right: 1px solid #cacaca;
  }

  .login-view .email-password-area.small .group-email {
    margin-bottom: 21px;
  }
}

@media (max-width: 767px) {
  .login-view .email-password-area.small {
    border-bottom: 1px solid #cacaca;
    border-bottom-left-radius: 0;
    border-bottom-right-radius: 0;
  }
}

.login-view .email-password-area.large {
  border-top-right-radius: 3px;
  border-bottom-right-radius: 3px;
}

@media (min-width: 767px) {
  .login-view .email-password-area.large {
    padding: 0 50px;
  }

  .login-view .email-password-area.large .group-login label,
  .login-view .email-password-area.large .group-password label {
    height: 45px;
    line-height: 45px;
  }
}

.login-view .social-area {
  border-top-right-radius: 3px;
  border-bottom-right-radius: 3px;
  padding: 0 20px;
  position: relative;
  padding-bottom: 20px;
  background-color: #f6f6f6;
}

.login-view .social-area .header {
  margin-bottom: -6px;
}

@media (max-width: 767px) {
  .login-view .social-area .header {
    padding: 0px;
  }
}

.login-view .social-area button {
  display: block;
  width: 100%;
  margin-bottom: 15px;
}

.login, .register { display: table; }
.va-wrapper { display: table-cell; width: 100%; vertical-align: middle; }
.custom-container { display: table-row; height: 100%; }
    </style>
    <!--[if lt IE 9]>
     <script src='https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js'></script>
     <script src='https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js'></script>
    <");
            WriteLiteral("![endif]-->\r\n</head>\r\n<body class=\"login\">\r\n    <div class=\"container custom-container\">\r\n        <div class=\"va-wrapper\">\r\n            <div class=\"view login-view container\">\r\n");
#line 40 "Login.cshtml"
                

#line default
#line hidden

#line 40 "Login.cshtml"
                 if (Stormpath.Owin.Abstractions.ViewModel.LoginFormViewModel.AcceptableStatuses.Any(x => x.Equals(Model.Status, StringComparison.OrdinalIgnoreCase))) {

#line default
#line hidden

            WriteLiteral("                    <div class=\"box row\">\r\n                        <div class=\"email-password-area col-xs-12 large col-sm-12\">\r\n                            <div class=\"header\">\r\n");
#line 44 "Login.cshtml"
                                

#line default
#line hidden

#line 44 "Login.cshtml"
                                 if (Model.Status.Equals("unverified", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral(@"                                    <span>Your account verification email has been sent!</span>
                                    <p>
                                        Before you can log into your account, you need to activate your
                                        account by clicking the link we sent to your inbox.
                                    </p>
                                    <p>Didn't get the email? <a");
            BeginWriteAttribute("href", " href=\"", 3171, "\"", 3199, 1);
#line 51 "Login.cshtml"
WriteAttributeValue("", 3178, Model.VerifyEmailUri, 3178, 21, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">Click Here</a>.</p>\r\n                                    <br>\r\n");
#line 53 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 54 "Login.cshtml"
                                 if (Model.Status.Equals("verified", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Your Account Has Been Verified.</span>\r\n                                    <p>\r\n                                        You may now login.\r\n                                    </p>\r\n");
#line 60 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 61 "Login.cshtml"
                                 if (Model.Status.Equals("created", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Your Account Has Been Created.</span>\r\n                                    <p>\r\n                                        You may now login.\r\n                                    </p>\r\n");
#line 67 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 68 "Login.cshtml"
                                 if (Model.Status.Equals("reset", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Password Reset Successfully.</span>\r\n                                    <p>\r\n                                        You can now login with your new password.\r\n                                    </p>\r\n");
#line 74 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 75 "Login.cshtml"
                                 if (Model.Status.Equals("forgot", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral(@"                                    <span>Password Reset Requested.</span>
                                    <p>
                                        If an account exists for the email provided, you will receive an email shortly.
                                    </p>
");
#line 81 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 82 "Login.cshtml"
                                 if (Model.Status.Equals("social_failed", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Login failed.</span>\r\n                                    <p>\r\n                                        An error occurred while trying to log you in. Please try again.\r\n                                    </p>\r\n");
#line 88 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                            </div>\r\n                        </div>\r\n                    </div>\r\n");
#line 92 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("                <br>\r\n                <div class=\"box row\">\r\n                    <div");
            BeginWriteAttribute("class", " class=\"", 5609, "\"", 5707, 3);
            WriteAttributeValue("", 5617, "email-password-area", 5617, 19, true);
            WriteAttributeValue(" ", 5636, "col-xs-12", 5637, 10, true);
#line 95 "Login.cshtml"
WriteAttributeValue(" ", 5646, hasSocialProviders ? "small col-sm-8" : "large col-sm-12", 5647, 61, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n");
#line 96 "Login.cshtml"
                        

#line default
#line hidden

#line 96 "Login.cshtml"
                         if (Model.RegistrationEnabled)
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"header\">\r\n                                <span>Log In or <a");
            BeginWriteAttribute("href", " href=\"", 5895, "\"", 5988, 1);
#line 99 "Login.cshtml"
WriteAttributeValue("", 5902, Model.RegisterUri + "?" + @StringConstants.StateTokenName + "=" + @Model.StateToken, 5902, 86, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">Create Account</a></span>\r\n                            </div>\r\n");
#line 101 "Login.cshtml"
                        }
                        else
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"header\">\r\n                                <span>Log In</span>\r\n                            </div>\r\n");
#line 107 "Login.cshtml"
                        }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 109 "Login.cshtml"
                        

#line default
#line hidden

#line 109 "Login.cshtml"
                         if (Model.Errors.Any())
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"alert alert-danger bad-login\">\r\n");
#line 112 "Login.cshtml"
                                

#line default
#line hidden

#line 112 "Login.cshtml"
                                 foreach (var error in Model.Errors)
                                {

#line default
#line hidden

            WriteLiteral("                                    <p>");
#line 114 "Login.cshtml"
                                  Write(error);

#line default
#line hidden
            WriteLiteral("</p>\r\n");
#line 115 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                            </div>\r\n");
#line 117 "Login.cshtml"
                        }

#line default
#line hidden

            WriteLiteral("                        <form method=\"post\" role=\"form\" class=\"login-form form-horizontal\">\r\n                            <input");
            BeginWriteAttribute("name", " name=\"", 6835, "\"", 6873, 1);
#line 119 "Login.cshtml"
WriteAttributeValue("", 6842, StringConstants.StateTokenName, 6842, 31, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" type=\"hidden\"");
            BeginWriteAttribute("value", " value=\"", 6888, "\"", 6913, 1);
#line 119 "Login.cshtml"
WriteAttributeValue("", 6896, Model.StateToken, 6896, 17, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral("/>\r\n\r\n");
#line 121 "Login.cshtml"
                            

#line default
#line hidden

#line 121 "Login.cshtml"
                             foreach (var field in Model.Form.Fields)
                            {

#line default
#line hidden

            WriteLiteral("                                <div");
            BeginWriteAttribute("class", " class=\"", 7058, "\"", 7101, 2);
            WriteAttributeValue("", 7066, "form-group", 7066, 10, true);
#line 123 "Login.cshtml"
WriteAttributeValue(" ", 7076, $"group-{field.Name}", 7077, 25, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n                                    <label");
            BeginWriteAttribute("class", " class=\"", 7147, "\"", 7203, 1);
#line 124 "Login.cshtml"
WriteAttributeValue("", 7155, hasSocialProviders ? "col-sm-12" : "col-sm-4", 7155, 48, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n                                        ");
#line 125 "Login.cshtml"
                                   Write(field.Label);

#line default
#line hidden
            WriteLiteral("\r\n                                    </label>\r\n                                    <div");
            BeginWriteAttribute("class", " class=\"", 7347, "\"", 7403, 1);
#line 127 "Login.cshtml"
WriteAttributeValue("", 7355, hasSocialProviders ? "col-sm-12" : "col-sm-8", 7355, 48, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n                                        <input");
            BeginWriteAttribute("placeholder", " placeholder=\"", 7453, "\"", 7485, 1);
#line 128 "Login.cshtml"
WriteAttributeValue("", 7467, field.Placeholder, 7467, 18, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("name", "\r\n                                               name=\"", 7486, "\"", 7552, 1);
#line 129 "Login.cshtml"
WriteAttributeValue("", 7541, field.Name, 7541, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("type", "\r\n                                               type=\"", 7553, "\"", 7619, 1);
#line 130 "Login.cshtml"
WriteAttributeValue("", 7608, field.Type, 7608, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("value", "\r\n                                               value=\"", 7620, "\"", 7725, 1);
#line 131 "Login.cshtml"
WriteAttributeValue("", 7676, Model.FormData.Get(field.Name) ?? string.Empty, 7676, 49, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral("\r\n                                               class=\"form-control\"\r\n                                               ");
#line 133 "Login.cshtml"
                                           Write(field.Required ? "required" : string.Empty);

#line default
#line hidden
            WriteLiteral(">\r\n                                    </div>\r\n                                </div>\r\n");
#line 136 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("                            <div>\r\n                                <button type=\"submit\" class=\"login btn btn-login btn-sp-green\">Log In</button>\r\n                            </div>\r\n                        </form>\r\n                    </div>\r\n");
#line 142 "Login.cshtml"
                    

#line default
#line hidden

#line 142 "Login.cshtml"
                     if (hasSocialProviders)
                    {

#line default
#line hidden

            WriteLiteral("                        <div class=\"social-area col-xs-12 col-sm-4\">\r\n                            <div class=\"header\">&nbsp;</div>\r\n                            <label>Easy 1-click login:</label>\r\n                            \r\n");
#line 148 "Login.cshtml"
                            

#line default
#line hidden

#line 148 "Login.cshtml"
                             if (samlProviders.Any())
                            {
                                foreach (var samlProvider in samlProviders)
                                {

#line default
#line hidden

            WriteLiteral("                                    <button class=\"btn btn-social btn-saml\"");
            BeginWriteAttribute("onclick", " onclick=\"", 8819, "\"", 8881, 5);
            WriteAttributeValue("", 8829, "samlLogin(\'", 8829, 11, true);
#line 152 "Login.cshtml"
WriteAttributeValue("", 8840, samlProvider.Href, 8840, 18, false);

#line default
#line hidden
            WriteAttributeValue("", 8858, "?st=", 8858, 4, true);
#line 152 "Login.cshtml"
WriteAttributeValue("", 8862, Model.StateToken, 8862, 17, false);

#line default
#line hidden
            WriteAttributeValue("", 8879, "\')", 8879, 2, true);
            EndWriteAttribute();
            WriteLiteral(">\r\n                                        <span class=\"fa fa-lock\"></span> ");
#line 153 "Login.cshtml"
                                                                    Write(samlProvider.Name);

#line default
#line hidden
            WriteLiteral("\r\n                                    </button>\r\n");
#line 155 "Login.cshtml"
                                }


#line default
#line hidden

            WriteLiteral(@"                                <script type=""text/javascript"">
                                    function samlLogin(provider) {
                                        console.log(provider);
                                    }
                                </script>
");
#line 162 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("\r\n\r\n");
#line 165 "Login.cshtml"
                            

#line default
#line hidden

#line 165 "Login.cshtml"
                             if (facebookProvider != null)
                            {

#line default
#line hidden

            WriteLiteral(@"                                <button class=""btn btn-social btn-facebook"" onclick=""facebookLogin()"">Facebook</button>
                                <script type=""text/javascript"">
                                    function facebookLogin() {
                                        var FB = window.FB;
                                        var facebookScope = '");
#line 171 "Login.cshtml"
                                                        Write(facebookProvider.Provider.Scope);

#line default
#line hidden
            WriteLiteral(@"';

                                        FB.login(function (response) {
                                            if (response.status === 'connected') {
                                                var queryString = 'access_token=' + response.authResponse.accessToken;

                                                if (""");
#line 177 "Login.cshtml"
                                                Write(Model.StateToken);

#line default
#line hidden
            WriteLiteral("\".length !== 0) {\r\n                                                    queryString += \"&");
#line 178 "Login.cshtml"
                                                                Write(StringConstants.StateTokenName);

#line default
#line hidden
            WriteLiteral("=\" + \"");
#line 178 "Login.cshtml"
                                                                                                     Write(Model.StateToken);

#line default
#line hidden
            WriteLiteral("\";\r\n                                                }\r\n\r\n                                                window.location.replace(\'");
#line 181 "Login.cshtml"
                                                                    Write(facebookProvider.Href);

#line default
#line hidden
            WriteLiteral(@"?' + queryString);
                                            }
                                        }, { scope: facebookScope });
                                    }

                                    window.fbAsyncInit = function () {
                                        FB.init({
                                            appId: '");
#line 188 "Login.cshtml"
                                               Write(facebookProvider.Provider.ClientId);

#line default
#line hidden
            WriteLiteral(@"',
                                            cookie: true,
                                            xfbml: true,
                                            version: 'v2.3'
                                        });
                                    };

                                    (function (d, s, id) {
                                        var js, fjs = d.getElementsByTagName(s)[0];
                                        if (d.getElementById(id)) { return; }
                                        js = d.createElement(s); js.id = id;
                                        js.src = ""//connect.facebook.net/en_US/sdk.js"";
                                        fjs.parentNode.insertBefore(js, fjs);
                                    }(document, 'script', 'facebook-jssdk'));
                                </script>
");
#line 203 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 205 "Login.cshtml"
                            

#line default
#line hidden

#line 205 "Login.cshtml"
                             if (googleProvider != null)
                            {

#line default
#line hidden

            WriteLiteral(@"                                <button class=""btn btn-social btn-google"" onclick=""googleLogin()"">Google</button>
                                <script type=""text/javascript"">
                                    function googleLogin() {
                                        var clientId = '");
#line 210 "Login.cshtml"
                                                   Write(googleProvider.Provider.ClientId);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var googleScope = \'");
#line 211 "Login.cshtml"
                                                      Write(googleProvider.Provider.Scope);

#line default
#line hidden
            WriteLiteral(@"';

                                        var finalUrl = 'https://accounts.google.com/o/oauth2/auth?response_type=code' +
                                            '&client_id=' + encodeURIComponent(clientId) +
                                            '&scope=' + encodeURIComponent(googleScope) +
                                            '&state=' + encodeURIComponent('");
#line 216 "Login.cshtml"
                                                                       Write(Model.StateToken);

#line default
#line hidden
            WriteLiteral("\') +\r\n                                            \'&include_granted_scopes=true\' +\r\n                                            \'&redirect_uri=\' + encodeURIComponent(\'");
#line 218 "Login.cshtml"
                                                                              Write(googleProvider.Href);

#line default
#line hidden
            WriteLiteral("\');\r\n\r\n                                        window.location = finalUrl;\r\n                                    }\r\n                                </script>\r\n");
#line 223 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 225 "Login.cshtml"
                            

#line default
#line hidden

#line 225 "Login.cshtml"
                             if (githubProvider != null)
                            {

#line default
#line hidden

            WriteLiteral(@"                                <button class=""btn btn-social btn-github"" onclick=""githubLogin()"">Github</button>
                                <script type=""text/javascript"">
                                    function githubLogin() {
                                        var clientId = '");
#line 230 "Login.cshtml"
                                                   Write(githubProvider.Provider.ClientId);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var gitHubScope = \'");
#line 231 "Login.cshtml"
                                                      Write(githubProvider.Provider.Scope);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var redirectUri = \'");
#line 232 "Login.cshtml"
                                                      Write(githubProvider.Href);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var stateToken = \'");
#line 233 "Login.cshtml"
                                                     Write(Model.StateToken);

#line default
#line hidden
            WriteLiteral(@"';

                                        var url = 'https://github.com/login/oauth/authorize' +
                                            '?client_id=' + encodeURIComponent(clientId) +
                                            '&scope=' + encodeURIComponent(gitHubScope) +
                                            '&redirect_uri=' + encodeURIComponent(redirectUri) +
                                            '&state=' + encodeURIComponent(stateToken);

                                        window.location = url;
                                    }
                                </script>
");
#line 244 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 246 "Login.cshtml"
                            

#line default
#line hidden

#line 246 "Login.cshtml"
                             if (linkedInProvider != null)
                            {

#line default
#line hidden

            WriteLiteral(@"                                <button class=""btn btn-social btn-linkedin"" onclick=""linkedinLogin()"">LinkedIn</button>
                                <script type=""text/javascript"">
                                    function buildUrl(baseUrl, queryString) {
                                        var result = baseUrl;

                                        if (queryString) {
                                            var serializedQueryString = '';

                                            for (var key in queryString) {
                                                var value = queryString[key];

                                                if (serializedQueryString.length) {
                                                    serializedQueryString += '&';
                                                }

                                                // Don't include any access_token parameters in
                                                // the query string as it will b");
            WriteLiteral(@"e added by LinkedIn.
                                                if (key === 'access_token') {
                                                    continue;
                                                }

                                                serializedQueryString += key + '=' + encodeURIComponent(value);
                                            }

                                            result += '?' + serializedQueryString;
                                        }

                                        return result;
                                    }

                                    function linkedinLogin() {
                                        var stateToken = '");
#line 279 "Login.cshtml"
                                                     Write(Model.StateToken);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var authorizationUrl = \'https://www.linkedin.com/uas/oauth2/authorization\';\r\n\r\n                                        var clientId = \'");
#line 282 "Login.cshtml"
                                                   Write(linkedInProvider.Provider.ClientId);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var redirectUri = \'");
#line 283 "Login.cshtml"
                                                      Write(linkedInProvider.Href);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var linkedinScope = \'");
#line 284 "Login.cshtml"
                                                        Write(linkedInProvider.Provider.Scope);

#line default
#line hidden
            WriteLiteral(@"';

                                        window.location = buildUrl(authorizationUrl, {
                                            response_type: 'code',
                                            client_id: clientId,
                                            scope: linkedinScope,
                                            redirect_uri: redirectUri,
                                            state: stateToken
                                        });
                                    }
                                </script>
");
#line 295 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("                        </div>\r\n");
#line 297 "Login.cshtml"
                    }

#line default
#line hidden

            WriteLiteral("                </div>\r\n");
#line 299 "Login.cshtml"
                

#line default
#line hidden

#line 299 "Login.cshtml"
                 if (Model.VerifyEmailEnabled)
                {

#line default
#line hidden

            WriteLiteral("                    <a style=\"float:left\"");
            BeginWriteAttribute("href", " href=\"", 17454, "\"", 17482, 1);
#line 301 "Login.cshtml"
WriteAttributeValue("", 17461, Model.VerifyEmailUri, 17461, 21, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"forgot\">Resend Verification Email?</a>\r\n");
#line 302 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 304 "Login.cshtml"
                

#line default
#line hidden

#line 304 "Login.cshtml"
                 if (Model.ForgotPasswordEnabled)
                {

#line default
#line hidden

            WriteLiteral("                    <a style=\"float:right\"");
            BeginWriteAttribute("href", " href=\"", 17664, "\"", 17695, 1);
#line 306 "Login.cshtml"
WriteAttributeValue("", 17671, Model.ForgotPasswordUri, 17671, 24, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"forgot\">Forgot Password?</a>\r\n");
#line 307 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("            </div>\r\n        </div>\r\n    </div>\r\n    <script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js\"></script>\r\n    <script src=\"//netdna.bootstrapcdn.com/bootstrap/3.1.1/js/bootstrap.min.js\"></script>\r\n</body>\r\n</html>");
        }
        #pragma warning restore 1998
    }
}
