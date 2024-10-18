using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AspectSharp.DynamicProxy.Utils
{
    internal sealed class LazyDictionary : IDictionary<string, object>
    {
        public object this[string key] { get => GetInstance()[key]; set => GetInstance()[key] = value; }

        public ICollection<string> Keys => GetInstance().Keys;

        public ICollection<object> Values => GetInstance().Values;

        public int Count => GetInstance().Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
            => GetInstance().Add(key, value);

        public void Add(KeyValuePair<string, object> item)
            => GetInstance().Add(item);

        public void Clear()
            => GetInstance().Clear();

        public bool Contains(KeyValuePair<string, object> item)
            => GetInstance().Contains(item);

        public bool ContainsKey(string key)
            => GetInstance().ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            => GetInstance().CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            => GetInstance().GetEnumerator();

        public bool Remove(string key)
            => GetInstance().Remove(key);

        public bool Remove(KeyValuePair<string, object> item)
            => GetInstance().Remove(item);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
            => GetInstance().TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private IDictionary<string, object>? _instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDictionary<string, object> GetInstance()
        {
            if (_instance is null)
                _instance = new Dictionary<string, object>();
            return _instance!;
        }
    }
}