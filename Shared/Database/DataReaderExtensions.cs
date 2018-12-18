using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Database
{
	/// <summary>
	/// Utility class of the manipulation of records returned by a datareader from a relational database.
	/// </summary>
	public static class DataReaderExtensions
	{
		/// <summary>
		/// Maps a name field of a data reader record to a string.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static string MapString(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(string);

			return r.GetString(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to an integer.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static int MapInt32(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(int);

			return r.GetInt32(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to a Date Time.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static int? MapNullableInt32(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(int);

			return r.GetInt32(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to an integer.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static long MapInt64(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(long) : r.GetInt64(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to a Date Time.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static long? MapNullableInt64(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? default(long) : r.GetInt64(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to a double.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static double MapDouble(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(double);

			return r.GetDouble(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to a nullable double.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static double? MapNullableDouble(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetDouble(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to a decimal.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static decimal MapDecimal(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(decimal);

			return r.GetDecimal(ord);

		}

		public static decimal? MapNullableDecimal(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetDecimal(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to a Date Time.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static DateTime MapDateTime(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(DateTime);

			return r.GetDateTime(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to a Date Time.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static DateTime? MapNullableDateTime(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return null;

			return r.GetDateTime(ord);
		}


		/// <summary>
		/// Maps a name field of a data reader record to aBoolean
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static bool MapBoolean(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(bool);

			return r.GetBoolean(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to an globally unique identifier (GUID).
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static Guid MapGuid(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(Guid);

			return r.GetGuid(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to an globally unique identifier (GUID).
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static Guid? MapNullableGuid(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			return r.IsDBNull(ord) ? (Guid?)null : r.GetGuid(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to a Byte.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static byte MapByte(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(byte);

			return r.GetByte(ord);
		}

		/// <summary>
		/// Maps a name field of a data reader record to a byte[].
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which a value is wanted.</param>
		/// <returns>The value of the named field; otherwize null. </returns>
		public static byte[] MapByteArray(this IDataReader r, string name)
		{
			var ord = r.GetOrdinal(name);

			if (r.IsDBNull(ord))
				return default(byte[]);

			return (byte[])r.GetValue(ord);
		}

		/// <summary>
		/// Check if a field exists in IDataReader.
		/// </summary>
		/// <param name="r">The <see cref="IDataReader"/> implementation.</param>
		/// <param name="name">The name of the field for which want to check.</param>
		/// <returns>true if the column exist; otherwize false. </returns>
		public static bool ColumnExists(this IDataReader r, string columnName)
		{
			try
			{
				return r.GetOrdinal(columnName) >= 0;
			}
			catch (IndexOutOfRangeException)
			{
				return false;
			}
		}
	}
}
