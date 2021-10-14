using Bot.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace Bot.Tests.Data
{
    public class MemoryCacheStub : IMemoryCache
    {
        private Dictionary<object, object> _cache = new Dictionary<object, object>();

        public void Set(string key, TokenData value)
        {
            var cacheEntry = CreateEntry(key);
            cacheEntry.Value = value;
        }

        public ICacheEntry CreateEntry(object key)
        {
            var cacheEntry = new CacheEntryStub { Key = key };
            _cache.Add(key, cacheEntry);
            return cacheEntry;
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {
            _cache.Remove(key);
        }

        public bool TryGetValue(object key, out object value)
        {
            if (_cache.TryGetValue(key, out var cacheEntry))
            {
                value = ((CacheEntryStub)cacheEntry).Value;
                return true;
            }

            value = null;
            return false;
        }
    }

    public class CacheEntryStub : ICacheEntry
    {
        public object Key { get; set; }
        public object Value { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public IList<IChangeToken> ExpirationTokens { get; set; }
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; }
        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public void Dispose()
        {
        }
    }
}
