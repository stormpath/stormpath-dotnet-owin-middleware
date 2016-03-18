// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Stormpath.Owin.Common.View
{
    /// <summary>
    /// Represents a deferred write operation in a <see cref="BaseView"/>.
    /// </summary>
    public class HelperResult
    {
        /// <summary>
        /// Creates a new instance of <see cref="HelperResult"/>.
        /// </summary>
        /// <param name="func">The delegate to invoke when <see cref="WriteTo(TextWriter)"/> is called.</param>
        public HelperResult(Func<TextWriter, Task> func)
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