﻿// <copyright file="OauthInvalidRequest.cs" company="Stormpath, Inc.">
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

namespace Stormpath.Owin.Middleware.Model.Error
{
    public sealed class OauthInvalidRequest : AbstractError
    {
        public override int StatusCode => 400;

        public OauthInvalidRequest()
        {
            this.Body = new
            {
                error = "invalid_request"
            };
        }

        public OauthInvalidRequest(string errorDescription)
        {
            Body = new
            {
                error = "invalid_request",
                error_description = errorDescription
            };
        }
    }
}
