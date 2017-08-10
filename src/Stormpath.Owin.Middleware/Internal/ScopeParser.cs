using System;
using System.Collections;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Internal
{
    public sealed class ScopeParser : IEnumerable<string>
    {
        private readonly string[] _scopes;

        public ScopeParser(string scope)
        {
            _scopes = (scope ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerator<string> GetEnumerator() => (_scopes as IEnumerable<string>).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
