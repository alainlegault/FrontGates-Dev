using Shared.Exceptions;
using Shared.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Repositories
{
	/// <summary>
	/// Utility class for database connectivity.
	/// </summary>
	public class DbUtils
	{
		const int MAX_RETRIES = 5;

		/// <summary>
		/// Returns an open connection to a given SQL Server database.
		/// </summary>
		/// <param name="connectionString">A valid connection string to a SQL Server database.</param>
		/// <returns>An open connection to a database.</returns>
		public static SqlConnection GetDbConnection(string connectionString)
		{
			return GetDbConnection(connectionString, false);
		}

		/// <summary>
		/// Returns an open connection to a given SQL Server database, but without trying
		/// to decrypt the user ID and password in the connection string.
		/// </summary>
		/// <param name="connectionString">A valid connection string to a SQL Server database.</param>
		/// <returns>An open connection to a database.</returns>
		public static SqlConnection GetDbConnectionNoEncryption(string connectionString)
		{
			return GetDbConnection(connectionString, false);
		}


		private static SqlConnection GetDbConnection(string connectionString, bool tryDecrypt)
		{
			var builder = new SqlConnectionStringBuilder();

			try
			{
				builder = new SqlConnectionStringBuilder(connectionString);

				//decrypting the user id and password used, if applicable...
				if (tryDecrypt)
				{
					builder.UserID = StringEncryptor.DecryptWithPassword(builder.UserID, StringEncryptor.DefaultPassword);
					builder.Password = StringEncryptor.DecryptWithPassword(builder.Password, StringEncryptor.DefaultPassword);
				}
			}
			catch (Exception)
			{
				throw new DatabaseException($"The format for the connection string '{builder.ConnectionString}' is invalid.");
			}

			SqlConnection conn = null;
			var retry = 0;
			var sb = new StringBuilder();

			while (retry < MAX_RETRIES)
			{
				try
				{
					conn = new SqlConnection(builder.ConnectionString);

					conn.Open();

					if (conn.State != ConnectionState.Open)
						conn.Open();

					return conn;
				}
				catch (Exception e)
				{
					conn?.Dispose();

					sb.AppendLine(e.ToString());

					retry++;
				}
			}

			sb.AppendLine($"Could not obtain a connection to database '{builder.InitialCatalog}' on server '{builder.DataSource}'.");

			throw new DatabaseException(sb.ToString());
		}
	}
}
