namespace Stormpath.Owin.Abstractions
{
    public static class EntityEncoder
    {
        public static string Encode(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;")
                .Replace("/", "&#x2F;");
        }
    }
}