using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace AspNetCore.CacheOutput.LiteDB
{
    /// <summary>
    /// Provider for Output Cache with LiteDB
    /// </summary>
    public class LiteDBOutputCacheProvider : IApiOutputCache
    {
        /// <summary>
        /// Database
        /// </summary>
        private readonly LiteDatabase _cache;

        /// <summary>
        /// Instantiate a LiteDBOutputCacheProvider
        /// </summary>
        /// <param name="pathToCacheDatabase">Path to LiteDB database. Default: 'cache.db'.</param>
        public LiteDBOutputCacheProvider(string pathToCacheDatabase = "cache.db")
        {
            try
            {
                _cache = new LiteDatabase(pathToCacheDatabase);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to access cache database. Please verify path parmissions.", e);
            }
        }
        
        #region Implementation

        /// <summary>
        /// Remove Items from cache by key pattern.
        /// </summary>
        /// <param name="keyPattern">Key Patern to remove items of cache</param>
        /// <returns></returns>
        public async Task RemoveStartsWithAsync(string keyPattern)
        {
            foreach(var key in CacheKeys(keyPattern))
            {
                await RemoveAsync(key);
            }
        }

        /// <summary>
        /// Get item of cache by key
        /// </summary>
        /// <typeparam name="T">Type result</typeparam>
        /// <param name="key">Key of cache item</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key) where T : class
        {
            try
            {
                CacheItem cacheItem = GetCacheItemDB(key);

                if (cacheItem.Expiration < DateTime.Now)
                {
                    await RemoveStartsWithAsync(key);
                    return null;
                }

                return await Task.FromResult(StringToObject<T>(cacheItem.Data));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Remove item of cache by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task RemoveAsync(string key)
        {
            await Task.Run(() => RemoveItemDB(key));
        }

        /// <summary>
        /// Contains key
        /// </summary>
        /// <param name="key">Key of item</param>
        /// <returns></returns>
        public async Task<bool> ContainsAsync(string key)
        {
            return await Task.FromResult(ContainsInDB(key));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <param name="dependsOnKey"></param>
        /// <returns></returns>
        public async Task AddAsync(string key, object value, DateTimeOffset expiration, string dependsOnKey = null)
        {
            if (!await ContainsAsync("key"))
            {
                SetCacheItemDB(key, value, expiration);
            }
        }

        #endregion

        #region Database methods

        private List<string> CacheKeys(string patternKey = "*")
        {
            if (patternKey == "*")
            {
                return _cache.GetCollection<CacheItem>().FindAll()
                    .Select(o => o.Key).ToList();
            }else
            {
                return _cache.GetCollection<CacheItem>()
                    .Find(c => c.Key.ToLower().Contains(patternKey.ToLower()))
                    .Select(o => o.Key).ToList();
            }
        }

        public CacheItem GetCacheItemDB(string key)
        {
            return _cache.GetCollection<CacheItem>().FindOne(o => o.Key == key);
        }

        private void SetCacheItemDB<T>(string key, T data, DateTimeOffset expiration)
        {
            var item = GetCacheItemDB(key);
            var stringData = ObjectToString(data);
            var absoluteExpiration = DateTime.Now.Add(expiration.Subtract(DateTime.Now));

            if (item == null)
            {
                _cache.GetCollection<CacheItem>().Insert(new CacheItem
                {
                    Key = key,
                    Data = stringData,
                    Expiration = absoluteExpiration
                });
            }
        }

        private bool ContainsInDB(string key)
        {
            return GetCacheItemDB(key) != null;
        }

        private void RemoveItemDB(string key)
        {
            _cache.GetCollection<CacheItem>().Delete(o => o.Key == key);
        }

        private string ObjectToString<T>(T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private T StringToObject<T>(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                return (T)new BinaryFormatter().Deserialize(ms);
            }
        }

        #endregion
    }
}
