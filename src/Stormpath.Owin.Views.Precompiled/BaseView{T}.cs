// <copyright file="BaseView.cs" company="Stormpath, Inc.">
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Stormpath.Owin.Views.Precompiled
{
    public abstract class BaseView<T> : IView
    {
        protected T Model { get; private set; }

        /// <summary>
        /// The output stream
        /// </summary>
        protected StreamWriter Output { get; private set; }

        /// <summary>
        /// Render the view.
        /// </summary>
        /// <param name="model">The model to use.</param>
        /// <param name="target">The target stream to write to.</param>
        public async Task ExecuteAsync(T model, Stream target)
        {
            Model = model;
            Output = new StreamWriter(target, Encoding.UTF8, 4096, leaveOpen: true);
            await ExecuteAsync();
            Output.Dispose();
        }

        public Task ExecuteAsync(object model, Stream target)
            => ExecuteAsync((T)model, target);

        /// <summary>
        /// Execute an individual request
        /// </summary>
        public abstract Task ExecuteAsync();

        /// <summary>
        /// Write the given value directly to the output
        /// </summary>
        /// <param name="value"></param>
        protected void WriteLiteral(string value)
        {
            WriteLiteralTo(Output, value);
        }

        /// <summary>
        /// Write the given value directly to the output
        /// </summary>
        /// <param name="value"></param>
        protected void WriteLiteral(object value)
        {
            WriteLiteralTo(Output, value);
        }

        private List<string> AttributeValues { get; set; }

        protected void WriteAttributeValue(string thingy, int startPostion, object value, int endValue, int dealyo, bool yesno)
        {
            if (AttributeValues == null)
            {
                AttributeValues = new List<string>();
            }

            AttributeValues.Add(value.ToString());
        }

        private string AttributeEnding { get; set; }

        protected void BeginWriteAttribute(string name, string begining, int startPosition, string ending, int endPosition, int thingy)
        {
            if (!string.IsNullOrEmpty(AttributeEnding))
            {
                throw new Exception("AttributeEnding must be empty.");
            }

            Output.Write(begining);
            AttributeEnding = ending;
        }

        protected void EndWriteAttribute()
        {
            if (string.IsNullOrEmpty(AttributeEnding))
            {
                throw new Exception("AttributeEnding must not be null.");
            }

            var attributes = string.Join(" ", AttributeValues);
            Output.Write(attributes);
            AttributeValues = null;

            Output.Write(AttributeEnding);
            AttributeEnding = null;
        }

        /// <summary>
        /// Writes the given attribute to the given writer
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="name">The name of the attribute to write</param>
        /// <param name="leader">The value of the prefix</param>
        /// <param name="trailer">The value of the suffix</param>
        /// <param name="values">The <see cref="AttributeValue"/>s to write.</param>
        protected void WriteAttributeTo(
            TextWriter writer,
            string name,
            string leader,
            string trailer,
            params AttributeValue[] values)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (leader == null)
            {
                throw new ArgumentNullException(nameof(leader));
            }

            if (trailer == null)
            {
                throw new ArgumentNullException(nameof(trailer));
            }


            WriteLiteralTo(writer, leader);
            foreach (var value in values)
            {
                WriteLiteralTo(writer, value.Prefix);

                // The special cases here are that the value we're writing might already be a string, or that the
                // value might be a bool. If the value is the bool 'true' we want to write the attribute name
                // instead of the string 'true'. If the value is the bool 'false' we don't want to write anything.
                // Otherwise the value is another object (perhaps an HtmlString) and we'll ask it to format itself.
                string stringValue;
                if (value.Value is bool)
                {
                    if ((bool)value.Value)
                    {
                        stringValue = name;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    stringValue = value.Value as string;
                }

                // Call the WriteTo(string) overload when possible
                if (value.Literal && stringValue != null)
                {
                    WriteLiteralTo(writer, stringValue);
                }
                else if (value.Literal)
                {
                    WriteLiteralTo(writer, value.Value);
                }
                else if (stringValue != null)
                {
                    WriteTo(writer, stringValue);
                }
                else
                {
                    WriteTo(writer, value.Value);
                }
            }
            WriteLiteralTo(writer, trailer);
        }

        /// <summary>
        /// Convert to string and html encode
        /// </summary>
        /// <param name="value"></param>
        protected void Write(object value)
        {
            WriteTo(Output, value);
        }

        /// <summary>
        /// Html encode and write
        /// </summary>
        /// <param name="value"></param>
        protected void Write(string value)
        {
            WriteTo(Output, value);
        }

        /// <summary>
        /// <see cref="DeferredWrite.WriteTo(TextWriter)"/> is invoked
        /// </summary>
        /// <param name="result">The <see cref="DeferredWrite"/> to invoke</param>
        protected void Write(DeferredWrite result)
        {
            WriteTo(Output, result);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        /// <remarks>
        /// <see cref="DeferredWrite.WriteTo(TextWriter)"/> is invoked for <see cref="DeferredWrite"/> types.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        protected void WriteTo(TextWriter writer, object value)
        {
            if (value != null)
            {
                var helperResult = value as DeferredWrite;
                if (helperResult != null)
                {
                    helperResult.WriteTo(writer);
                }
                else
                {
                    WriteTo(writer, Convert.ToString(value, CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        protected void WriteTo(TextWriter writer, string value)
        {
            WriteLiteralTo(writer, WebUtility.HtmlEncode(value));
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        protected void WriteLiteralTo(TextWriter writer, object value)
        {
            WriteLiteralTo(writer, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        protected void WriteLiteralTo(TextWriter writer, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(value);
            }
        }

        protected string HtmlEncodeAndReplaceLineBreaks(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Split on line breaks before passing it through the encoder.
            return string.Join("<br />" + Environment.NewLine,
                input.Split(new[] { "\r\n" }, StringSplitOptions.None)
                .SelectMany(s => s.Split(new[] { '\r', '\n' }, StringSplitOptions.None))
                .Select(WebUtility.HtmlEncode));
        }
    }
}
