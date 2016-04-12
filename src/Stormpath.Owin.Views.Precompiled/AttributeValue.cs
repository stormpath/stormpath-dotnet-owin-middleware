// <copyright file="AttributeValue.cs" company="Stormpath, Inc.">
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

namespace Stormpath.Owin.Views.Precompiled
{
    public class AttributeValue
    {
        public AttributeValue(string prefix, object value, bool literal)
        {
            Prefix = prefix;
            Value = value;
            Literal = literal;
        }

        public string Prefix { get; }

        public object Value { get; }

        public bool Literal { get; }

        public static AttributeValue FromTuple(Tuple<string, object, bool> value)
        {
            return new AttributeValue(value.Item1, value.Item2, value.Item3);
        }

        public static AttributeValue FromTuple(Tuple<string, string, bool> value)
        {
            return new AttributeValue(value.Item1, value.Item2, value.Item3);
        }

        public static implicit operator AttributeValue(Tuple<string, object, bool> value)
        {
            return FromTuple(value);
        }
    }
}
