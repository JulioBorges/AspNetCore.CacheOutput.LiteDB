using LiteDB;
using System;

namespace AspNetCore.CacheOutput.LiteDB
{
    /// <summary>
    /// Data Model to Cache Items
    /// </summary>
    public class CacheItem
    {
        public CacheItem()
        {
            Id = ObjectId.NewObjectId();
        }

        /// <summary>
        /// Id for LiteDB
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// Key of Cache Item
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Data Cached
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Expiration
        /// </summary>
        public DateTime Expiration { get; set; }
    }
}
