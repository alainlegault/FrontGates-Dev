using Shared.Extensions;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{
	public enum ReferenceIdType
	{
		PkId,
		Email
	}

	public class MemoryCacheProvider : BaseCacheProvider, ICacheProvider
	{
		public static MemoryCache Cache { get; set; }
		public bool EnableCache { get; set; }
		public int CacheInSeconds { get; set; }
		public bool ForceUpdate { get; set; }
		public Guid ReferenceId { get; set; }

		public MemoryCacheProvider()
		{
			Cache = MemoryCache.Default;
			EnableCache = (ConfigurationManager.AppSettings["EnableCache"] ?? "false").To<bool>();
			CacheInSeconds = (ConfigurationManager.AppSettings["CacheDurationInSeconds"] ?? "120").To<int>();
		}

		public MemoryCacheProvider(bool enableCache, int cacheInSeconds)
		{
			Cache = MemoryCache.Default;
			EnableCache = enableCache;
			CacheInSeconds = cacheInSeconds;
		}

		public T Execute<T>(Request<T> request) where T : class, new()
		{
			return GetModelFromCache(request.Key, request.Model, request.Populate);
		}

		public T Add<T>(object key, T model, bool useTypeFullNameInKey = true) where T : class, new()
		{
			if (!EnableCache)
				return model;

			if (key == null)
				return null;

			var typeName = typeof(T).FullName;
			var keyValue = useTypeFullNameInKey ? $"{typeName}{key}" : key.ToString() ?? "";
			var expirationDate = DateTime.UtcNow.AddSeconds(new TimeSpan(0, 0, 0, CacheInSeconds).TotalSeconds);
			DateTimeOffset offset = expirationDate;
			var obj = new CacheItem<T>
			{
				Item = model,
				Expiration = expirationDate,
				Key = keyValue,
				Region = typeName,
				ReferenceId = (model as BaseModel)?.Id ?? Guid.Empty
			};

			Cache.Set(keyValue, obj, offset);

			return model;
		}

		public async Task<T> GetModelFromCacheAsync<T>(object key, T model, Func<T, Task<T>> populate, bool useTypeFullNameInKey = true) where T : class, new()
		{
			if (!EnableCache)
				return await populate(model);

			if (key == null)
				return null;

			var typeName = typeof(T).FullName;
			var keyValue = useTypeFullNameInKey ? $"{typeName}" : key.ToString() ?? "";

			if (keyValue.IsNullOrEmpty())
				return null;

			if (Cache[keyValue] != null && !ForceUpdate && (Cache[keyValue] is CacheItem<T> cacheItem && !cacheItem.HasExpired()))
				return cacheItem.Item as T ?? new T();

			model = await populate(model);

			var expirationDate = DateTime.UtcNow.AddSeconds(new TimeSpan(0, 0, 0, CacheInSeconds).TotalSeconds);
			DateTimeOffset offset = expirationDate;
			var obj = new CacheItem<T>
			{
				Item = model,
				Expiration = expirationDate,
				Key = keyValue,
				Region = typeName,
				ReferenceId = (model as BaseModel)?.Id ?? Guid.Empty
			};

			Cache.Set(keyValue, obj, offset);

			if (ForceUpdate && keyValue.Equals($"{typeName}Save") && (model as BaseModel) != null)
				RemoveModelFromCache<T>($"{(model as BaseModel).Id}");

			if (ForceUpdate)
				ForceUpdate = false;

			return obj.Item as T ?? new T();
		}

		public T GetModelFromCache<T>(object key, T model, Func<T, T> populate, bool useTypeFullNameInKey = true) where T : class, new()
		{
			if (!EnableCache)
				return populate(model);

			if (key == null)
				return null;

			var typeName = typeof(T).FullName;
			var keyValue = useTypeFullNameInKey ? $"{typeName}{key}" : key.ToString() ?? "";

			if (keyValue.IsNullOrEmpty())
				return null;

			if (Cache[keyValue] != null && !ForceUpdate)
			{
				var cacheItem = Cache[keyValue] as CacheItem<T>;

				if (cacheItem != null && !cacheItem.HasExpired())
					return cacheItem.Item as T ?? new T();
			}

			model = populate(model);

			var expirationDate = DateTime.UtcNow.AddSeconds(new TimeSpan(0, 0, 0, CacheInSeconds).TotalSeconds);
			DateTimeOffset offset = expirationDate;
			var obj = new CacheItem<T>
			{
				Item = model,
				Expiration = expirationDate,
				Key = keyValue,
				Region = typeName,
				ReferenceId = (model as BaseModel)?.Id ?? Guid.Empty
			};

			Cache.Set(keyValue, obj, offset);

			if (ForceUpdate && keyValue.Equals($"{typeName}Save") && (model as BaseModel) != null)
				RemoveModelFromCache<T>($"{(model as BaseModel).Id}");

			if (ForceUpdate)
				ForceUpdate = false;

			return obj.Item as T ?? new T();
		}

		public object RemoveModelFromCache<T>(object key) where T : class
		{
			if (!EnableCache)
				return null;

			if (key == null)
				return null;

			var keyValue = $"{typeof(T).FullName}{key}";

			return keyValue.IsNullOrEmpty() ? null : Cache.Remove(keyValue);
		}

		public object RemoveModelFromCache<T>(object key = null, bool useContains = false) where T : class, new()
		{
			return null;

			if (!EnableCache)
				return null;

			key = (key ?? "").ToString().IsNullOrEmpty() ? typeof(T).Name : key?.ToString() ?? "";

			if (key.ToString().IsNullOrEmpty())
				return null;

			var list = new List<object>();

			var obj = Cache[$"ServiceItem-GetById-{key}"];

			if (obj == null)
			{
				var a = "";
				obj = Cache.Where(o => o.Key.ToLower().Contains(key.ToString().ToLower()));
			}

			foreach (var item in Cache.Where(o => o.Key.IsNotNullOrEmpty() && (useContains ? o.Key.Contains(key.ToString()) : o.Key.StartsWith(key.ToString()))))
				list.Add(Cache.Remove(item.Key));

			return list;
		}
	}
}
