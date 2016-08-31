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
using Stormpath.SDK.Logging;
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
            = (ctx, ct) => TaskConstants.CompletedTask;

        public Func<PostChangePasswordContext, CancellationToken, Task> PostChangePasswordHandler { get; set; }
            = (ctx, ct) => TaskConstants.CompletedTask;

        public Func<PreLoginContext, CancellationToken, Task> PreLoginHandler { get; set; }
            = (ctx, ct) => TaskConstants.CompletedTask;

        public Func<PostLoginContext, CancellationToken, Task> PostLoginHandler { get; set; }
            = (ctx, ct) => TaskConstants.CompletedTask;

        public Func<PreLogoutContext, CancellationToken, Task> PreLogoutHandler { get; set; }
            = (ctx, ct) => TaskConstants.CompletedTask;

        public Func<PostLogoutContext, CancellationToken, Task> PostLogoutHandler { get; set; }
            = (ctx, ct) => TaskConstants.CompletedTask;

        public Func<PreRegistrationContext, CancellationToken, Task> PreRegistrationHandler { get; set; }
            = (ctx, ct) => TaskConstants.CompletedTask;

        public Func<PostRegistrationContext, CancellationToken, Task> PostRegistrationHandler { get; set; }
            = (ctx, ct) => TaskConstants.CompletedTask;

        public Func<PreVerifyEmailContext, CancellationToken, Task> PreVerifyEmailHandler { get; set; }
            = (ctx, ct) => TaskConstants.CompletedTask;

        public Func<PostVerifyEmailContext, CancellationToken, Task> PostVerifyEmailHandler { get; set; }
            = (ctx, ct) => TaskConstants.CompletedTask;
    }
}
