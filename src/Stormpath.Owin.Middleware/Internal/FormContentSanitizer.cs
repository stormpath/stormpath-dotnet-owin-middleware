using System;
using System.Collections.Generic;
using System.Linq;
using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware.Internal
{
    internal sealed class FormContentSanitizer
    {
        public IDictionary<string, string[]> Sanitize(
            IDictionary<string, string[]> formData,
            string[] verbatimFields = null)
        {
            var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            if (verbatimFields == null)
            {
                verbatimFields = new string[0];
            }

            foreach (var field in verbatimFields)
            {
                string[] fieldData;
                if (formData.TryGetValue(field, out fieldData))
                {
                    result[field] = fieldData;
                }
            }

            var dataExcludingVerbatim = formData.Where(kvp => !verbatimFields.Contains(kvp.Key, StringComparer.Ordinal));

            foreach (var item in dataExcludingVerbatim)
            {
                result[item.Key] = item.Value.Select(EntityEncoder.Encode).ToArray();
            }

            return result;
        }
    }
}
