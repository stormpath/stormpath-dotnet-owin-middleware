using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Stormpath.Owin.Middleware
{
    public static class DynamicObjectExtensions
    {
        public static dynamic ToDynamic(this object obj)
        {
            if (obj is IDictionary<string, object> asDictionary)
            {
                return DictionaryToDynamic(asDictionary);
            }

            return PocoToDynamic(obj);
        }

        private static dynamic DictionaryToDynamic(IDictionary<string, object> dict)
        {
            IDictionary<string, object> expandoDictionary = new ExpandoObject();

            if (dict == null)
            {
                return expandoDictionary;
            }

            foreach (var item in dict)
            {
                expandoDictionary.Add(item);
            }
            return expandoDictionary as ExpandoObject;
        }

        private static dynamic PocoToDynamic(object obj)
        {
            IDictionary<string, object> expandoDictionary = new ExpandoObject();

            foreach (var property in obj.GetType().GetTypeInfo().DeclaredProperties)
            {
                var rawValue = property.GetValue(obj);
                if (rawValue == null)
                {
                    expandoDictionary.Add(property.Name, null);
                    continue;
                }

                var type = rawValue.GetType();
                if (type != typeof(string) && type.GetTypeInfo().IsClass)
                {
                    expandoDictionary.Add(property.Name, rawValue.ToDynamic());
                }
                else
                {
                    expandoDictionary.Add(property.Name, rawValue);
                }
            }

            return expandoDictionary as ExpandoObject;
        }
    }
}
