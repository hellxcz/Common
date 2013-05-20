using System;

namespace InMemoryCache
{
    public interface ICache
    {
        T Get<T>(string cacheID, Func<T> getItemCallback) where T : class;
        T Get<T>(string cacheID, Func<T> getItemCallback, TimeSpan slidingExpiration) where T : class;
        T GetWithAbsoluteExpiration<T>(string cacheID, Func<T> getItemCallback, TimeSpan absoluteExpiration) where T : class;
        bool Contains(string cacheID);
        void Remove(string cacheID);
        void Put(string cacheID, object item);
    }
}