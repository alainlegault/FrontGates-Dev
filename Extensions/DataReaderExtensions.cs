using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logger.Extensions
{
	public static class DataReaderExtensions
	{
		public static string MapString(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(string) : r.GetString(ord);
		}

		public static int MapInt32(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(int) : r.GetInt32(ord);
		}

		public static int? MapNullableInt32(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetInt32(ord);
		}

		public static long MapInt64(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(long) : r.GetInt64(ord);
		}

		public static long? MapNullableInt64(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetInt64(ord);
		}

		public static double MapDouble(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(double) : r.GetDouble(ord);
		}

		public static double? MapNullableDouble(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetDouble(ord);
		}

		public static decimal MapDecimal(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(decimal) : r.GetDecimal(ord);
		}

		public static decimal? MapNullableDecimal(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetDecimal(ord);
		}

		public static DateTime MapDateTime(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(DateTime) : r.GetDateTime(ord);
		}

		public static DateTime? MapNullableDateTime(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetDateTime(ord);
		}

		public static bool MapBoolean(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(bool) : r.GetBoolean(ord);
		}

		public static bool? MapNullableBoolean(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetBoolean(ord);
		}

		public static Guid MapGuid(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(Guid) : r.GetGuid(ord);
		}

		public static Guid? MapNullableGuid(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetGuid(ord);
		}

		public static byte MapByte(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(byte) : r.GetByte(ord);
		}

		public static byte? MapNullableByte(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetByte(ord);
		}

		public static byte[] MapByteArray(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(byte[]) : (byte[])r.GetValue(ord);
		}
	}
}
