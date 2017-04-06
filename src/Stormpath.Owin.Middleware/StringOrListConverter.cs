using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware
{
    public sealed class StringOrListConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
            => objectType == typeof(string) || objectType == typeof(List<string>) || objectType == typeof(IList<string>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return new List<string> { reader.Value?.ToString() };
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                var list = new List<string>();
                reader.Read();
                while (reader.TokenType != JsonToken.EndArray)
                {
                    list.Add(reader.Value?.ToString());

                    reader.Read();
                }
                return list;
            }

            throw new NotImplementedException($"Unexpected token type {reader.TokenType} for StringOrListConverter");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
