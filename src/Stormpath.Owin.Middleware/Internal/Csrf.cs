// <copyright file="Csrf.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class Csrf
    {
        public static readonly string OauthStateTokenCookieName = "oauthStateToken";

        public static bool ConsumeOauthStateToken(IOwinEnvironment context, ILogger logger)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, logger);

            var cookieParser = CookieParser.FromRequest(context, logger);

            var oauthQueryStateToken = queryString.GetString("state");
            var oauthCookieStateToken = cookieParser?.Get(OauthStateTokenCookieName);

            if (!string.Equals(oauthQueryStateToken, oauthCookieStateToken, StringComparison.Ordinal))
            {
                return false;
            }

            Cookies.DeleteCookie(context, OauthStateTokenCookieName, logger);
            return true;
        }
    }
}
