using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logger.Extensions
{
	public static class DateExtensions
	{
		public static DateTime AsDateTime(this string s)
		{
			if (string.IsNullOrEmpty(s))
				return default(DateTime);

			DateTime.TryParse(s, out DateTime i);

			return i;
		}

		public static List<DateTime> AsListOfDateTime(this string s, string delimiter = ",")
		{
			var ids = new List<DateTime>();

			if (string.IsNullOrEmpty(s))
				return ids;

			ids.AddRange(s.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(o => o.AsDateTime()));

			return ids;
		}

		public static bool IsEarlierThanNow(this DateTime t1, bool includeUniversalTime = false)
		{
			return t1.IsEarlierThanSource(includeUniversalTime ? DateTime.Now.ToUniversalTime() : DateTime.Now.Date, includeUniversalTime);
		}

		public static bool IsLaterThanNow(this DateTime t1, bool includeUniversalTime = false)
		{
			return t1.IsLaterThanSource(includeUniversalTime ? DateTime.Now.ToUniversalTime() : DateTime.Now.Date, includeUniversalTime);
		}

		public static bool IsEqualToNow(this DateTime t1, bool includeUniversalTime = false)
		{
			return t1.IsEqualToSource(includeUniversalTime ? DateTime.Now.ToUniversalTime() : DateTime.Now.Date, includeUniversalTime);
		}

		public static bool IsEarlierThanSource(this DateTime t1, DateTime t2, bool includeUniversalTime = false)
		{
			var results = DateTime.Compare(includeUniversalTime ? t1.ToUniversalTime() : t1, includeUniversalTime ? t2.ToUniversalTime() : t2);

			switch (results)
			{
				default:
					return true;
				case 0:
					return !t1.IsEqualToSource(t2, includeUniversalTime);
				case 1:
					return false;
			}
		}

		public static bool IsLaterThanSource(this DateTime t1, DateTime t2, bool includeUniversalTime = false)
		{
			var results = DateTime.Compare(includeUniversalTime ? t1.ToUniversalTime() : t1, includeUniversalTime ? t2.ToUniversalTime() : t2);

			switch (results)
			{
				default:
					return false;
				case 1:
					return true;
			}
		}

		public static bool IsEqualToSource(this DateTime t1, DateTime t2, bool includeUniversalTime = false)
		{
			var results = DateTime.Compare(includeUniversalTime ? t1.ToUniversalTime() : t1, includeUniversalTime ? t2.ToUniversalTime() : t2);

			switch (results)
			{
				case 0:
					return true;
				default:
					return false;
			}
		}
	}
}
