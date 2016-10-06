// <copyright file="DefaultOwinRequest.cs" company="Stormpath, Inc.">
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

namespace Stormpath.Owin.Abstractions
{
    public sealed class DefaultOwinRequest : IOwinRequest
    {
        private readonly IDictionary<string, object> _environment;

        public DefaultOwinRequest(IDictionary<string, object> owinEnvironment)
        {
            _environment = owinEnvironment;
        }

        public string Scheme
            => _environment.Get<string>(OwinKeys.RequestScheme);

        public Stream Body
            => _environment.Get<Stream>(OwinKeys.RequestBody);

        public IDictionary<string, string[]> Headers
            => _environment.Get<IDictionary<string, string[]>>(OwinKeys.RequestHeaders);

        public string Method
            => _environment.Get<string>(OwinKeys.RequestMethod);

        public string Path
            => _environment.Get<string>(OwinKeys.RequestPath);

        public string PathBase
            => _environment.Get<string>(OwinKeys.RequestPathBase);
        
        public string QueryString
            => _environment.Get<string>(OwinKeys.RequestQueryString);

        /// <summary>
        /// Reconstructs the original request URI, minus the scheme and host.
        /// </summary>
        public string OriginalUri
        {
            get
            {
                var uri = $"{PathBase}{Path}";
                if (!string.IsNullOrEmpty(QueryString))
                {
                    uri += "?" + QueryString;
                }

                return uri;
            }
        }

        public object this[string key]
        {
            get { return _environment.Get(key); }
            set { _environment[key] = value; }
        }

        public async Task<string> GetBodyAsStringAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bodyAsString = string.Empty;
            using (var reader = new StreamReader(Body))
            {
                bodyAsString = await reader.ReadToEndAsync();
            }

            return bodyAsString;
        }
    }
}
