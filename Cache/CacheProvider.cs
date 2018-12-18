using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{
	public interface ICacheProvider
	{
		bool EnableCache { get; set; }
		int CacheInSeconds { get; set; }
		bool ForceUpdate { get; set; }
		Guid ReferenceId { get; set; }
		T Execute<T>(BaseCacheProvider.Request<T> request) where T : class, new();
		T GetModelFromCache<T>(object key, T model, Func<T, T> populate, bool useTypeFullNameInKey = true) where T : class, new();
		object RemoveModelFromCache<T>(object key) where T : class;
	}

	public abstract class BaseCacheProvider
	{
		public enum CallType
		{
			Get,
			GetList,
			Put,
			Save,
			Remove
		}

		public class CacheItem<T> where T : class
		{
			public object Item { get; set; }
			public DateTime Expiration { get; set; }
			public object Key { get; set; }
			public string Region { get; set; }
			public object ReferenceId { get; set; }

			public bool HasExpired()
			{
				return Expiration < DateTime.UtcNow;
			}
		}

		public class Request<T> where T : class
		{
			public CallType Call { get; set; }
			public object Key { get; set; }
			public T Model { get; set; }
			public Func<T, T> Populate { get; set; }
			public Type Type => typeof(T);
			public string TypeName => Type.FullName;
			public List<KeyValuePair<string, object>> Items { get; set; }
		}
	}
}
