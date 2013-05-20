using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace InMemoryCache
{
    public class InMemoryCache : ICache
    {
        public InMemoryCache()
        {
            _lockers = new ConcurrentDictionary<string, object>();
        }

        private ConcurrentDictionary<string, object> _lockers;

        public T Get<T>(string cacheID, Func<T> getItemCallback) where T : class
        {
            return Get(cacheID, getItemCallback, TimeSpan.FromMinutes(60));
        }

        public T Get<T>(string cacheID, Func<T> getItemCallback, TimeSpan slidingExpiration) where T : class
        {
            return Get(cacheID, getItemCallback, slidingExpiration, InsertToCacheWithSliding);
        }

        public T GetWithAbsoluteExpiration<T>(string cacheID, Func<T> getItemCallback, TimeSpan absoluteExpiration)
            where T : class
        {
            return Get(cacheID, getItemCallback, absoluteExpiration, InsertToCacheWithAbsolute);
        }

        protected T Get<T>(string cacheID, Func<T> getItemCallback, TimeSpan expiration, Action<string, TimeSpan, T> insertToCacheAction) where T : class
        {
            cacheID = cacheID.ToLower();

            Func<T> getFast = () => ((byte[])HttpRuntime.Cache.Get(cacheID))
                                        .Unzip()
                                        .GetObject<T>();

            Func<T> getSlow = () =>
            {
                object locker = null;

                lock (_lockers)
                {
                    if (_lockers.ContainsKey(cacheID))
                    {
                        _lockers.TryGetValue(cacheID, out locker);
                    }

                    if (locker == null)
                    {
                        locker = new Object();
                        _lockers.TryAdd(cacheID, locker);
                    }
                }

                T item;

                lock (locker)
                {
                    item = getFast();

                    if (item != null)
                    {
                        return item;
                    }

                    item = getItemCallback();

                    if (item != null)
                    {
                        insertToCacheAction(cacheID, expiration, item);
                    }
                }

                return item;
            };

            return FastSlowGet.Get(getFast, getSlow);

        }

        private void InsertToCacheWithAbsolute<T>(string cacheID, TimeSpan absoluteExpiration, T item) where T : class
        {
            HttpRuntime
                .Cache
                .Insert(cacheID,
                //item.GetDeepCopy(),
                        item.GetJSON().Zip(),
                        null,
                        DateTime.Now.Add(absoluteExpiration),
                        TimeSpan.Zero,
                        CacheItemPriority.Normal,
                        (key, value, reason) =>
                        {
                            lock (_lockers)
                            {
                                object _value = null;

                                _lockers.TryRemove(key, out _value);
                            }
                        }
                );
        }

        private void InsertToCacheWithSliding<T>(string cacheID, TimeSpan slidingExpiration, T item) where T : class
        {
            HttpRuntime
                .Cache
                .Insert(cacheID,
                //item.GetDeepCopy(),
                        item.GetJSON().Zip(),
                        null,
                        Cache.NoAbsoluteExpiration,
                        slidingExpiration,
                        CacheItemPriority.Normal,
                        (key, value, reason) =>
                        {
                            lock (_lockers)
                            {
                                object _value = null;

                                _lockers.TryRemove(key, out _value);
                            }
                        }
                );
        }

        public bool Contains(string cacheID)
        {
            var item = HttpRuntime.Cache.Get(cacheID.ToLower());
            return item != null;
        }

        public void Remove(string cacheID)
        {
            HttpRuntime.Cache.Remove(cacheID.ToLower());
        }

        public void Put(string cacheID, object item)
        {
            lock (HttpRuntime.Cache)
            {
                HttpRuntime.Cache
                    .Insert(cacheID.ToLower(),
                    //item.GetDeepCopy()
                        item.GetJSON().Zip()
                    );
            }
        }
    }
}
