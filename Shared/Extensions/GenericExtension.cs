using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Extensions
{
	public static class GenericExtension
	{
		public static T To<T>(this string s)
		{
			return s.IsNullOrEmpty() ? default(T) : ((object)s).To<T>();
		}

		public static T To<T>(this object o)
		{
			if (o is string && typeof(T) == typeof(Guid))
				return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(o.ToString());

			return o != null && o.ToString() != "" ?
				(T)Convert.ChangeType(o, typeof(T)) :
				((o as string) == null || ((o as string) == "") ?
					default(T) :
					(T)Convert.ChangeType(o, typeof(T)));
		}

		public static bool IsValidObject<T>(this List<T> objs)
		{
			return objs.AnyOrNotNull();
		}

		public static bool IsValidObject<T>(this T obj)
		{
			return obj != null && (obj as dynamic).Id != Guid.Empty;
		}

		public static string GetDictionaryItemByIndex(this Dictionary<string, bool> dic, int index)
		{
			return dic.ElementAt(index).Key;
		}

		public static List<KeyValuePair<TKey, TValue>> ConvertDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
		{
			var keyValueList = new List<KeyValuePair<TKey, TValue>>();

			foreach (TKey key in dictionary.Keys)
				keyValueList.Add(new KeyValuePair<TKey, TValue>(key, dictionary[key]));

			return keyValueList;
		}

		public static List<KeyValuePair<string, string>> ConvertDictionary(this Dictionary<string, string> dic)
		{
			var keyValueList = new List<KeyValuePair<string, string>>();

			foreach (var kvp in dic)
				keyValueList.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value));

			return keyValueList;
		}

		public static Dictionary<int, string> ConvertEnumToDictionary(Type enumerationType)
		{
			var dictionary = new Dictionary<int, string>();

			if (enumerationType.IsEnum)
				foreach (int value in Enum.GetValues(enumerationType))
					dictionary.Add(value, Enum.GetName(enumerationType, value));

			return dictionary;
		}
	}
}
