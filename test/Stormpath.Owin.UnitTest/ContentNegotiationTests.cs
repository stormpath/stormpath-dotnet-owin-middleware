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
        private static readonly string FormUrlEncoded = "application/x-www-form-urlencoded";

        private static readonly string[] DefaultProduces = new string[] { ApplicationJson, TextHtml };

        [Fact]
        public void Null_accept_header_serves_first_produces()
        {
            var result = ContentNegotiation.NegotiateAcceptHeader(null, DefaultProduces, logger: null);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(DefaultProduces.First());
        }

        [Fact]
        public void StarStar_header_serves_first_produces()
        {
            var result = ContentNegotiation.NegotiateAcceptHeader(null, DefaultProduces, logger: null);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(DefaultProduces.First());
        }

        [Fact]
        public void Html_preferred_and_in_produces()
        {
            var result = ContentNegotiation.NegotiateAcceptHeader(TextHtml, DefaultProduces, logger: null);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(TextHtml);
        }

        [Fact]
        public void Html_preferred_but_not_in_produces()
        {
            var producesOnlyJson = new string[] { ApplicationJson };

            var result = ContentNegotiation.NegotiateAcceptHeader("text/html", producesOnlyJson, logger: null);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public void Html_preferred_with_implicit_quality_factor()
        {
            var headerValue = "application/json; q=0.8, text/html";

            var result = ContentNegotiation.NegotiateAcceptHeader(headerValue, DefaultProduces, logger: null);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(TextHtml);
        }

        [Fact]
        public void Html_preferred_with_higher_quality_factor()
        {
            var headerValue = "application/json; q=0.8, text/html;q=0.9";

            var result = ContentNegotiation.NegotiateAcceptHeader(headerValue, DefaultProduces, logger: null);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(TextHtml);
        }

        [Fact]
        public void Json_preferred_and_in_produces()
        {
            var result = ContentNegotiation.NegotiateAcceptHeader(ApplicationJson, DefaultProduces, logger: null);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(ApplicationJson);
        }

        [Fact]
        public void Json_preferred_but_not_in_produces()
        {
            var producesOnlyHtml = new string[] { TextHtml };

            var result = ContentNegotiation.NegotiateAcceptHeader(ApplicationJson, producesOnlyHtml, logger: null);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public void Json_preferred_with_implicit_quality_factor()
        {
            var headerValue = "text/html; q=0.8, application/json";

            var result = ContentNegotiation.NegotiateAcceptHeader(headerValue, DefaultProduces, logger: null);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(ApplicationJson);
        }

        [Fact]
        public void Json_preferred_with_higher_quality_factor()
        {
            var headerValue = "text/html; q=0.8, application/json;q=0.9";

            var result = ContentNegotiation.NegotiateAcceptHeader(headerValue, DefaultProduces, logger: null);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(ApplicationJson);
        }

        [Fact]
        public void Unsupported_header_fails()
        {
            var result = ContentNegotiation.NegotiateAcceptHeader("foo/bar", DefaultProduces, logger: null);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public void Form_urlencoded_is_valid_post_body_type()
        {
            var result = ContentNegotiation.DetectBodyType(FormUrlEncoded);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(FormUrlEncoded);
        }

        [Fact]
        public void Json_is_valid_post_body_type()
        {
            var result = ContentNegotiation.DetectBodyType(ApplicationJson);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(ApplicationJson);
        }

        [Fact]
        public void TextPlain_is_valid_post_body_type()
        {
            var result = ContentNegotiation.DetectBodyType("text/plain");

            result.Success.Should().BeFalse();
        }

        [Fact]
        public void TextHtml_is_valid_post_body_type()
        {
            var result = ContentNegotiation.DetectBodyType(TextHtml);

            result.Success.Should().BeFalse();
        }

        /// <summary>
        /// Regression test for https://github.com/stormpath/stormpath-dotnet-owin-middleware/issues/57
        /// </summary>
        [Fact]
        public void Complex_scenario_1_is_html()
        {
            var headerValue = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            var result = ContentNegotiation.NegotiateAcceptHeader(headerValue, DefaultProduces, logger: null);

            result.Success.Should().BeTrue();
            result.ContentType.ToString().Should().Be(TextHtml);
        }
    }
}
