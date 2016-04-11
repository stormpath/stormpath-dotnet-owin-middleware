// <copyright file="IOwinRequest.cs" company="Stormpath, Inc.">
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Common
{
    public interface IOwinRequest
    {
        string Scheme { get; }
        Stream Body { get; }
        string Method { get; }
        string Path { get; }
        string PathBase { get; }
        string QueryString { get; }
        string OriginalUri { get; }
        IDictionary<string, string[]> Headers { get; }
        object this[string key] { get; set; }

        Task<string> GetBodyAsStringAsync(CancellationToken cancellationToken);
    }
}