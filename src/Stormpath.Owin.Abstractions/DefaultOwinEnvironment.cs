// <copyright file="DefaultOwinEnvironment.cs" company="Stormpath, Inc.">
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
using System.Threading;

namespace Stormpath.Owin.Abstractions
{
    public sealed class DefaultOwinEnvironment : IOwinEnvironment
    {
        private readonly IDictionary<string, object> _environment;

        public DefaultOwinEnvironment(IDictionary<string, object> owinEnvironment)
        {
            _environment = owinEnvironment;
            Request = new DefaultOwinRequest(owinEnvironment);
            Response = new DefaultOwinResponse(owinEnvironment);
        }

        public IOwinRequest Request { get; private set; }

        public IOwinResponse Response { get; private set; }

        public CancellationToken CancellationToken
        {
            get { return _environment.Get<CancellationToken>(OwinKeys.CallCancelled); }
            set { _environment.SetOrRemove(OwinKeys.CallCancelled, value); }
        }
    }
}
