﻿// <copyright file="LoginViewModel.cs" company="Stormpath, Inc.">
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

using System.Collections.Generic;

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class LoginFormViewModel : LoginViewModel
    {
        public static readonly string[] AcceptableStatuses = {
            "unverified",
            "verified",
            "created",
            "forgot",
            "reset",
            "social_failed"
        };

        public LoginFormViewModel()
        {
        }

        public LoginFormViewModel(LoginViewModel existing)
        {
            // Copy and extend
            this.Form = existing.Form;
            this.AccountStores = existing.AccountStores;
        }

        public string Status { get; set; }

        public bool RegistrationEnabled { get; set; }

        public string RegisterUri { get; set; }

        public bool VerifyEmailEnabled { get; set; }

        public string VerifyEmailUri { get; set; }

        public bool ForgotPasswordEnabled { get; set; }

        public string ForgotPasswordUri { get; set; }

        public IDictionary<string, string> FormData { get; set; } = new Dictionary<string, string>();

        public IList<string> Errors { get; set; } = new List<string>();

        public string StateToken { get; set; }
    }
}
