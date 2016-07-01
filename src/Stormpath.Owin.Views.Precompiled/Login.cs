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

    public class Login : BaseView<ExtendedLoginViewModel>
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
    AccountStoreViewModel facebookStore = Model.AccountStores.FirstOrDefault(store => store.Name.Equals("facebook", StringComparison.OrdinalIgnoreCase));
    AccountStoreViewModel googleStore = Model.AccountStores.FirstOrDefault(store => store.Name.Equals("google", StringComparison.OrdinalIgnoreCase));
    AccountStoreViewModel githubStore = Model.AccountStores.FirstOrDefault(store => store.Name.Equals("github", StringComparison.OrdinalIgnoreCase));

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
#line 38 "Login.cshtml"
                

#line default
#line hidden

#line 38 "Login.cshtml"
                 if (Stormpath.Owin.Abstractions.ViewModel.ExtendedLoginViewModel.AcceptableStatuses.Any(x => x.Equals(Model.Status, StringComparison.OrdinalIgnoreCase))) {

#line default
#line hidden

            WriteLiteral("                    <div class=\"box row\">\r\n                        <div class=\"email-password-area col-xs-12 large col-sm-12\">\r\n                            <div class=\"header\">\r\n");
#line 42 "Login.cshtml"
                                

#line default
#line hidden

#line 42 "Login.cshtml"
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
            BeginWriteAttribute("href", " href=\"", 2779, "\"", 2807, 1);
#line 49 "Login.cshtml"
WriteAttributeValue("", 2786, Model.VerifyEmailUri, 2786, 21, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">Click Here</a>.</p>\r\n                                    <br>\r\n");
#line 51 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 52 "Login.cshtml"
                                 if (Model.Status.Equals("verified", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Your Account Has Been Verified.</span>\r\n                                    <p>\r\n                                        You may now login.\r\n                                    </p>\r\n");
#line 58 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 59 "Login.cshtml"
                                 if (Model.Status.Equals("created", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Your Account Has Been Created.</span>\r\n                                    <p>\r\n                                        You may now login.\r\n                                    </p>\r\n");
#line 65 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 66 "Login.cshtml"
                                 if (Model.Status.Equals("reset", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Password Reset Successfully.</span>\r\n                                    <p>\r\n                                        You can now login with your new password.\r\n                                    </p>\r\n");
#line 72 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 73 "Login.cshtml"
                                 if (Model.Status.Equals("forgot", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral(@"                                    <span>Password Reset Requested.</span>
                                    <p>
                                        If an account exists for the email provided, you will receive an email shortly.
                                    </p>
");
#line 79 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                            </div>\r\n                        </div>\r\n                    </div>\r\n");
#line 83 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("                <br>\r\n                <div class=\"box row\">\r\n                    <div");
            BeginWriteAttribute("class", " class=\"", 4783, "\"", 4881, 3);
            WriteAttributeValue("", 4791, "email-password-area", 4791, 19, true);
            WriteAttributeValue(" ", 4810, "col-xs-12", 4811, 10, true);
#line 86 "Login.cshtml"
WriteAttributeValue(" ", 4820, hasSocialProviders ? "small col-sm-8" : "large col-sm-12", 4821, 61, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n");
#line 87 "Login.cshtml"
                        

#line default
#line hidden

#line 87 "Login.cshtml"
                         if (Model.RegistrationEnabled)
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"header\">\r\n                                <span>Log In or <a");
            BeginWriteAttribute("href", " href=\"", 5069, "\"", 5094, 1);
#line 90 "Login.cshtml"
WriteAttributeValue("", 5076, Model.RegisterUri, 5076, 18, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">Create Account</a></span>\r\n                            </div>\r\n");
#line 92 "Login.cshtml"
                        }
                        else
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"header\">\r\n                                <span>Log In</span>\r\n                            </div>\r\n");
#line 98 "Login.cshtml"
                        }

#line default
#line hidden

            WriteLiteral("                        ");
#line 99 "Login.cshtml"
                         if (Model.Errors.Any())
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"alert alert-danger bad-login\">\r\n");
#line 102 "Login.cshtml"
                                

#line default
#line hidden

#line 102 "Login.cshtml"
                                 foreach (var error in Model.Errors)
                                {

#line default
#line hidden

            WriteLiteral("                                    <p>");
#line 104 "Login.cshtml"
                                  Write(error);

#line default
#line hidden
            WriteLiteral("</p>\r\n");
#line 105 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                            </div>\r\n");
#line 107 "Login.cshtml"
                        }

#line default
#line hidden

            WriteLiteral("                        <form method=\"post\" role=\"form\" class=\"login-form form-horizontal\">\r\n\r\n");
#line 111 "Login.cshtml"
                            

#line default
#line hidden

#line 111 "Login.cshtml"
                             foreach (var field in Model.Form.Fields)
                            {

#line default
#line hidden

            WriteLiteral("                                <div");
            BeginWriteAttribute("class", " class=\"", 6115, "\"", 6158, 2);
            WriteAttributeValue("", 6123, "form-group", 6123, 10, true);
#line 113 "Login.cshtml"
WriteAttributeValue(" ", 6133, $"group-{field.Name}", 6134, 25, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n                                    <label");
            BeginWriteAttribute("class", " class=\"", 6204, "\"", 6260, 1);
#line 114 "Login.cshtml"
WriteAttributeValue("", 6212, hasSocialProviders ? "col-sm-12" : "col-sm-4", 6212, 48, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n                                        ");
#line 115 "Login.cshtml"
                                   Write(field.Label);

#line default
#line hidden
            WriteLiteral("\r\n                                    </label>\r\n                                    <div");
            BeginWriteAttribute("class", " class=\"", 6404, "\"", 6460, 1);
#line 117 "Login.cshtml"
WriteAttributeValue("", 6412, hasSocialProviders ? "col-sm-12" : "col-sm-8", 6412, 48, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n                                        <input");
            BeginWriteAttribute("placeholder", " placeholder=\"", 6609, "\"", 6641, 1);
#line 119 "Login.cshtml"
WriteAttributeValue("", 6623, field.Placeholder, 6623, 18, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("name", "\r\n                                               name=\"", 6642, "\"", 6708, 1);
#line 120 "Login.cshtml"
WriteAttributeValue("", 6697, field.Name, 6697, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("type", "\r\n                                               type=\"", 6709, "\"", 6775, 1);
#line 121 "Login.cshtml"
WriteAttributeValue("", 6764, field.Type, 6764, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("value", "\r\n                                               value=\"", 6776, "\"", 6881, 1);
#line 122 "Login.cshtml"
WriteAttributeValue("", 6832, Model.FormData.Get(field.Name) ?? string.Empty, 6832, 49, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral("\r\n                                               class=\"form-control\"\r\n                                               ");
#line 124 "Login.cshtml"
                                           Write(field.Required ? "required" : string.Empty);

#line default
#line hidden
            WriteLiteral(">\r\n                                    </div>\r\n                                </div>\r\n");
#line 127 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("                            <div>\r\n                                <button type=\"submit\" class=\"login btn btn-login btn-sp-green\">Log In</button>\r\n                            </div>\r\n                        </form>\r\n                    </div>\r\n");
#line 133 "Login.cshtml"
                    

#line default
#line hidden

#line 133 "Login.cshtml"
                     if (hasSocialProviders)
                    {

#line default
#line hidden

            WriteLiteral("                        <div class=\"social-area col-xs-12 col-sm-4\">\r\n                            <div class=\"header\">&nbsp;</div>\r\n                            <label>Easy 1-click login:</label>\r\n");
#line 138 "Login.cshtml"
                            

#line default
#line hidden

#line 138 "Login.cshtml"
                             if (facebookStore != null)
                            {

#line default
#line hidden

            WriteLiteral(@"                                <button class=""btn btn-social btn-facebook"" onclick=""facebookLogin()"">Facebook</button>
                                <script type=""text/javascript"">
                                    function facebookLogin() {
                                        var FB = window.FB;
                                        var facebookScope = '");
#line 144 "Login.cshtml"
                                                        Write(facebookStore.Provider.Scope);

#line default
#line hidden
            WriteLiteral(@"';

                                        FB.login(function (response) {
                                            if (response.status === 'connected') {
                                                var queryStr = window.location.search.replace('?', '');
                                                // TODO make dynamic
                                                if (queryStr) {
                                                    // Don't include any access_token parameters in
                                                    // the query string as it will be added by us.
                                                    queryStr = queryStr.replace(/(&?)access_token=([^&]*)/, '');

                                                    window.location.replace('");
#line 155 "Login.cshtml"
                                                                        Write(facebookStore.Href);

#line default
#line hidden
            WriteLiteral("?\' + queryStr + \'&access_token=\' + response.authResponse.accessToken);\r\n                                                } else {\r\n                                                    window.location.replace(\'");
#line 157 "Login.cshtml"
                                                                        Write(facebookStore.Href);

#line default
#line hidden
            WriteLiteral(@"?access_token=' + response.authResponse.accessToken);
                                                }
                                            }
                                        }, { scope: facebookScope });
                                    }

                                    window.fbAsyncInit = function () {
                                        FB.init({
                                            appId: '");
#line 165 "Login.cshtml"
                                               Write(facebookStore.Provider.ClientId);

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
#line 180 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 182 "Login.cshtml"
                            

#line default
#line hidden

#line 182 "Login.cshtml"
                             if (googleStore != null)
                            {

#line default
#line hidden

            WriteLiteral(@"                                <button class=""btn btn-social btn-google"" onclick=""googleLogin()"">Google</button>
                                <script type=""text/javascript"">
                                    if (!location.origin)
                                        location.origin = location.protocol + ""//"" + location.host;

                                    function googleLogin() {
                                        var clientId = '");
#line 190 "Login.cshtml"
                                                   Write(googleStore.Provider.ClientId);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var googleScope = \'");
#line 191 "Login.cshtml"
                                                      Write(googleStore.Provider.Scope);

#line default
#line hidden
            WriteLiteral(@"';

                                        var finalUrl = 'https://accounts.google.com/o/oauth2/auth?response_type=code&client_id=' +
                                            clientId +
                                            '&scope=' + googleScope +
                                            '&include_granted_scopes=true&redirect_uri=' +
                                            location.origin + '");
#line 197 "Login.cshtml"
                                                          Write(googleStore.Href);

#line default
#line hidden
            WriteLiteral("\';\r\n\r\n                                        window.location = finalUrl;\r\n                                    }\r\n                                </script>\r\n");
#line 202 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 204 "Login.cshtml"
                            

#line default
#line hidden

#line 204 "Login.cshtml"
                             if (githubStore != null)
                            {

#line default
#line hidden

            WriteLiteral(@"                                <button class=""btn btn-social btn-github"" onclick=""githubLogin()"">Github</button>
                                <script type=""text/javascript"">
                                    if (!location.origin)
                                        location.origin = location.protocol + ""//"" + location.host;

                                    function githubLogin() {
                                        var clientId = '");
#line 212 "Login.cshtml"
                                                   Write(githubStore.Provider.ClientId);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var gitHubScope = \'");
#line 213 "Login.cshtml"
                                                      Write(githubStore.Provider.Scope);

#line default
#line hidden
            WriteLiteral("\';\r\n                                        var redirectUri = location.origin + \'");
#line 214 "Login.cshtml"
                                                                        Write(githubStore.Href);

#line default
#line hidden
            WriteLiteral(@"';

                                        var url = 'https://github.com/login/oauth/authorize' +
                                            '?client_id=' + encodeURIComponent(clientId) +
                                            '&scope=' + encodeURIComponent(gitHubScope) +
                                            '&redirect_uri=' + encodeURIComponent(redirectUri);

                                        window.location = url;
                                    }
                                </script>
");
#line 224 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("                        </div>\r\n");
#line 226 "Login.cshtml"
                    }

#line default
#line hidden

            WriteLiteral("                </div>\r\n");
#line 228 "Login.cshtml"
                

#line default
#line hidden

#line 228 "Login.cshtml"
                 if (Model.VerifyEmailEnabled)
                {

#line default
#line hidden

            WriteLiteral("                    <a style=\"float:left\"");
            BeginWriteAttribute("href", " href=\"", 13373, "\"", 13401, 1);
#line 230 "Login.cshtml"
WriteAttributeValue("", 13380, Model.VerifyEmailUri, 13380, 21, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"forgot\">Resend Verification Email?</a>\r\n");
#line 231 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 233 "Login.cshtml"
                

#line default
#line hidden

#line 233 "Login.cshtml"
                 if (Model.ForgotPasswordEnabled)
                {

#line default
#line hidden

            WriteLiteral("                    <a style=\"float:right\"");
            BeginWriteAttribute("href", " href=\"", 13583, "\"", 13614, 1);
#line 235 "Login.cshtml"
WriteAttributeValue("", 13590, Model.ForgotPasswordUri, 13590, 24, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"forgot\">Forgot Password?</a>\r\n");
#line 236 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("            </div>\r\n        </div>\r\n    </div>\r\n    <script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js\"></script>\r\n    <script src=\"//netdna.bootstrapcdn.com/bootstrap/3.1.1/js/bootstrap.min.js\"></script>\r\n</body>\r\n</html>");
        }
        #pragma warning restore 1998
    }
}
