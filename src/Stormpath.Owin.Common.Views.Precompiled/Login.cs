namespace Stormpath.Owin.Common.Views.Precompiled
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
using Stormpath.Owin.Common

#line default
#line hidden
    ;
    using System.Threading.Tasks;

    public class Login : BaseView<Stormpath.Owin.Common.ViewModel.ExtendedLoginViewModel>
    {
#line 5 "Login.cshtml"

    private new Stormpath.Owin.Common.ViewModel.ExtendedLoginViewModel Model { get; }

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
        html,\r\nbody {\r\n  height: 100%;\r\n}\r\n\r\n@media (max-width: 767px) {\r\n  html,\r\n  body {\r\n    padding: 0 4px;\r\n  }\r\n}\r\n\r\nbody {\r\n  margin-left: auto;\r\n  margin-right: auto;\r\n}\r\n\r\nbody,\r\ndiv,\r\np,\r\na,\r\nlabel {\r\n  font-family: 'Open Sans';\r\n  font-size: 14px;\r\n  font-weight: 400;\r\n  color: #484848;\r\n}\r\n\r\na {\r\n  color: #0072dd;\r\n}\r\n\r\np {\r\n  line-height: 21px;\r\n}\r\n\r\n.container {\r\n  max-width: 620px;\r\n}\r\n\r\n.logo {\r\n  margin: 34px auto 25px auto;\r\n  display: block;\r\n}\r\n\r\n.btn-sp-green {\r\n  height: 45px;\r\n  line-height: 22.5px;\r\n  padding: 0 40px;\r\n  color: #fff;\r\n  font-size: 17px;\r\n  background: -webkit-linear-gradient(#42c41a 50%, #2dbd00 50%);\r\n  background: linear-gradient(#42c41a 50%, #2dbd00 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#2dbd00, endColorstr=#42c41a);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#2dbd00, endColorstr=#42c41a)';\r\n}\r\n\r\n.btn-sp-green:hover,\r\n.btn-sp-green:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#43cd1a 50%, #2ec700 50%);\r\n  background: linear-gradient(#43cd1a 50%, #2ec700 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#2ec700, endColorstr=#43cd1a);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#2ec700, endColorstr=#43cd1a)';\r\n}\r\n\r\n.btn-social {\r\n  height: 37px;\r\n  line-height: 18.5px;\r\n  color: #fff;\r\n  font-size: 16px;\r\n  border-radius: 3px;\r\n}\r\n\r\n.btn-social:hover,\r\n.btn-social:focus {\r\n  color: #fff;\r\n}\r\n\r\n.btn-facebook {\r\n  background: -webkit-linear-gradient(#4c6fc5 50%, #3d63c0 50%);\r\n  background: linear-gradient(#4c6fc5 50%, #3d63c0 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#3d63c0, endColorstr=#4c6fc5);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#3d63c0, endColorstr=#4c6fc5)';\r\n}\r\n\r\n.btn-facebook:hover,\r\n.btn-facebook:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#4773de 50%, #3767db 50%);\r\n  background: linear-gradient(#4773de 50%, #3767db 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#3767db, endColorstr=#4773de);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#3767db, endColorstr=#4773de)';\r\n}\r\n\r\n.btn-google {\r\n  background: -webkit-linear-gradient(#e05b4b 50%, #dd4b39 50%);\r\n  background: linear-gradient(#e05b4b 50%, #dd4b39 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#dd4b39, endColorstr=#e05b4b);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#dd4b39, endColorstr=#e05b4b)';\r\n}\r\n\r\n.btn-google:hover,\r\n.btn-google:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#ea604e 50%, #e8503c 50%);\r\n  background: linear-gradient(#ea604e 50%, #e8503c 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#e8503c, endColorstr=#ea604e);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#e8503c, endColorstr=#ea604e)';\r\n}\r\n\r\n.btn-linkedin {\r\n  background: -webkit-linear-gradient(#007cbc 50%, #0077B5 50%);\r\n  background: linear-gradient(#007cbc 50%, #0077B5 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5)';\r\n}\r\n\r\n.btn-linkedin:hover,\r\n.btn-linkedin:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#007cbc 50%, #0077B5 50%);\r\n  background: linear-gradient(#007cbc 50%, #0077B5 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5)';\r\n}\r\n\r\n.btn-github {\r\n  background: -webkit-linear-gradient(#848282 50%, #7B7979 50%);\r\n  background: linear-gradient(#848282 50%, #7B7979 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#848282, endColorstr=#7B7979);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#848282, endColorstr=#7B7979)';\r\n}\r\n\r\n.btn-github:hover,\r\n.btn-github:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#8C8888 50%, #848080 50%);\r\n  background: linear-gradient(#8C8888 50%, #848080 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#8C8888, endColorstr=#848080);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#8C8888, endColorstr=#848080)';\r\n}\r\n\r\n.btn-register {\r\n  font-size: 16px;\r\n}\r\n\r\n.form-control {\r\n  font-size: 15px;\r\n  box-shadow: none;\r\n}\r\n\r\n.form-control::-webkit-input-placeholder {\r\n  color: #aaadb0;\r\n}\r\n\r\n.form-control::-moz-placeholder {\r\n  color: #aaadb0;\r\n}\r\n\r\n.form-control:-ms-input-placeholder {\r\n  color: #aaadb0;\r\n}\r\n\r\n.form-control::placeholder {\r\n  color: #aaadb0;\r\n}\r\n\r\n.form-control:focus {\r\n  box-shadow: inset 0 1px 1px rgba(0, 0, 0, 0.075), 0 0 6px rgba(0, 132, 255, 0.4);\r\n}\r\n\r\n.view .header {\r\n  padding: 34px 0;\r\n}\r\n\r\n.view .header,\r\n.view .header a {\r\n  font-weight: 300;\r\n  font-size: 21px;\r\n}\r\n\r\n.view input[type='text'],\r\n.view input[type='password'],\r\n.view input[type='email'],\r\n.view input[type='color'],\r\n.view input[type='date'],\r\n.view input[type='datetime']\r\n.view input[type='datetime-local'],\r\n.view input[type='email'],\r\n.view input[type='month'],\r\n.view input[type='number'],\r\n.view input[type='range'],\r\n.view input[type='search'],\r\n.view input[type='tel'],\r\n.view input[type='time'],\r\n.view input[type='url'],\r\n.view input[type='week']{\r\n  background-color: #f6f6f6;\r\n  height: 45px;\r\n}\r\n\r\n.view a.forgot,\r\n.view a.to-login {\r\n  float: right;\r\n  padding: 17px 0;\r\n  font-size: 13px;\r\n}\r\n\r\n.view form button {\r\n  display: block;\r\n  float: right;\r\n  margin-bottom: 25px;\r\n}\r\n\r\n.view form label {\r\n  height: 45px;\r\n  line-height: 45px;\r\n}\r\n\r\n.box {\r\n  box-shadow: 0 0px 3px 1px rgba(0, 0, 0, 0.1);\r\n  border: 1px solid #cacaca;\r\n  border-radius: 3px;\r\n  padding: 0 30px;\r\n}\r\n\r\n.sp-form .has-error,\r\n.sp-form .has-error .help-block {\r\n  color: #ec3e3e;\r\n  font-weight: 600;\r\n}\r\n\r\n.sp-form .has-error input[type='text'],\r\n.sp-form .has-error input[type='password'] {\r\n  border-color: #ec3e3e;\r\n}\r\n\r\n.sp-form .form-group {\r\n  margin-bottom: 21px;\r\n}\r\n\r\n.sp-form input[type='text'],\r\n.sp-form input[type='password'] {\r\n  position: relative;\r\n}\r\n\r\n.sp-form .help-block {\r\n  font-size: 12px;\r\n  position: absolute;\r\n  top: 43px;\r\n}\r\n\r\n.verify-view .box {\r\n  padding-bottom: 30px;\r\n}\r\n\r\n.verify-view .box .header {\r\n  padding-bottom: 20px;\r\n}\r\n\r\n.unverified-view .box {\r\n  padding-bottom: 30px;\r\n}\r\n\r\n.unverified-view .box .header {\r\n  padding-bottom: 25px;\r\n}\r\n\r\n.login-view .box {\r\n  background-color: #f6f6f6;\r\n  padding: 0;\r\n}\r\n\r\n.login-view label {\r\n  margin-bottom: 7px;\r\n}\r\n\r\n.login-view .header p {\r\n  margin-top: 2em;\r\n}\r\n\r\n.login-view .email-password-area {\r\n  background-color: white;\r\n  border-top-left-radius: 3px;\r\n  border-bottom-left-radius: 3px;\r\n}\r\n\r\n@media (min-width: 767px) {\r\n  .login-view .email-password-area {\r\n    padding: 0 30px;\r\n  }\r\n}\r\n\r\n.login-view .email-password-area label {\r\n  height: 14px;\r\n  line-height: 14px;\r\n}\r\n\r\n.login-view .email-password-area input[type='checkbox'] {\r\n  visibility: hidden;\r\n}\r\n\r\n.login-view .email-password-area input[type='checkbox'] + label {\r\n  position: relative;\r\n  padding-left: 8px;\r\n  line-height: 16px;\r\n  font-size: 13px;\r\n}\r\n\r\n.login-view .email-password-area input[type='checkbox'] + label:after {\r\n  position: absolute;\r\n  left: -16px;\r\n  width: 16px;\r\n  height: 16px;\r\n  border: 1px solid #cacaca;\r\n  background-color: #f6f6f6;\r\n  content: '';\r\n}\r\n\r\n.login-view .email-password-area input[type='checkbox']:checked + label:after {\r\n  background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAA2ZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowRTVBQUVGMzJEODBFMjExODQ2N0NBMjk4MjdCNDBCNyIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDo0RTY4NUM4NURGNEYxMUUyQUE5QkExOTlGODU3RkFEOCIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo0RTY4NUM4NERGNEYxMUUyQUE5QkExOTlGODU3RkFEOCIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M2IChXaW5kb3dzKSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjQxNDQ4M0NEM0JERkUyMTE4MEYwQjNBRjIwMUNENzQxIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOkZDMEMxNjY2OUVCMUUyMTFBRjVDQkQ0QjE5MTNERDU2Ii8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+3YY4qgAAALlJREFUeNpi/P//PwMlgImBQjDwBrCgCwQHB+NUfObMGT9mZuboe/fuheM0ABu4fv060/fv32cBNTNycHBE4nUBNs0/f/7cAWSeMzQ0rCA5DICaNwKj+qGRkVEFUYF47ty5GWfPns2EsjsYGRlFgM5OJzoQ//37t5eLi2sRMMDec3Jypn79+lVXX1//H9HRaGJisvr379/nuLm5lwKdP9vMzOwZyekAaEA3EF8G4hZCYcQ4mhcYAAIMAJGST/dDIpNQAAAAAElFTkSuQmCC);\r\n  background-position: -1px -1px;\r\n}\r\n\r\n@media (min-width: 767px) {\r\n  .login-view .email-password-area.small {\r\n    border-right: 1px solid #cacaca;\r\n  }\r\n\r\n  .login-view .email-password-area.small .group-email {\r\n    margin-bottom: 21px;\r\n  }\r\n}\r\n\r\n@media (max-width: 767px) {\r\n  .login-view .email-password-area.small {\r\n    border-bottom: 1px solid #cacaca;\r\n    border-bottom-left-radius: 0;\r\n    border-bottom-right-radius: 0;\r\n  }\r\n}\r\n\r\n.login-view .email-password-area.large {\r\n  border-top-right-radius: 3px;\r\n  border-bottom-right-radius: 3px;\r\n}\r\n\r\n@media (min-width: 767px) {\r\n  .login-view .email-password-area.large {\r\n    padding: 0 50px;\r\n  }\r\n\r\n  .login-view .email-password-area.large .group-login label,\r\n  .login-view .email-password-area.large .group-password label {\r\n    height: 45px;\r\n    line-height: 45px;\r\n  }\r\n}\r\n\r\n.login-view .social-area {\r\n  border-top-right-radius: 3px;\r\n  border-bottom-right-radius: 3px;\r\n  padding: 0 20px;\r\n  position: relative;\r\n  padding-bottom: 20px;\r\n  background-color: #f6f6f6;\r\n}\r\n\r\n.login-view .social-area .header {\r\n  margin-bottom: -6px;\r\n}\r\n\r\n@media (max-width: 767px) {\r\n  .login-view .social-area .header {\r\n    padding: 0px;\r\n  }\r\n}\r\n\r\n.login-view .social-area button {\r\n  display: block;\r\n  width: 100%;\r\n  margin-bottom: 15px;\r\n}\r\n\r\n.login, .register { display: table; }\r\n.va-wrapper { display: table-cell; width: 100%; vertical-align: middle; }\r\n.custom-container { display: table-row; height: 100%; }
    </style>
    <!--[if lt IE 9]>
     <script src='https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js'></script>
     <script src='https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js'></script>
    <");
            WriteLiteral("![endif]-->\r\n</head>\r\n<body class=\"login\">\r\n    <div class=\"container custom-container\">\r\n        <div class=\"va-wrapper\">\r\n            <div class=\"view login-view container\">\r\n");
#line 34 "Login.cshtml"
                

#line default
#line hidden

#line 34 "Login.cshtml"
                 if (Stormpath.Owin.Common.ViewModel.ExtendedLoginViewModel.AcceptableStatuses.Any(x => x.Equals(Model.Status, StringComparison.OrdinalIgnoreCase))) {

#line default
#line hidden

            WriteLiteral("                    <div class=\"box row\">\r\n                        <div class=\"email-password-area col-xs-12 large col-sm-12\">\r\n                            <div class=\"header\">\r\n");
#line 38 "Login.cshtml"
                                

#line default
#line hidden

#line 38 "Login.cshtml"
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
            BeginWriteAttribute("href", " href=\"", 2303, "\"", 2331, 1);
#line 45 "Login.cshtml"
WriteAttributeValue("", 2310, Model.VerifyEmailUri, 2310, 21, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">Click Here</a>.</p>\r\n                                    <br>\r\n");
#line 47 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 48 "Login.cshtml"
                                 if (Model.Status.Equals("verified", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Your Account Has Been Verified.</span>\r\n                                    <p>\r\n                                        You may now login.\r\n                                    </p>\r\n");
#line 54 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 55 "Login.cshtml"
                                 if (Model.Status.Equals("created", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Your Account Has Been Created.</span>\r\n                                    <p>\r\n                                        You may now login.\r\n                                    </p>\r\n");
#line 61 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 62 "Login.cshtml"
                                 if (Model.Status.Equals("reset", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral("                                    <span>Password Reset Successfully.</span>\r\n                                    <p>\r\n                                        You can now login with your new password.\r\n                                    </p>\r\n");
#line 68 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                                ");
#line 69 "Login.cshtml"
                                 if (Model.Status.Equals("forgot", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral(@"                                    <span>Password Reset Requested.</span>
                                    <p>
                                        If an account exists for the email provided, you will receive an email shortly.
                                    </p>
");
#line 75 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                            </div>\r\n                        </div>\r\n                    </div>\r\n");
#line 79 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("                <br>\r\n                <div class=\"box row\">\r\n                    <div class=\"email-password-area col-xs-12 large col-sm-12\"> \r\n");
#line 83 "Login.cshtml"
                        

#line default
#line hidden

#line 83 "Login.cshtml"
                         if (Model.RegistrationEnabled)
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"header\">\r\n                                <span>Log In or <a");
            BeginWriteAttribute("href", " href=\"", 4615, "\"", 4640, 1);
#line 86 "Login.cshtml"
WriteAttributeValue("", 4622, Model.RegisterUri, 4622, 18, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">Create Account</a></span>\r\n                            </div>\r\n");
#line 88 "Login.cshtml"
                        }
                        else
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"header\">\r\n                                <span>Log In</span>\r\n                            </div>\r\n");
#line 94 "Login.cshtml"
                        }

#line default
#line hidden

            WriteLiteral("                        ");
#line 95 "Login.cshtml"
                         if (Model.Errors.Any())
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"alert alert-danger bad-login\">\r\n");
#line 98 "Login.cshtml"
                                

#line default
#line hidden

#line 98 "Login.cshtml"
                                 foreach (var error in Model.Errors)
                                {

#line default
#line hidden

            WriteLiteral("                                    <p>");
#line 100 "Login.cshtml"
                                  Write(error);

#line default
#line hidden
            WriteLiteral("</p>\r\n");
#line 101 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                            </div>\r\n");
#line 103 "Login.cshtml"
                        }

#line default
#line hidden

            WriteLiteral("                        <form method=\"post\" role=\"form\" class=\"login-form form-horizontal\">\r\n\r\n");
#line 107 "Login.cshtml"
                            

#line default
#line hidden

#line 107 "Login.cshtml"
                             foreach (var field in Model.Form.Fields)
                            {

#line default
#line hidden

            WriteLiteral("                                <div");
            BeginWriteAttribute("class", " class=\"", 5661, "\"", 5704, 2);
            WriteAttributeValue("", 5669, "form-group", 5669, 10, true);
#line 109 "Login.cshtml"
WriteAttributeValue(" ", 5679, $"group-{field.Name}", 5680, 25, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n                                    <label class=\"col-sm-4\">");
#line 110 "Login.cshtml"
                                                       Write(field.Label);

#line default
#line hidden
            WriteLiteral("</label> \r\n                                    <div class=\"col-sm-8\">\r\n                                        <input");
            BeginWriteAttribute("placeholder", " placeholder=\"", 6053, "\"", 6085, 1);
#line 113 "Login.cshtml"
WriteAttributeValue("", 6067, field.Placeholder, 6067, 18, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("name", "\r\n                                               name=\"", 6086, "\"", 6152, 1);
#line 114 "Login.cshtml"
WriteAttributeValue("", 6141, field.Name, 6141, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("type", "\r\n                                               type=\"", 6153, "\"", 6219, 1);
#line 115 "Login.cshtml"
WriteAttributeValue("", 6208, field.Type, 6208, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("value", "\r\n                                               value=\"", 6220, "\"", 6325, 1);
#line 116 "Login.cshtml"
WriteAttributeValue("", 6276, Model.FormData.Get(field.Name) ?? string.Empty, 6276, 49, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral("\r\n                                               class=\"form-control\"\r\n                                               ");
#line 118 "Login.cshtml"
                                           Write(field.Required ? "required" : string.Empty);

#line default
#line hidden
            WriteLiteral(">\r\n                                    </div>\r\n                                </div>\r\n");
#line 121 "Login.cshtml"
                            }

#line default
#line hidden

            WriteLiteral(@"                            <div>
                                <button type=""submit"" class=""login btn btn-login btn-sp-green"">Log In</button>
                            </div>
                        </form>
                    </div>
                </div>
");
#line 129 "Login.cshtml"
                

#line default
#line hidden

#line 129 "Login.cshtml"
                 if (Model.VerifyEmailEnabled)
                {

#line default
#line hidden

            WriteLiteral("                    <a style=\"float:right\"");
            BeginWriteAttribute("href", " href=\"", 7035, "\"", 7063, 1);
#line 131 "Login.cshtml"
WriteAttributeValue("", 7042, Model.VerifyEmailUri, 7042, 21, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"forgot\">Forgot Password?</a>\r\n");
#line 132 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 134 "Login.cshtml"
                

#line default
#line hidden

#line 134 "Login.cshtml"
                 if (Model.ForgotPasswordEnabled)
                {

#line default
#line hidden

            WriteLiteral("                    <a style=\"float:right\"");
            BeginWriteAttribute("href", " href=\"", 7235, "\"", 7266, 1);
#line 136 "Login.cshtml"
WriteAttributeValue("", 7242, Model.ForgotPasswordUri, 7242, 24, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"forgot\">Forgot Password?</a>\r\n");
#line 137 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral(@"                
            </div>
        </div>
    </div>
    <script src=""https://ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js""></script>
    <script src=""//netdna.bootstrapcdn.com/bootstrap/3.1.1/js/bootstrap.min.js""></script>
</body>
</html>");
        }
        #pragma warning restore 1998
    }
}
