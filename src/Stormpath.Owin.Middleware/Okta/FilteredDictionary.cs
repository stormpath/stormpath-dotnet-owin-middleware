using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class FilteredDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _fullDictionary;
        private readonly TKey[] _keysToHide;
        
        public FilteredDictionary(IDictionary<TKey, TValue> fullDictionary, IEnumerable<TKey> keysToHide)
        {
            _fullDictionary = fullDictionary;
            _keysToHide = keysToHide.ToArray();
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!ContainsKey(key)) throw new KeyNotFoundException("The given key was not present in the dictionary");
                return _fullDictionary[key];
            }
            set => _fullDictionary[key] = value;
        }

        private bool OnlyFiltered(KeyValuePair<TKey, TValue> kvp)
            => !_keysToHide.Contains(kvp.Key);

        public ICollection<TKey> Keys
            => _fullDictionary
            .Where(OnlyFiltered)
            .Select(kvp => kvp.Key)
            .ToArray();

        public ICollection<TValue> Values
            => _fullDictionary
            .Where(OnlyFiltered)
            .Select(kvp => kvp.Value)
            .ToArray();

        public int Count => _fullDictionary
            .Where(OnlyFiltered)
            .Count();

        public bool IsReadOnly => _fullDictionary.IsReadOnly;

        public void Add(TKey key, TValue value)
            => _fullDictionary[key] = value;

        public void Add(KeyValuePair<TKey, TValue> item)
            => _fullDictionary[item.Key] = item.Value;

        public void Clear()
        {
            foreach (var key in Keys)
            {
                Remove(key);
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
            => _fullDictionary
            .Where(OnlyFiltered)
            .Contains(item);

        public bool ContainsKey(TKey key)
            => _fullDictionary
            .Where(OnlyFiltered)
            .Any(kvp => kvp.Key.Equals(key));

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => (_fullDictionary
            .Where(OnlyFiltered)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) as IDictionary<TKey, TValue>)
            .CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => _fullDictionary
            .Where(OnlyFiltered)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .GetEnumerator();

        public bool Remove(TKey key)
        {
            if (!ContainsKey(key)) return false;
            return _fullDictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Contains(item)) return false;
            return _fullDictionary.Remove(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
            => _fullDictionary
            .Where(OnlyFiltered)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator()
            => _fullDictionary
            .Where(OnlyFiltered)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .GetEnumerator();
    }
}
