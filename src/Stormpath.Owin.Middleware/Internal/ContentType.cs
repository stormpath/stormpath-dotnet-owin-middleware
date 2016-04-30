// <copyright file="AbstractMiddlewareController.cs" company="Stormpath, Inc.">
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

namespace Stormpath.Owin.Middleware.Internal
{
    public sealed class ContentType
    {
        public static readonly ContentType Any = new ContentType("*/*");

        public static readonly ContentType Json = new ContentType("application/json");

        public static readonly ContentType Html = new ContentType("text/html");

        public static readonly ContentType FormUrlEncoded = new ContentType("application/x-www-form-urlencoded");

        private ContentType(string contentType)
        {
            this.value = contentType;
        }

        public static ContentType Parse(string contentType)
        {
            if (contentType.Equals(Any))
            {
                return Any;
            }

            if (contentType.Equals(Json, StringComparison.Ordinal))
            {
                return Json;
            }

            if (contentType.Equals(Html, StringComparison.Ordinal))
            {
                return Html;
            }

            if (contentType.Equals(FormUrlEncoded, StringComparison.Ordinal))
            {
                return FormUrlEncoded;
            }

            return new ContentType(contentType);
        }

        private readonly string value;

        public override string ToString()
            => this.value;

        public static implicit operator string(ContentType contentType)
            => contentType.value;
    }
}
