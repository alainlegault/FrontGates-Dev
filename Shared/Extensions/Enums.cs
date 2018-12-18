using Newtonsoft.Json;
using Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Extensions
{
	public static class EnumExtensions
	{
		public static string GetFriendlyDescription(this Enum value, bool replaceUnderscoreWithSpace = false)
		{
			const string defaultValue = "";

			if (value == null)
				return defaultValue;

			var field = value.GetType().GetField(value.ToString());

			if (field == null)
				return defaultValue;

			var attributes = field.GetCustomAttributes(typeof(FriendlyDescriptionAttribute), true);

			if (attributes.Length > 0)
				return ((FriendlyDescriptionAttribute)attributes[0]).Text;

			return (replaceUnderscoreWithSpace ? value.ToString().Replace("_", " ") : value.ToString());
		}
	}

	public static class EnumsToList
	{
		public static List<KeyValuePair<int, string>> ConvertEnumsToListOfKvp(this Type t, bool getFriendlyDescription = true)
		{
			return (from object item in Enum.GetValues(t) select new KeyValuePair<int, string>((int)item, (item as Enum).GetFriendlyDescription())).ToList();
		}

		public static string EnumToJson<T>()
		{
			var values = Enum.GetValues(typeof(T)).Cast<int>();
			var enumDictionary = values.ToDictionary(value => Enum.GetName(typeof(T), value));

			return JsonConvert.SerializeObject(enumDictionary);
		}
	}
}
