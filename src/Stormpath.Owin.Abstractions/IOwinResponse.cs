﻿// <copyright file="IOwinResponse.cs" company="Stormpath, Inc.">
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Abstractions
{
    public interface IOwinResponse
    {
        Stream Body { get; set; }
        IDictionary<string, string[]> Headers { get; }
        string ReasonPhrase { set; }
        int StatusCode { set; }
        Action<Action<object>, object> OnSendingHeaders { get; }

        object this[string key] { get; set; }

        Task WriteAsync(string text, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken));
    }
}