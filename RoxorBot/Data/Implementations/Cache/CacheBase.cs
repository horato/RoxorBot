using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces.Cache;

namespace RoxorBot.Data.Implementations.Cache
{
    public abstract class CacheBase<T, TKey> : ICacheBase<T, TKey> where T : class
    {
        private static readonly Dictionary<TKey, T> Cache = new Dictionary<TKey, T>();

        public T Get(TKey id)
        {
            if (!Exists(id))
                throw new InvalidOperationException($"Video {id} does not exists in cache");

            return Cache[id];
        }

        public Dictionary<TKey, T> Get(IEnumerable<TKey> ids)
        {
            var requested = ids?.ToList().Distinct();
            if (requested == null)
                throw new ArgumentNullException(nameof(ids));

            return requested.ToDictionary(id => id, id => Exists(id) ? Cache[id] : null);
        }

        public void Add(TKey key, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (Exists(key))
                Cache[key] = item;
            else
                Cache.Add(key, item);
        }

        public void Add(Dictionary<TKey, T> items)
        {
            var requested = items?.ToList();
            if (requested == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in requested)
                Add(item.Key, item.Value);
        }

        public bool Exists(TKey id)
        {
            return Cache.ContainsKey(id);
        }
    }
}
