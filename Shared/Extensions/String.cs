using Shared.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shared.Extensions
{
	public static class String
	{
		public static string DecodeBase64(this string value)
		{
			return StringEncoding.DecodeBase64(value);
		}

		public static string EncodeBase64(this string value)
		{
			return StringEncoding.EncodeBase64(value);
		}

		public static string EncodeUrl(this string value)
		{
			return StringEncoding.EncodeUrl(value);
		}

		public static string DecodeUrl(this string value)
		{
			return StringEncoding.DecodeUrl(value);
		}

		public static byte[] FromBase64ToByteArray(this string s)
		{
			return Convert.FromBase64String(s);
		}

		public static string FromByteArrayToBase64(this byte[] b)
		{
			return Convert.ToBase64String(b);
		}

		public static string MakeFileSystemSafe(this string s)
		{
			return new string(s.Where(IsFileSystemSafe).ToArray());
		}

		public static bool IsFileSystemSafe(char c)
		{
			return !Path.GetInvalidFileNameChars().Contains(c);
		}

		public static string CleanUp(this string s, string dirtyCharacters, string replacementCharacter = "")
		{
			return dirtyCharacters.ToCharArray().Aggregate(s, (current, c) => current.Replace(c.ToString(), replacementCharacter));
		}

		public static bool IsNullOrEmpty(this string s)
		{
			return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
		}

		public static bool IsNotNullOrEmpty(this string s)
		{
			return !(s.IsNullOrEmpty());
		}

		public static bool HasValue(this string s)
		{
			return !string.IsNullOrWhiteSpace(s);
		}

		public static string IfNoValue(this string value, string defaultValue = null)
		{
			return value.HasValue() ? value : defaultValue;
		}

		public static string Crop(this string s, int start = 0, int end = 0, string trailer = "")
		{
			return (s.Length > start && s.Length >= end ? s.Substring(start + 1, (end - start) - 1) : s) + trailer;
		}

		public static Stream ToStream(this string s)
		{
			var byteArray = Encoding.UTF8.GetBytes(s);
			using (var stream = new MemoryStream(byteArray))
				return stream;
		}

		public static string ToString(this Stream stream)
		{
			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}

		public static bool IsValidBase64String(this string s)
		{
			if (string.IsNullOrEmpty(s))
				return false;

			return ((s.Trim().Length % 4 == 0) && Regex.IsMatch(s.Trim(), @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None));
		}

		public static string EncodeQuotes(this string str)
		{
			return str.Replace("\"", "&quot;");
		}

		/// <summary>
		/// Remove HTML tags from string using char array.
		/// </summary>
		public static string RemoveHtml(this string source, bool removeLineBreaks = true)
		{
			if (source.IsNullOrEmpty())
				return string.Empty;

			var array = new char[source.Length];
			var arrayIndex = 0;
			var inside = false;

			for (var i = 0; i < source.Length; i++)
			{
				var let = source[i];

				switch (@let)
				{
					case '<':
						inside = true;

						continue;
					case '>':
						inside = false;

						continue;
				}

				if (inside)
					continue;

				array[arrayIndex] = @let;
				arrayIndex++;
			}

			return new string(array, 0, arrayIndex).Replace(Environment.NewLine, string.Empty);
		}

		public static string ToFriendlyUrl(this string url)
		{
			if (url == null)
				return "";

			const int maxlen = 80;
			var len = url.Length;
			var prevdash = false;
			var sb = new StringBuilder(len);

			for (var i = 0; i < len; i++)
			{
				var c = url[i];
				if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
				{
					sb.Append(c);
					prevdash = false;
				}
				else if (c >= 'A' && c <= 'Z')
				{
					// tricky way to convert to lowercase
					sb.Append((char)(c | 32));
					prevdash = false;
				}
				else if (c == ' ' || c == ',' || c == '.' || c == '/' || c == '\\' || c == '-' || c == '_' || c == '=')
				{
					if (!prevdash && sb.Length > 0)
					{
						sb.Append('-');
						prevdash = true;
					}
				}
				else if (c >= 128)
				{
					var prevlen = sb.Length;

					sb.Append(RemoveAccents(c.ToString()));

					if (prevlen != sb.Length)
						prevdash = false;
				}
				if (i == maxlen)
					break;
			}

			return prevdash ? sb.ToString().Substring(0, sb.Length - 1) : sb.ToString();
		}

		public static string RemoveAccents(this string input)
		{
			return new string(
				input
				.Normalize(System.Text.NormalizationForm.FormD)
				.ToCharArray()
				.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
				.ToArray());

			// the normalization to FormD splits accented letters in accents+letters
			// the rest removes those accents (and other non-spacing characters)
		}

		static T GetValue<T>(string s)
		{
			var tryParse = typeof(T).GetMethod("TryParse", new[] { typeof(string), typeof(T).MakeByRefType() });
			var t = default(T);

			if (tryParse == null)
				return t;

			var parameters = new object[] { s, t };
			var success = tryParse.Invoke(null, parameters);

			if ((bool)success)
				t = (T)parameters[1];

			return t;
		}

		public static string ArrayToString<T>(this List<T> list, string delimiter = ", ") where T : struct
		{
			if (list.AnyOrNotNull())
				return string.Join(delimiter, list.ToArray());
			return string.Empty;
		}

		public static List<T> StringToArray<T>(this string str, string delimiter = ", ") where T : struct
		{
			if (string.IsNullOrEmpty(str))
				return new List<T>();

			var list = new List<T>();
			var array = str.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
			var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));

			foreach (var value in array)
			{
				var id = default(T);
				id = GetValue<T>(value);

				if (!id.Equals(default(T)))
				{
					var propValue = typeConverter.ConvertFromString(value);
					var item = (T)propValue;

					list.Add(item);
				}
			}
			return list;
		}

		public static string GetVirtualPath(this string physicalPath, string physicalApplicationPath)
		{
			if (!physicalPath.StartsWith(physicalApplicationPath))
				throw new InvalidOperationException("Physical path is not within the application root");

			return "~/" + physicalPath.Substring(physicalApplicationPath.Length).Replace("\\", "/");
		}

		public static string FormatEnumValue(this string str, string stringToReplace = "_", string replacementString = " ")
		{
			return str.Replace(stringToReplace, replacementString);
		}

		public static string Right(this string value, int length)
		{
			if (value.Length > length)
				return value.Substring(value.Length - length);
			return value;
		}

		public static string Left(this string value, int length)
		{
			if (value.Length >= length)
				return value.Substring(0, length);
			return value;
		}

		public static string Mid(this string value, int startIndex, int length)
		{
			if (value.Length >= length && startIndex >= 0)
				return value.Substring(startIndex, length);
			return value;
		}

		public static string Mid(this string value, int startIndex)
		{
			if (startIndex >= 0)
				return value.Substring(startIndex);
			return value;
		}

		public static string Truncate(this string str, int start, int end, string replacmentString = "", int maximumStartingCharacter = 10, int maximumEndingCharacters = 10)
		{
			if (str.Length >= end)
			{
				if (!string.IsNullOrEmpty(replacmentString))
				{
					var part1 = "";
					if (str.Length >= maximumStartingCharacter)
						part1 = str.Substring(0, maximumStartingCharacter);

					var part2 = "";
					if (str.Length > maximumEndingCharacters)
						part2 = str.Right(Math.Min((int)(str.Length * 0.3), maximumEndingCharacters));

					var part3 = str;
					if (!string.IsNullOrEmpty(part1) && !string.IsNullOrEmpty(part2))
						part3 = string.Format("{0}{1}{2}", part1, replacmentString, part2);

					return part3;
				}
				return str.Substring(start, end);
			}
			return str;
		}

		/// <summary>
		/// Implement's VB's Like operator logic.
		/// </summary>
		public static bool IsLike(this string s, string pattern)
		{
			// Characters matched so far
			var matched = 0;

			// Loop through pattern string
			for (var i = 0; i < pattern.Length;)
			{
				// Check for end of string
				if (matched >= s.Length)
					return false;

				// Get next pattern character
				var c = pattern[i++];

				if (c == '[') // Character list
				{
					// Text for exclude character
					var exclude = (i < pattern.Length && pattern[i] == '!');

					if (exclude)
						i++;
					// Build character list
					var j = pattern.IndexOf(']', i);

					if (j < 0)
						j = s.Length;

					var charList = CharListToSet(pattern.Substring(i, j - i));

					i = j + 1;

					if (charList.Contains(s[matched]) == exclude)
						return false;

					matched++;
				}
				else if (c == '?') // Any single character
					matched++;
				else if (c == '#') // Any single digit
				{
					if (!char.IsDigit(s[matched]))
						return false;

					matched++;
				}
				else if (c == '*') // Zero or more characters
				{
					if (i < pattern.Length)
					{
						// Matches all characters until
						// next character in pattern
						var next = pattern[i];
						var j = s.IndexOf(next, matched);

						if (j < 0)
							return false;

						matched = j;
					}
					else
					{
						// Matches all remaining characters
						matched = s.Length;
						break;
					}
				}
				else // Exact character
				{
					if (c != s[matched])
						return false;

					matched++;
				}
			}

			// Return true if all characters matched
			return (matched == s.Length);
		}

		/// <summary>
		/// Converts a string of characters to a HashSet of characters. If the string
		/// contains character ranges, such as A-Z, all characters in the range are
		/// also added to the returned set of characters.
		/// </summary>
		/// <param name="charList">Character list string</param>
		private static HashSet<char> CharListToSet(string charList)
		{
			var set = new HashSet<char>();

			for (var i = 0; i < charList.Length; i++)
				if ((i + 1) < charList.Length && charList[i + 1] == '-')
				{
					// Character range
					var startChar = charList[i++];

					i++; // Hyphen

					var endChar = (char)0;

					if (i < charList.Length)
						endChar = charList[i++];

					for (int j = startChar; j <= endChar; j++)
						set.Add((char)j);
				}
				else
					set.Add(charList[i]);

			return set;
		}
	}
}
