using Shared.Extensions;
using Shared.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{
	public class ObjectCacheProvider : BaseCacheProvider, ICacheProvider
	{
		private readonly ConcurrentDictionary<string, object> Cache;
		public bool EnableCache { get; set; }
		public int CacheInSeconds { get; set; }
		public bool ForceUpdate { get; set; }
		public Guid ReferenceId { get; set; }

		public ObjectCacheProvider()
		{
			Cache = new ConcurrentDictionary<string, object>();
			EnableCache = (ConfigurationManager.AppSettings["EnableCache"] ?? "false").To<bool>();
			CacheInSeconds = (ConfigurationManager.AppSettings["CacheDurationInSeconds"] ?? "120").To<int>();
		}

		public ObjectCacheProvider(bool enableCache, int cacheInSeconds)
		{
			Cache = new ConcurrentDictionary<string, object>();
			EnableCache = enableCache;
			CacheInSeconds = cacheInSeconds;
		}

		public class CacheItem<T> where T : class
		{
			public T Item { get; set; }
			public DateTime Expiration { get; set; }
			public string Key { get; set; }
			public string Region { get; set; }
			public Guid ReferenceId { get; set; }

			public bool HasExpired()
			{
				return Expiration < DateTime.UtcNow;
			}
		}

		public T Execute<T>(Request<T> request) where T : class, new()
		{
			return default(T);
		}

		public T GetModelFromCache<T>(object key, T model, Func<T, T> populate, bool useTypeFullNameInKey = true) where T : class, new()
		{
			if (!EnableCache)
				return populate(model);

			if (key == null)
				return null;

			var typeName = typeof(T).Name;

			if (model is IList && model.GetType().IsGenericType)
				typeName = model.GetType().GetGenericArguments()[0].Name;

			var keyValue = useTypeFullNameInKey ? $"{typeName}{key}" : key.ToString() ?? "";

			if (keyValue.IsNullOrEmpty())
				return null;

			if (Cache.ContainsKey(keyValue) && !ForceUpdate)
			{
				var cacheItem = Cache[keyValue] as CacheItem<T>;

				if (cacheItem != null && !cacheItem.HasExpired())
					return cacheItem.Item;
			}

			model = populate(model);

			var expirationDate = DateTime.UtcNow.AddSeconds(new TimeSpan(0, 0, 0, CacheInSeconds).TotalSeconds);
			var obj = new CacheItem<T>
			{
				Item = model,
				Expiration = expirationDate,
				Key = keyValue,
				Region = typeName
			};

			Cache.TryAdd(keyValue, obj);

			if (ForceUpdate && keyValue.Equals($"{typeName}Save") && (model as BaseModel) != null)
				RemoveModelFromCache<T>($"{typeName}GetById{(model as BaseModel).Id}");

			if (ForceUpdate)
				ForceUpdate = false;

			return obj.Item;
		}

		public object RemoveModelFromCache<T>(object key) where T : class
		{
			if (!EnableCache)
				return null;

			if (key == null)
				return null;

			var keyValue = $"{typeof(T).Name}{key}";
			object obj;

			return !keyValue.IsNullOrEmpty() && Cache.TryRemove(keyValue, out obj);
		}
	}
}
