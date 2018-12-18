using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shared.Extensions
{
	public static class LinqExtension
	{

		public static List<List<T>> SplitInChunks<T>(this List<T> source, int batch)
		{
			return source
				.Select((x, i) => new { Index = i, Value = x })
				.GroupBy(x => x.Index / batch)
				.Select(x => x.Select(v => v.Value).ToList())
				.ToList();
		}

		public static List<T> Page<T>(this List<T> list, int pageNumber, int pageSize)
		{
			return list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
		}

		/// <summary>
		/// Not accurate, needs to be filtered by property in if(!hs.Add(myList[i]))
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="myList"></param>
		/// <returns></returns>
		public static bool HasDuplicates<T>(this List<T> myList)
		{
			var hs = new HashSet<T>();

			for (var i = 0; i < myList.Count; ++i)
				if (!hs.Add(myList[i]))
					return true;

			return false;
		}

		public static bool HasDuplicates(this List<XElement> myList, string elementColumnName = "Id")
		{
			var hs = new HashSet<string>();

			for (var i = 0; i < myList.Count; ++i)
				if (!hs.Add(myList[i].Element(elementColumnName).Value))
					return true;

			return false;
		}

		public static bool HasDuplicates(this List<XElement> myList, out List<XElement> list, string elementColumnName = "Id")
		{
			var hs = new HashSet<string>();
			var duplicates = new List<XElement>();
			var hasDuplicates = false;

			for (var i = 0; i < myList.Count; ++i)
				if (!hs.Add(myList[i].Element(elementColumnName).Value))
				{
					duplicates.Add(myList[i]);
					hasDuplicates = true;
				}

			list = duplicates;
			return hasDuplicates;
		}

		public static bool ObjectIdExists<T>(this List<T> source, Guid objectId)
		{
			var dynamicList = new List<dynamic>();
			source.ForEach(d => dynamicList.Add((dynamic)d));

			return dynamicList.Any(o => o.Id.Equals(objectId));
		}

		public static bool AnyOrNotNull(this string[] source)
		{
			if (source == null || source.Length <= 0)
				return false;

			return source.Length > 0;
		}

		public static bool AnyOrNotNull(this string[] source, Func<string, bool> predicate)
		{
			if (source == null || source.Length <= 0)
				return false;

			var results = source.ToList().Where(predicate).AsParallel().ToList();

			return results.Any();
		}

		public static bool AnyOrNotNull<T>(this List<T> source)
		{
			return source != null && source.Any();
		}

		public static bool AnyOrNotNull<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
		{
			if (source == null)
				return false;

			var results = source.Where(predicate).AsParallel().ToList();

			return results.Any();
		}

		public static bool AnyOrNotNull<TSource>(this List<TSource> source, Func<TSource, bool> predicate, out List<TSource> results)
		{
			if (source == null)
			{
				results = new List<TSource>();

				return false;
			}

			results = source.Where(predicate).AsParallel().ToList();

			return results.Count > 0;
		}

		public static bool AnyOrNotNull<TSource>(this IEnumerable<TSource> source)
		{
			return source != null && source.Any();
		}

		public static bool AnyOrNotNull<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			if (source == null)
				return false;

			var results = source.Where(predicate).AsParallel().ToList();

			return results.Any();
		}

		public static bool AnyOrNotNull<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource result)
		{
			result = default(TSource);

			if (source == null)
				return false;

			var results = source.Where(predicate).AsParallel().ToList();

			if (results.Count > 0 && results.FirstOrDefault() != null)
				result = results.FirstOrDefault();

			return results.Count > 0;
		}

		public static bool AnyOrNotNull<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out List<TSource> results)
		{
			if (source == null)
			{
				results = new List<TSource>();
				return false;
			}

			results = source.Where(predicate).AsParallel().ToList();

			return results.Count > 0;
		}

		public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out List<TSource> results)
		{
			if (source == null)
			{
				results = new List<TSource>();
				return false;
			}

			results = source.Where(predicate).AsParallel().ToList();

			return results.Count > 0;
		}

		public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource result)
		{
			result = default(TSource);

			if (source == null)
				return false;

			var results = source.Where(predicate).AsParallel().ToList();

			if (results.Count > 0 && results.FirstOrDefault() != null)
				result = results.FirstOrDefault();

			return results.Count > 0;
		}

		public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource result)
		{
			result = default(TSource);

			if (source == null)
				return default(TSource);

			var results = source.Where(predicate).AsParallel().ToList();

			if (results.Count > 0 && results.FirstOrDefault() != null)
				result = results.FirstOrDefault();

			return result;
		}

		public static List<T> Shuffle<T>(this List<T> list)
		{
			return list.OrderBy(t => Guid.NewGuid()).ToList();
		}
	}
}
