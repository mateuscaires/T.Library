using System;
using MC = System.Runtime.Caching;
using System.Linq;
using System.Collections.Generic;

namespace T.Common
{
    public class MemoryCache : ICacheService
    {
        private static ICacheService _instance;

        public static ICacheService Instance
        {
            get
            {
                if (_instance.IsNull())
                    _instance = new MemoryCache();
                return _instance;
            }
        }

        public int CacheDuration { get; set; }

        private MemoryCache()
        {
            CacheDuration = 60;
        }

        public TValue Get<TValue>(string key, Func<TValue> callback) where TValue : class
        {
            return Get(key, callback, CacheDuration);
        }

        public TValue Get<TValue>(string key, Func<TValue> callback, int cacheDuration) where TValue : class
        {
            TValue item = MC.MemoryCache.Default.Get(key) as TValue;

            if (item == null)
            {
                item = callback();
                if (item != null)
                    MC.MemoryCache.Default.Add(key, item, DateTime.Now.AddMinutes(cacheDuration));
            }

            return item;
        }

        public TValue GetValue<TValue>(string key, Func<TValue> callback)
        {
            return GetValue(key, callback, CacheDuration);
        }

        public TValue GetValue<TValue>(string key, Func<TValue> callback, int cacheDuration)
        {
            TValue item = default(TValue);

            try
            {
                item = (TValue)MC.MemoryCache.Default.Get(key);
            }
            catch
            {

            }

            if (item.IsNull() || item.Equals(default(TValue)))
            {
                item = callback();
                if (item != null)
                    MC.MemoryCache.Default.Add(key, item, DateTime.Now.AddMinutes(cacheDuration));
            }

            return item;
        }
        public void ClearCache()
        {
            List<string> keys = MC.MemoryCache.Default.Select(a => a.Key).ToList();

            Remove(keys);
        }

        public void ClearCache(string part)
        {
            List<string> keys = MC.MemoryCache.Default.Select(a => a.Key).ToList();

            keys = keys.Where(a => a.Contains(part)).ToList();

            Remove(keys);
        }

        public void RemoveCache(string key)
        {
            try
            {
                MC.MemoryCache.Default.Remove(key);
            }
            catch
            {

            }
        }

        private void Remove(List<string> keys)
        {
            try
            {
                keys.ForEach(key => MC.MemoryCache.Default.Remove(key));
            }
            catch
            {

            }
        }
    }

    public interface ICacheService
    {
        int CacheDuration { get; set; }
        TValue Get<TValue>(string key, Func<TValue> callback) where TValue : class;
        TValue Get<TValue>(string key, Func<TValue> callback, int cacheDuration) where TValue : class;
        TValue GetValue<TValue>(string key, Func<TValue> callback);
        TValue GetValue<TValue>(string key, Func<TValue> callback, int cacheDuration);
        void ClearCache();
        void ClearCache(string key);
        void RemoveCache(string key);
    }
}
