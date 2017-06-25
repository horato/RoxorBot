using System.Collections.Generic;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Interfaces.Cache
{
    public interface ICacheBase<T, TKey>
    {
        T Get(TKey id);
        Dictionary<TKey, T> Get(IEnumerable<TKey> ids);
        void Add(TKey key, T item);
        void Add(Dictionary<TKey, T> items);
        bool Exists(TKey id);
    }
}
