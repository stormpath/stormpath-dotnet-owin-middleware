namespace Stormpath.Owin.Common.Views.Precompiled
{
#line 1 "Register.cshtml"
using System

#line default
#line hidden
    ;
#line 2 "Register.cshtml"
using System.Linq

#line default
#line hidden
    ;
#line 3 "Register.cshtml"
using Stormpath.Owin.Common

#line default
#line hidden
    ;
    using System.Threading.Tasks;

    public class Register : BaseView<Stormpath.Owin.Common.ViewModel.ExtendedRegisterViewModel>
    {
#line 5 "Register.cshtml"

    private new Stormpath.Owin.Common.ViewModel.ExtendedRegisterViewModel Model { get; }

#line default
#line hidden
        #line hidden
        public Register()
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
    <title>Create an Account</title>
    <meta content=""Create a new account."" name=""description"">
    <meta content=""width=device-width"" name=""viewport"">
    <link href=""//fonts.googleapis.com/css?family=Open+Sans:300italic,300,400italic,400,600italic,600,700italic,700,800italic,800"" rel=""stylesheet"" type=""text/css"">
    <link href=""//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css"" rel=""stylesheet"">
    <style>
        html,\r\nbody {\r\n  height: 100%;\r\n}\r\n\r\n@media (max-width: 767px) {\r\n  html,\r\n  body {\r\n    padding: 0 4px;\r\n  }\r\n}\r\n\r\nbody {\r\n  margin-left: auto;\r\n  margin-right: auto;\r\n}\r\n\r\nbody,\r\ndiv,\r\np,\r\na,\r\nlabel {\r\n  font-family: 'Open Sans';\r\n  font-size: 14px;\r\n  font-weight: 400;\r\n  color: #484848;\r\n}\r\n\r\na {\r\n  color: #0072dd;\r\n}\r\n\r\np {\r\n  line-height: 21px;\r\n}\r\n\r\n.container {\r\n  max-width: 620px;\r\n}\r\n\r\n.logo {\r\n  margin: 34px auto 25px auto;\r\n  display: block;\r\n}\r\n\r\n.btn-sp-green {\r\n  height: 45px;\r\n  line-height: 22.5px;\r\n  padding: 0 40px;\r\n  color: #fff;\r\n  font-size: 17px;\r\n  background: -webkit-linear-gradient(#42c41a 50%, #2dbd00 50%);\r\n  background: linear-gradient(#42c41a 50%, #2dbd00 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#2dbd00, endColorstr=#42c41a);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#2dbd00, endColorstr=#42c41a)';\r\n}\r\n\r\n.btn-sp-green:hover,\r\n.btn-sp-green:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#43cd1a 50%, #2ec700 50%);\r\n  background: linear-gradient(#43cd1a 50%, #2ec700 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#2ec700, endColorstr=#43cd1a);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#2ec700, endColorstr=#43cd1a)';\r\n}\r\n\r\n.btn-social {\r\n  height: 37px;\r\n  line-height: 18.5px;\r\n  color: #fff;\r\n  font-size: 16px;\r\n  border-radius: 3px;\r\n}\r\n\r\n.btn-social:hover,\r\n.btn-social:focus {\r\n  color: #fff;\r\n}\r\n\r\n.btn-facebook {\r\n  background: -webkit-linear-gradient(#4c6fc5 50%, #3d63c0 50%);\r\n  background: linear-gradient(#4c6fc5 50%, #3d63c0 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#3d63c0, endColorstr=#4c6fc5);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#3d63c0, endColorstr=#4c6fc5)';\r\n}\r\n\r\n.btn-facebook:hover,\r\n.btn-facebook:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#4773de 50%, #3767db 50%);\r\n  background: linear-gradient(#4773de 50%, #3767db 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#3767db, endColorstr=#4773de);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#3767db, endColorstr=#4773de)';\r\n}\r\n\r\n.btn-google {\r\n  background: -webkit-linear-gradient(#e05b4b 50%, #dd4b39 50%);\r\n  background: linear-gradient(#e05b4b 50%, #dd4b39 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#dd4b39, endColorstr=#e05b4b);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#dd4b39, endColorstr=#e05b4b)';\r\n}\r\n\r\n.btn-google:hover,\r\n.btn-google:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#ea604e 50%, #e8503c 50%);\r\n  background: linear-gradient(#ea604e 50%, #e8503c 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#e8503c, endColorstr=#ea604e);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#e8503c, endColorstr=#ea604e)';\r\n}\r\n\r\n.btn-linkedin {\r\n  background: -webkit-linear-gradient(#007cbc 50%, #0077B5 50%);\r\n  background: linear-gradient(#007cbc 50%, #0077B5 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5)';\r\n}\r\n\r\n.btn-linkedin:hover,\r\n.btn-linkedin:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#007cbc 50%, #0077B5 50%);\r\n  background: linear-gradient(#007cbc 50%, #0077B5 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#007cbc, endColorstr=#0077B5)';\r\n}\r\n\r\n.btn-github {\r\n  background: -webkit-linear-gradient(#848282 50%, #7B7979 50%);\r\n  background: linear-gradient(#848282 50%, #7B7979 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#848282, endColorstr=#7B7979);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#848282, endColorstr=#7B7979)';\r\n}\r\n\r\n.btn-github:hover,\r\n.btn-github:focus {\r\n  color: #fff;\r\n  background: -webkit-linear-gradient(#8C8888 50%, #848080 50%);\r\n  background: linear-gradient(#8C8888 50%, #848080 50%);\r\n  filter: progid:DXImageTransform.Microsoft.gradient(GradientType=0, startColorstr=#8C8888, endColorstr=#848080);\r\n  -ms-filter: 'progid:DXImageTransform.Microsoft.gradient (GradientType=0, startColorstr=#8C8888, endColorstr=#848080)';\r\n}\r\n\r\n.btn-register {\r\n  font-size: 16px;\r\n}\r\n\r\n.form-control {\r\n  font-size: 15px;\r\n  box-shadow: none;\r\n}\r\n\r\n.form-control::-webkit-input-placeholder {\r\n  color: #aaadb0;\r\n}\r\n\r\n.form-control::-moz-placeholder {\r\n  color: #aaadb0;\r\n}\r\n\r\n.form-control:-ms-input-placeholder {\r\n  color: #aaadb0;\r\n}\r\n\r\n.form-control::placeholder {\r\n  color: #aaadb0;\r\n}\r\n\r\n.form-control:focus {\r\n  box-shadow: inset 0 1px 1px rgba(0, 0, 0, 0.075), 0 0 6px rgba(0, 132, 255, 0.4);\r\n}\r\n\r\n.view .header {\r\n  padding: 34px 0;\r\n}\r\n\r\n.view .header,\r\n.view .header a {\r\n  font-weight: 300;\r\n  font-size: 21px;\r\n}\r\n\r\n.view input[type='text'],\r\n.view input[type='password'],\r\n.view input[type='email'],\r\n.view input[type='color'],\r\n.view input[type='date'],\r\n.view input[type='datetime']\r\n.view input[type='datetime-local'],\r\n.view input[type='email'],\r\n.view input[type='month'],\r\n.view input[type='number'],\r\n.view input[type='range'],\r\n.view input[type='search'],\r\n.view input[type='tel'],\r\n.view input[type='time'],\r\n.view input[type='url'],\r\n.view input[type='week']{\r\n  background-color: #f6f6f6;\r\n  height: 45px;\r\n}\r\n\r\n.view a.forgot,\r\n.view a.to-login {\r\n  float: right;\r\n  padding: 17px 0;\r\n  font-size: 13px;\r\n}\r\n\r\n.view form button {\r\n  display: block;\r\n  float: right;\r\n  margin-bottom: 25px;\r\n}\r\n\r\n.view form label {\r\n  height: 45px;\r\n  line-height: 45px;\r\n}\r\n\r\n.box {\r\n  box-shadow: 0 0px 3px 1px rgba(0, 0, 0, 0.1);\r\n  border: 1px solid #cacaca;\r\n  border-radius: 3px;\r\n  padding: 0 30px;\r\n}\r\n\r\n.sp-form .has-error,\r\n.sp-form .has-error .help-block {\r\n  color: #ec3e3e;\r\n  font-weight: 600;\r\n}\r\n\r\n.sp-form .has-error input[type='text'],\r\n.sp-form .has-error input[type='password'] {\r\n  border-color: #ec3e3e;\r\n}\r\n\r\n.sp-form .form-group {\r\n  margin-bottom: 21px;\r\n}\r\n\r\n.sp-form input[type='text'],\r\n.sp-form input[type='password'] {\r\n  position: relative;\r\n}\r\n\r\n.sp-form .help-block {\r\n  font-size: 12px;\r\n  position: absolute;\r\n  top: 43px;\r\n}\r\n\r\n.verify-view .box {\r\n  padding-bottom: 30px;\r\n}\r\n\r\n.verify-view .box .header {\r\n  padding-bottom: 20px;\r\n}\r\n\r\n.unverified-view .box {\r\n  padding-bottom: 30px;\r\n}\r\n\r\n.unverified-view .box .header {\r\n  padding-bottom: 25px;\r\n}\r\n\r\n.login-view .box {\r\n  background-color: #f6f6f6;\r\n  padding: 0;\r\n}\r\n\r\n.login-view label {\r\n  margin-bottom: 7px;\r\n}\r\n\r\n.login-view .header p {\r\n  margin-top: 2em;\r\n}\r\n\r\n.login-view .email-password-area {\r\n  background-color: white;\r\n  border-top-left-radius: 3px;\r\n  border-bottom-left-radius: 3px;\r\n}\r\n\r\n@media (min-width: 767px) {\r\n  .login-view .email-password-area {\r\n    padding: 0 30px;\r\n  }\r\n}\r\n\r\n.login-view .email-password-area label {\r\n  height: 14px;\r\n  line-height: 14px;\r\n}\r\n\r\n.login-view .email-password-area input[type='checkbox'] {\r\n  visibility: hidden;\r\n}\r\n\r\n.login-view .email-password-area input[type='checkbox'] + label {\r\n  position: relative;\r\n  padding-left: 8px;\r\n  line-height: 16px;\r\n  font-size: 13px;\r\n}\r\n\r\n.login-view .email-password-area input[type='checkbox'] + label:after {\r\n  position: absolute;\r\n  left: -16px;\r\n  width: 16px;\r\n  height: 16px;\r\n  border: 1px solid #cacaca;\r\n  background-color: #f6f6f6;\r\n  content: '';\r\n}\r\n\r\n.login-view .email-password-area input[type='checkbox']:checked + label:after {\r\n  background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAA2ZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDowRTVBQUVGMzJEODBFMjExODQ2N0NBMjk4MjdCNDBCNyIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDo0RTY4NUM4NURGNEYxMUUyQUE5QkExOTlGODU3RkFEOCIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo0RTY4NUM4NERGNEYxMUUyQUE5QkExOTlGODU3RkFEOCIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M2IChXaW5kb3dzKSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjQxNDQ4M0NEM0JERkUyMTE4MEYwQjNBRjIwMUNENzQxIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOkZDMEMxNjY2OUVCMUUyMTFBRjVDQkQ0QjE5MTNERDU2Ii8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+3YY4qgAAALlJREFUeNpi/P//PwMlgImBQjDwBrCgCwQHB+NUfObMGT9mZuboe/fuheM0ABu4fv060/fv32cBNTNycHBE4nUBNs0/f/7cAWSeMzQ0rCA5DICaNwKj+qGRkVEFUYF47ty5GWfPns2EsjsYGRlFgM5OJzoQ//37t5eLi2sRMMDec3Jypn79+lVXX1//H9HRaGJisvr379/nuLm5lwKdP9vMzOwZyekAaEA3EF8G4hZCYcQ4mhcYAAIMAJGST/dDIpNQAAAAAElFTkSuQmCC);\r\n  background-position: -1px -1px;\r\n}\r\n\r\n@media (min-width: 767px) {\r\n  .login-view .email-password-area.small {\r\n    border-right: 1px solid #cacaca;\r\n  }\r\n\r\n  .login-view .email-password-area.small .group-email {\r\n    margin-bottom: 21px;\r\n  }\r\n}\r\n\r\n@media (max-width: 767px) {\r\n  .login-view .email-password-area.small {\r\n    border-bottom: 1px solid #cacaca;\r\n    border-bottom-left-radius: 0;\r\n    border-bottom-right-radius: 0;\r\n  }\r\n}\r\n\r\n.login-view .email-password-area.large {\r\n  border-top-right-radius: 3px;\r\n  border-bottom-right-radius: 3px;\r\n}\r\n\r\n@media (min-width: 767px) {\r\n  .login-view .email-password-area.large {\r\n    padding: 0 50px;\r\n  }\r\n\r\n  .login-view .email-password-area.large .group-login label,\r\n  .login-view .email-password-area.large .group-password label {\r\n    height: 45px;\r\n    line-height: 45px;\r\n  }\r\n}\r\n\r\n.login-view .social-area {\r\n  border-top-right-radius: 3px;\r\n  border-bottom-right-radius: 3px;\r\n  padding: 0 20px;\r\n  position: relative;\r\n  padding-bottom: 20px;\r\n  background-color: #f6f6f6;\r\n}\r\n\r\n.login-view .social-area .header {\r\n  margin-bottom: -6px;\r\n}\r\n\r\n@media (max-width: 767px) {\r\n  .login-view .social-area .header {\r\n    padding: 0px;\r\n  }\r\n}\r\n\r\n.login-view .social-area button {\r\n  display: block;\r\n  width: 100%;\r\n  margin-bottom: 15px;\r\n}\r\n\r\n.login, .register { display: table; }\r\n.va-wrapper { display: table-cell; width: 100%; vertical-align: middle; }\r\n.custom-container { display: table-row; height: 100%; }
    </style>
    <!--[if lt IE 9]>
     <script src='https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js'></script>
     <script src='https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js'></scri");
            WriteLiteral(@"pt>
    <![endif]-->
</head>
    <body class=""register"">
        <div class=""container custom-container"">
            <div class=""va-wrapper"">
                <div class=""view registration-view container"">
                    <div class=""box row"">
                        <div class=""col-sm-12"">
                            <div class=""header"">
                                <span>Create Account</span>
                            </div>
");
#line 39 "Register.cshtml"
                            

#line default
#line hidden

#line 39 "Register.cshtml"
                             if (Model.Errors.Any())
                            {

#line default
#line hidden

            WriteLiteral("                                <div class=\"alert alert-danger\">\r\n");
#line 42 "Register.cshtml"
                                    

#line default
#line hidden

#line 42 "Register.cshtml"
                                     foreach (var error in Model.Errors)
                                    {

#line default
#line hidden

            WriteLiteral("                                        <p>");
#line 44 "Register.cshtml"
                                      Write(error);

#line default
#line hidden
            WriteLiteral("</p>\r\n");
#line 45 "Register.cshtml"
                                    }

#line default
#line hidden

            WriteLiteral("                                </div>\r\n");
#line 47 "Register.cshtml"
                            }

#line default
#line hidden

            WriteLiteral("                            <form method=\"post\" role=\"form\" class=\"registration-form form-horizontal sp-form\">\r\n");
#line 49 "Register.cshtml"
                                

#line default
#line hidden

#line 49 "Register.cshtml"
                                 foreach (var field in Model.Form.Fields)
                                {

#line default
#line hidden

            WriteLiteral("                                    <div form-group=\"true\"");
            BeginWriteAttribute("class", " class=\"", 2359, "\"", 2402, 2);
            WriteAttributeValue("", 2367, "form-group", 2367, 10, true);
#line 51 "Register.cshtml"
WriteAttributeValue(" ", 2377, $"group-{field.Name}", 2378, 25, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(">\r\n                                        <label class=\"col-sm-4\">");
#line 52 "Register.cshtml"
                                                           Write(field.Label);

#line default
#line hidden
            WriteLiteral("</label>\r\n                                        <div class=\"col-sm-8\">\r\n                                            <input");
            BeginWriteAttribute("placeholder", " placeholder=\"", 2606, "\"", 2638, 1);
#line 54 "Register.cshtml"
WriteAttributeValue("", 2620, field.Placeholder, 2620, 18, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("name", "\r\n                                                   name=\"", 2639, "\"", 2709, 1);
#line 55 "Register.cshtml"
WriteAttributeValue("", 2698, field.Name, 2698, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginWriteAttribute("type", "\r\n                                                   type=\"", 2710, "\"", 2780, 1);
#line 56 "Register.cshtml"
WriteAttributeValue("", 2769, field.Type, 2769, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral("\r\n                                                   class=\"form-control\"");
            BeginWriteAttribute("value", "\r\n                                                   value=\"", 2854, "\"", 2963, 1);
#line 58 "Register.cshtml"
WriteAttributeValue("", 2914, Model.FormData.Get(field.Name) ?? string.Empty, 2914, 49, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral("\r\n                                                   ");
#line 59 "Register.cshtml"
                                               Write(field.Required ? "required" : string.Empty);

#line default
#line hidden
            WriteLiteral(">\r\n                                        </div>\r\n                                    </div>\r\n");
#line 62 "Register.cshtml"

                                }

#line default
#line hidden

            WriteLiteral("                                <button type=\"submit\" class=\"btn btn-register btn-sp-green\">Create Account</button>\r\n                            </form>\r\n                        </div>\r\n                    </div>\r\n                    <a");
            BeginWriteAttribute("href", " href=\"", 3430, "\"", 3452, 1);
#line 68 "Register.cshtml"
WriteAttributeValue("", 3437, Model.LoginUri, 3437, 15, false);

#line default
#line hidden
            EndWriteAttribute();
            WriteLiteral(@" class=""to-login"">Back to Log In</a>
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
