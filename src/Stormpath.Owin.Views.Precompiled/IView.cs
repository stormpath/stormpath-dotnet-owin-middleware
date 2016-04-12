// <copyright file="IView.cs" company="Stormpath, Inc.">
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

using System.IO;
using System.Threading.Tasks;

namespace Stormpath.Owin.Views.Precompiled
{
    public interface IView
    {
        /// <summary>
        /// Render the view.
        /// </summary>
        /// <param name="model">The model to use.</param>
        /// <param name="target">The target stream to write to.</param>
        Task ExecuteAsync(object model, Stream target);
    }
}
