using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Extensions
{
	public static class DateExtension
	{
		//public static string ToFormattedDateTime(this DateTime dt, string format = "D", string cultureInfo = "en-CA", bool includeTime = false, string defaultTimeFormat = "h:mm:ss tt", bool useLocalTimeZone = true)
		//{
		//	var culture = new CultureInfo(cultureInfo);
		//	var dateTime = dt;

		//	if (useLocalTimeZone)
		//		dateTime = dt.ToLocalTimeZone();

		//	return !includeTime ? dateTime.ToString(format, culture) : string.Concat(ToFormattedDateTime(dateTime, format, cultureInfo), dateTime.ToString(" " + defaultTimeFormat, culture));
		//}

		//public static string ToFormattedDateTime(this string dt, string format = "D", string cultureInfo = "en-CA", bool includeTime = false, string defaultTimeFormat = "h:mm:ss tt", bool useLocalTimeZone = true)
		//{
		//	DateTime dateTime;
		//	DateTime.TryParse(dt, new CultureInfo(cultureInfo), DateTimeStyles.None, out dateTime);
		//	return ToFormattedDateTime(dateTime, format, cultureInfo, includeTime, defaultTimeFormat, useLocalTimeZone);
		//}

		public static DateTime ToLocalTimeZone(this DateTime utcDate, int timeZoneOffset = -4)
		{
			////var user = Thread.CurrentPrincipal as CustomPrincipal;
			////var timeZone = user != null ?
			////				user.TimeZone :
			////				DefaultTimeZone;
			////var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone) ?? TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZone);

			//var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") ?? TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			//return TimeZoneInfo.ConvertTimeFromUtc(utcDate, localTimeZone);

			return utcDate.AddHours(timeZoneOffset);
		}

		//Less than zero t1 is earlier than t2. 
		//Zero t1 is the same as t2. 
		//Greater than zero t1 is later than t2. 
		public static string DaySuffix(this int day)
		{
			var result = string.Empty;
			switch (day.ToString().Substring(day.ToString().Length - 1, 1))
			{
				case "1":
					if (!day.Equals(11))
						result = "st";
					else
						return "th";
					break;
				case "2":
					result = !day.Equals(12) ? "nd" : "th";
					break;
				case "3":
					if (!day.Equals(13))
						result = "rd";
					else
						return "th";
					break;
				case "4":
				case "5":
				case "6":
				case "7":
				case "8":
				case "9":
				case "0":
					result = "th";
					break;
			}

			return $"{result}";
		}

		public static string GetTimeAgo(this DateTime dt)
		{
			var t = DateTime.UtcNow - dt;
			var deltaSeconds = t.TotalSeconds;
			var deltaMinutes = deltaSeconds / 60.0f;

			if (deltaSeconds < 5)
				return "Just now";

			if (deltaSeconds < 60)
				return Math.Floor(deltaSeconds) + " seconds ago";

			if (deltaSeconds < 120)
				return "A minute ago";

			if (deltaMinutes < 60)
				return Math.Floor(deltaMinutes) + " minutes ago";

			if (deltaMinutes < 120)
				return "An hour ago";

			if (deltaMinutes < (24 * 60))
				return $"{(int)Math.Floor(deltaMinutes / 60)} hours ago";

			if (deltaMinutes < (24 * 60 * 2))
				return "Yesterday";

			if (deltaMinutes < (24 * 60 * 7))
				return $"{(int)Math.Floor(deltaMinutes / (60 * 24))} days ago";

			if (deltaMinutes < (24 * 60 * 14))
				return "Last week";

			if (deltaMinutes < (24 * 60 * 31))
				return $"{(int)Math.Floor(deltaMinutes / (60 * 24 * 7))} weeks ago";

			if (deltaMinutes < (24 * 60 * 61))
				return "Last month";

			if (deltaMinutes < (24 * 60 * 365.25))
				return $"{(int)Math.Floor(deltaMinutes / (60 * 24 * 30))} months ago";

			if (deltaMinutes < (24 * 60 * 731))
				return "Last year";

			return $"{(int)Math.Floor(deltaMinutes / (60 * 24 * 365))} years ago";
		}

		public static DateTime AsDateTime(this string s)
		{
			if (string.IsNullOrEmpty(s))
				return default(DateTime);

			DateTime i;
			DateTime.TryParse(s, out i);
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

		public static bool IsDateEarlierThanNow(this DateTime t1, bool includeUniversalTime = false)
		{
			return t1.IsDateEarlierThanSource(includeUniversalTime ? DateTime.Now.ToUniversalTime() : DateTime.Now.Date, includeUniversalTime);
		}

		public static bool IsDateLaterThanNow(this DateTime t1, bool includeUniversalTime = false)
		{
			return t1.IsDateLaterThanSource(includeUniversalTime ? DateTime.Now.ToUniversalTime() : DateTime.Now.Date, includeUniversalTime);
		}

		public static bool IsDateEqualToNow(this DateTime t1, bool includeUniversalTime = false)
		{
			return t1.IsDateEqualToSource(includeUniversalTime ? DateTime.Now.ToUniversalTime() : DateTime.Now.Date, includeUniversalTime);
		}

		public static bool IsDateEarlierThanSource(this DateTime t1, DateTime t2, bool includeUniversalTime = false)
		{
			var results = DateTime.Compare(includeUniversalTime ? t1.ToUniversalTime() : t1, includeUniversalTime ? t2.ToUniversalTime() : t2);

			switch (results)
			{
				default:
					return true;
				case 0:
					return !t1.IsDateEqualToSource(t2, includeUniversalTime);
				case 1:
					return false;
			}
		}

		public static bool IsDateLaterThanSource(this DateTime t1, DateTime t2, bool includeUniversalTime = false)
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

		public static bool IsDateEqualToSource(this DateTime t1, DateTime t2, bool includeUniversalTime = false)
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
