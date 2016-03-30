// <copyright file="ContentNegotiation.cs" company="Stormpath, Inc.">
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

using System.Linq;
using FluentAssertions;
using Stormpath.Owin.Middleware.Internal;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class ContentNegotiationTests
    {
        private static readonly string ApplicationJson = "application/json";
        private static readonly string TextHtml = "text/html";

        private static readonly string[] DefaultProduces = new string[] { ApplicationJson, TextHtml };

        [Fact]
        public void Null_accept_header_serves_first_produces()
        {
            var result = ContentNegotiation.Negotiate(null, DefaultProduces);

            result.Success.Should().BeTrue();
            result.Preferred.ToString().Should().Be(DefaultProduces.First());
        }

        [Fact]
        public void StarStar_header_serves_first_produces()
        {
            var result = ContentNegotiation.Negotiate(null, DefaultProduces);

            result.Success.Should().BeTrue();
            result.Preferred.ToString().Should().Be(DefaultProduces.First());
        }

        [Fact]
        public void Html_preferred_and_in_produces()
        {
            var result = ContentNegotiation.Negotiate(TextHtml, DefaultProduces);

            result.Success.Should().BeTrue();
            result.Preferred.ToString().Should().Be(TextHtml);
        }

        [Fact]
        public void Html_preferred_but_not_in_produces()
        {
            var producesOnlyJson = new string[] { ApplicationJson };

            var result = ContentNegotiation.Negotiate("text/html", producesOnlyJson);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public void Html_preferred_with_implicit_quality_factor()
        {
            var headerValue = "application/json; q=0.8, text/html";

            var result = ContentNegotiation.Negotiate(headerValue, DefaultProduces);

            result.Success.Should().BeTrue();
            result.Preferred.ToString().Should().Be(TextHtml);
        }

        [Fact]
        public void Html_preferred_with_higher_quality_factor()
        {
            var headerValue = "application/json; q=0.8, text/html;q=0.9";

            var result = ContentNegotiation.Negotiate(headerValue, DefaultProduces);

            result.Success.Should().BeTrue();
            result.Preferred.ToString().Should().Be(TextHtml);
        }

        [Fact]
        public void Json_preferred_and_in_produces()
        {
            var result = ContentNegotiation.Negotiate(ApplicationJson, DefaultProduces);

            result.Success.Should().BeTrue();
            result.Preferred.ToString().Should().Be(ApplicationJson);
        }

        [Fact]
        public void Json_preferred_but_not_in_produces()
        {
            var producesOnlyHtml = new string[] { TextHtml };

            var result = ContentNegotiation.Negotiate(ApplicationJson, producesOnlyHtml);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public void Json_preferred_with_implicit_quality_factor()
        {
            var headerValue = "text/html; q=0.8, application/json";

            var result = ContentNegotiation.Negotiate(headerValue, DefaultProduces);

            result.Success.Should().BeTrue();
            result.Preferred.ToString().Should().Be(ApplicationJson);
        }

        [Fact]
        public void Json_preferred_with_higher_quality_factor()
        {
            var headerValue = "text/html; q=0.8, application/json;q=0.9";

            var result = ContentNegotiation.Negotiate(headerValue, DefaultProduces);

            result.Success.Should().BeTrue();
            result.Preferred.ToString().Should().Be(ApplicationJson);
        }

        [Fact]
        public void Unsupported_header_fails()
        {
            var result = ContentNegotiation.Negotiate("foo/bar", DefaultProduces);

            result.Success.Should().BeFalse();
        }
    }
}
