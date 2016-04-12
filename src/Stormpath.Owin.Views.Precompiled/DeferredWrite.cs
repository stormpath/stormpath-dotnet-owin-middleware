// <copyright file="DeferredWrite.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
// Contains code Copyright (c) .NET Foundation. All rights reserved.
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
using System.IO;
using System.Threading.Tasks;

namespace Stormpath.Owin.Views.Precompiled
{
    /// <summary>
    /// Represents a deferred write operation in a <see cref="BaseView"/>.
    /// </summary>
    public class DeferredWrite
    {
        /// <summary>
        /// Creates a new instance of <see cref="HelperResult"/>.
        /// </summary>
        /// <param name="func">The delegate to invoke when <see cref="WriteTo(TextWriter)"/> is called.</param>
        public DeferredWrite(Func<TextWriter, Task> func)
        {
            WriteFunc = func;
        }

        public Func<TextWriter, Task> WriteFunc { get; }

        /// <summary>
        /// Method invoked to produce content from the <see cref="HelperResult"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        public Task WriteTo(TextWriter writer)
        {
            return WriteFunc(writer);
        }
    }
}
