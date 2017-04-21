// <copyright file="StormpathMiddlewareOptions.cs" company="Stormpath, Inc.">
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class StormpathOwinOptions
    {
        public object Configuration { get; set; }

        public string ConfigurationFileRoot { get; set; }

        public ILogger Logger { get; set; }

        public string LibraryUserAgent { get; set; }

        public IViewRenderer ViewRenderer { get; set; }

        public Func<PreChangePasswordContext, CancellationToken, Task> PreChangePasswordHandler { get; set; }

        public Func<PostChangePasswordContext, CancellationToken, Task> PostChangePasswordHandler { get; set; }

        public Func<PreLoginContext, CancellationToken, Task> PreLoginHandler { get; set; }

        public Func<PostLoginContext, CancellationToken, Task> PostLoginHandler { get; set; }

        public Func<PreLogoutContext, CancellationToken, Task> PreLogoutHandler { get; set; }

        public Func<PostLogoutContext, CancellationToken, Task> PostLogoutHandler { get; set; }

        public Func<PreRegistrationContext, CancellationToken, Task> PreRegistrationHandler { get; set; }

        public Func<PostRegistrationContext, CancellationToken, Task> PostRegistrationHandler { get; set; }

        public Func<PreVerifyEmailContext, CancellationToken, Task> PreVerifyEmailHandler { get; set; }

        public Func<PostVerifyEmailContext, CancellationToken, Task> PostVerifyEmailHandler { get; set; }

        public Func<SendVerificationEmailContext, CancellationToken, Task> SendVerificationEmailHandler { get; set; }
    }
}
