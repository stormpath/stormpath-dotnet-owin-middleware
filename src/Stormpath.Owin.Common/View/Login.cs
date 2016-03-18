namespace Stormpath.Owin.Common.View
{
#line 1 "Login.cshtml"
using System

#line default
#line hidden
    ;
    using System.Threading.Tasks;

    public class Login : BaseView<Stormpath.Owin.Common.ViewModel.LoginViewModelExtended>
    {
        #line hidden
        public Login()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
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
  font-family: ""Open Sans"";
  font-size: 14px;
 ");
            WriteLiteral(@" font-weight: 400;
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
  -ms-filter: ""progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#2dbd00, endColorstr=#42c41a)"";
}

.btn-sp-green:hover,
.btn-sp-green:focus {
  color: #fff;
  background: -webkit-linear-gradient(#43cd1a 50%, #2ec700 50%);
  background: linear-gradient(#43cd1a 50%, #2ec700 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#2ec700, endColorstr=#43cd1a);
  -ms-filter: ""progid:DXImageTrans");
            WriteLiteral(@"form.Microsoft.gradient (GradientType=0, startColorstr=#2ec700, endColorstr=#43cd1a)"";
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
  -ms-filter: ""progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#3d63c0, endColorstr=#4c6fc5)"";
}

.btn-facebook:hover,
.btn-facebook:focus {
  color: #fff;
  background: -webkit-linear-gradient(#4773de 50%, #3767db 50%);
  background: linear-gradient(#4773de 50%, #3767db 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#3767db, endColorstr=#4773de);
  -ms-filter: ""progid:DXImageTransform.Microsoft.gradient (Gradien");
            WriteLiteral(@"tType=0, startColorstr=#3767db, endColorstr=#4773de)"";
}

.btn-google {
  background: -webkit-linear-gradient(#e05b4b 50%, #dd4b39 50%);
  background: linear-gradient(#e05b4b 50%, #dd4b39 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#dd4b39, endColorstr=#e05b4b);
  -ms-filter: ""progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#dd4b39, endColorstr=#e05b4b)"";
}

.btn-google:hover,
.btn-google:focus {
  color: #fff;
  background: -webkit-linear-gradient(#ea604e 50%, #e8503c 50%);
  background: linear-gradient(#ea604e 50%, #e8503c 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#e8503c, endColorstr=#ea604e);
  -ms-filter: ""progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#e8503c, endColorstr=#ea604e)"";
}

.btn-linkedin {
  background: -webkit-linear-gradient(#007cbc 50%, #0077B5 50%);
  background: linear-gradient(#007cbc 50%, #0077B5 50%);
  filter: progid:D");
            WriteLiteral(@"XImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5);
  -ms-filter: ""progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5)"";
}

.btn-linkedin:hover,
.btn-linkedin:focus {
  color: #fff;
  background: -webkit-linear-gradient(#007cbc 50%, #0077B5 50%);
  background: linear-gradient(#007cbc 50%, #0077B5 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5);
  -ms-filter: ""progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5)"";
}

.btn-github {
  background: -webkit-linear-gradient(#848282 50%, #7B7979 50%);
  background: linear-gradient(#848282 50%, #7B7979 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#848282, endColorstr=#7B7979);
  -ms-filter: ""progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#848282, endColorstr=#7B7979)""");
            WriteLiteral(@";
}

.btn-github:hover,
.btn-github:focus {
  color: #fff;
  background: -webkit-linear-gradient(#8C8888 50%, #848080 50%);
  background: linear-gradient(#8C8888 50%, #848080 50%);
  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#8C8888, endColorstr=#848080);
  -ms-filter: ""progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#8C8888, endColorstr=#848080)"";
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

.view in");
            WriteLiteral(@"put[type=""text""],
.view input[type=""password""],
.view input[type=""email""],
.view input[type=""color""],
.view input[type=""date""],
.view input[type=""datetime""]
.view input[type=""datetime-local""],
.view input[type=""email""],
.view input[type=""month""],
.view input[type=""number""],
.view input[type=""range""],
.view input[type=""search""],
.view input[type=""tel""],
.view input[type=""time""],
.view input[type=""url""],
.view input[type=""week""]{
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

.sp-form .has-error input[type=""text""]");
            WriteLiteral(@",
.sp-form .has-error input[type=""password""] {
  border-color: #ec3e3e;
}

.sp-form .form-group {
  margin-bottom: 21px;
}

.sp-form input[type=""text""],
.sp-form input[type=""password""] {
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
");
            WriteLiteral(@"
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
  content: """";
}

.login-view .email-password-area input[type='checkbox']:checked + label:after {
  background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAA2ZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1ze");
            WriteLiteral(@"W50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowRTVBQUVGMzJEODBFMjExODQ2N0NBMjk4MjdCNDBCNyIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDo0RTY4NUM4NURGNEYxMUUyQUE5QkExOTlGODU3RkFEOCIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo0RTY4NUM4NERGNEYxMUUyQUE5QkExOTlGODU3RkFEOCIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M2IChXaW5kb3dzKSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjQxNDQ4M0NEM0JERkUyMTE4MEYwQjNBRjIwMUNENzQxIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOkZDMEMxNjY2OUVCMUUyMTFBRjVDQkQ0QjE5MTNERDU2Ii8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+3YY4qgAAALlJREFUeNpi/P//PwMlgImBQjDwBrCgCwQHB+NUfObMGT9mZuboe/fuheM0ABu4fv060/fv32cBNTNycHBE4nUBNs0/f/7cAWSeMzQ0rCA5DICaNwKj+qGRkVEFUYF47ty5GWfPns2EsjsYGRlFgM5OJzoQ//37t5eLi2sRMMDec3Jyp");
            WriteLiteral(@"n79+lVXX1//H9HRaGJisvr379/nuLm5lwKdP9vMzOwZyekAaEA3EF8G4hZCYcQ4mhcYAAIMAJGST/dDIpNQAAAAAElFTkSuQmCC);
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

  .login-view .email-password-area.large .group-email label,
  .login-view .email-password-area.large .group-password label {
    height: 45px;
    line-height: 45px;
  }
}

.login-view .social-area {
  border-top-right-radius: 3px;
  border-bottom-r");
            WriteLiteral(@"ight-radius: 3px;
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
    <![endif]-->
</head>
<body class=""login"">
    <div class=""container custom-container"">
        <div class=""va-wrapper"">
            <div class=""view login-view container"">
");
#line 415 "Login.cshtml"
                

#line default
#line hidden

#line 415 "Login.cshtml"
                 if (!string.IsNullOrEmpty(Model.Status)) {

#line default
#line hidden

            WriteLiteral("                    <div class=\"box row\">\r\n                        <div class=\"email-password-area col-xs-12 large col-sm-12\">\r\n                            <div class=\"header\">\r\n");
#line 419 "Login.cshtml"
                                

#line default
#line hidden

#line 419 "Login.cshtml"
                                 if (Model.Status.Equals("unverified", StringComparison.OrdinalIgnoreCase))
                                {

#line default
#line hidden

            WriteLiteral(@"                                    <span>Your account verification email has been sent!</span>
                                    <p>
                                        Before you can log into your account, you need to activate your
                                        account by clicking the link we sent to your inbox.
                                    </p>
                                    <p>Didn't get the email? <a href=""/verify"">Click Here</a>.</p>
                                    <br>
");
#line 428 "Login.cshtml"
                                }

#line default
#line hidden

            WriteLiteral("                            </div>\r\n                        </div>\r\n                    </div>\r\n");
#line 432 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("                <br>\r\n                <div class=\"box row\">\r\n                    <div class=\"email-password-area col-xs-12 large col-sm-12\"> \r\n");
#line 436 "Login.cshtml"
                        

#line default
#line hidden

#line 436 "Login.cshtml"
                         if (Model.RegistrationEnabled)
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"header\">\r\n                                <span>Log In or <a href=\"/register\">Create Account</a></span>\r\n                            </div>\r\n");
#line 441 "Login.cshtml"
                        }
                        else
                        {

#line default
#line hidden

            WriteLiteral("                            <div class=\"header\">\r\n                                <span>Log In</span>\r\n                            </div>\r\n");
#line 447 "Login.cshtml"
                        }

#line default
#line hidden

            WriteLiteral("                        \r\n                        <form method=\"post\" role=\"form\" action=\"/login\" class=\"login-form form-horizontal\">\r\n                            <div class=\"form-group group-email\">\r\n");
#line 452 "Login.cshtml"
                                

#line default
#line hidden

#line 452 "Login.cshtml"
                                 if (Model.DisplayUsernameOrEmail)
                                {

#line default
#line hidden

            WriteLiteral("                                    <label class=\"col-sm-4\">Username or Email</label> ");
#line 454 "Login.cshtml"
                                                                                      

#line default
#line hidden

#line 454 "Login.cshtml"
                                                                                                                                               
                                }
                                else
                                {

#line default
#line hidden

            WriteLiteral("                                    <label class=\"col-sm-4\">Email</label> ");
#line 458 "Login.cshtml"
                                                                          

#line default
#line hidden

#line 458 "Login.cshtml"
                                                                                                                                   
                                }

#line default
#line hidden

            WriteLiteral("\r\n                                <div class=\"col-sm-8\"> \r\n                                    <input autofocus=\"true\"");
            BeginWriteAttribute("placeholder", " placeholder=\"", 15005, "\"", 15079, 1);
#line 462 "Login.cshtml"
WriteAttributeValue("", 15019, Model.RegistrationEnabled ? "Email or Username" : "Email", 15019, 60, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" required name=\"login\" type=\"text\"");
            BeginWriteAttribute("value", " value=\"", 15114, "\"", 15174, 1);
#line 462 "Login.cshtml"
WriteAttributeValue("", 15122, Model.FormData.GetOrNull("login") ?? string.Empty, 15122, 52, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(@" class=""form-control"">
                                </div>
                            </div>
                            <div class=""form-group group-password"">
                                <label class=""col-sm-4"">Password</label> 
                                <div class=""col-sm-8""> 
                                    <input placeholder=""Password"" required type=""password"" name=""password"" class=""form-control"">
                                </div>
                            </div>
                            <div>
                                <button type=""submit"" class=""login btn btn-login btn-sp-green"">Log In</button>
                            </div>
                        </form>
                    </div>
                </div>
");
#line 478 "Login.cshtml"
                

#line default
#line hidden

#line 478 "Login.cshtml"
                 if (Model.VerifyEmailEnabled)
                {

#line default
#line hidden

            WriteLiteral("                    <a style=\"float:right\"");
            BeginWriteAttribute("href", " href=\"", 16224, "\"", 16252, 1);
#line 480 "Login.cshtml"
WriteAttributeValue("", 16231, Model.VerifyEmailUri, 16231, 21, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"forgot\">Forgot Password?</a>\r\n");
#line 481 "Login.cshtml"
                }

#line default
#line hidden

            WriteLiteral("\r\n");
#line 483 "Login.cshtml"
                

#line default
#line hidden

#line 483 "Login.cshtml"
                 if (Model.ForgotPasswordEnabled)
                {

#line default
#line hidden

            WriteLiteral("                    <a style=\"float:right\"");
            BeginWriteAttribute("href", " href=\"", 16424, "\"", 16455, 1);
#line 485 "Login.cshtml"
WriteAttributeValue("", 16431, Model.ForgotPasswordUri, 16431, 24, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(" class=\"forgot\">Forgot Password?</a>\r\n");
#line 486 "Login.cshtml"
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