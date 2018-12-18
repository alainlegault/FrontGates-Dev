using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utils
{
	public class StringEncoding
	{
		private static readonly char[] Padding = { '=' };

		public static string EncodeBase64(string s)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
		}

		public static string DecodeBase64(string s)
		{
			if (s.IsNullOrEmpty())
				return string.Empty;

			try
			{
				return Encoding.UTF8.GetString(Convert.FromBase64String(s));
			}
			catch (Exception ex)
			{
				return string.Empty;
			}
		}

		public static bool IsValidBase64String(string s)
		{
			if (s.IsNullOrEmpty())
				return false;

			s = s.Trim();

			var mod4 = s.Length % 4;

			if (mod4 != 0)
				return false;

			var i = 0;
			var checkPadding = false;
			var paddingCount = 1;

			for (i = 0; i < s.Length; i++)
			{
				var c = s[i];

				if (checkPadding)
				{
					if (c != '=')
						return false;

					paddingCount++;

					if (paddingCount > 3)
						return false;

					continue;
				}

				if (c >= 'A' && c <= 'z' || c >= '0' && c <= '9')
					continue;

				switch (c)
				{
					case '+':
					case '/':
						continue;
					case '=':
						checkPadding = true;
						continue;
				}

				return false;
			}

			return true;
		}

		public static string EncodeUrl(string s)
		{
			return Convert.ToBase64String(Encoding.ASCII.GetBytes(s)).TrimEnd(Padding).Replace('+', '-').Replace('/', '_');
		}

		public static string DecodeUrl(string s)
		{
			var incoming = s.Replace('_', '/').Replace('-', '+');

			switch (s.Length % 4)
			{
				case 2: incoming += "=="; break;
				case 3: incoming += "="; break;
			}

			var bytes = Convert.FromBase64String(incoming);
			var originalText = Encoding.ASCII.GetString(bytes);

			return originalText;
		}
	}
}
