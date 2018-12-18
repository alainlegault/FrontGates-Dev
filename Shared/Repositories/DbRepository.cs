using Microsoft.SqlServer.Server;
using Shared.Exceptions;
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
	/// Base class for all database repositories.
	/// </summary>
	public abstract class DbRepository
	{
		private DatabaseName _defaultDatabaseName = DatabaseName.Default;

		/// <summary>
		/// Initializes a new instance of the <see cref="DbRepository"/> class.
		/// </summary>
		/// <param name="context">The domain context.</param>
		/// <param name="dbName"></param>
		protected DbRepository(DomainContext context, DatabaseName dbName = DatabaseName.Default)
		{
			Context = context;
			IsEnlisted = false;
			CurrentDbName = dbName;
		}

		/// <summary>
		/// Gets or sets the current domain context.
		/// </summary>
		protected DomainContext Context { get; set; }

		/// <summary>
		/// Gets or sets the ambient database connection associated with the 
		/// current instance of <see cref="DbRepository"/>.
		/// </summary>
		internal IDbConnection CurrentConnection { get; set; }

		/// <summary>
		/// Gets or sets the current database name
		/// </summary>
		protected DatabaseName CurrentDbName { get; set; }

		/// <summary>
		/// Gets or sets the ambient database transaction associated with the
		/// current instance of <see cref="DbRepository"/>.
		/// </summary>
		public IDbTransaction CurrentTransaction { get; set; }
		//protected internal IDbTransaction CurrentTransaction { get; set; }

		/// <summary>
		/// Gets or sets a value indicating if the curent repository
		/// instance is enlisted within an outer unit of work.
		/// </summary>
		public bool IsEnlisted { get; set; }

		/// <summary>
		/// Creates a connection to the database informed.
		/// </summary>
		/// <param name="dbName">Enumeration of type <see cref="DatabaseName"/>.</param>
		private void InitDbConnection(DatabaseName dbName)
		{
			if (IsEnlisted)
				return;

			if (CurrentConnection != null && CurrentConnection.State != ConnectionState.Closed && CurrentConnection.State != ConnectionState.Broken)
			{
				//if there is a connection opened to another db, throw an exception
				if (CurrentDbName != dbName)
					throw new DatabaseException($"The repository cannot request a connection to database '{dbName}' because it possesses an open connection to database {CurrentDbName}.");

				//auto-rollback on dispose
				if (CurrentTransaction?.Connection != null)
					CurrentTransaction.Rollback();

				if (CurrentConnection.State != ConnectionState.Closed || CurrentConnection.State != ConnectionState.Broken)
				{
					CurrentConnection.Close();

					CurrentConnection = null;
				}
			}

			CurrentConnection = Context.CreateConnection(dbName);
			CurrentDbName = dbName;
		}

		/// <summary>
		/// Runs a repository operation either as part of an outer unit of work or a standalone one. 
		/// If it's the latter case and CurrentConnection is null, it will connect to the CurrentDBName.
		/// </summary>
		/// <param name="fn">A delegate function that will be executed.</param>
		/// <param name="commandType"></param>
		protected void Run(Action<SqlCommand> fn, CommandType commandType = CommandType.StoredProcedure)
		{
			try
			{
				//we create a new connection without a transaction
				if (!IsEnlisted)
					InitDbConnection(CurrentDbName);

				//we create a command that will be used by the current repository
				using (var cmd = CurrentConnection.CreateCommand())
				{
					cmd.Transaction = CurrentTransaction;
					cmd.CommandType = commandType;

					fn((SqlCommand)cmd);
				}
			}
			finally
			{
				//if the repository is not enlisted within an outer unit of work, 
				//we can commit the transaction and close it's connection.
				if (!IsEnlisted)
					CurrentConnection.Close();
			}
		}

		/// <summary>
		/// Runs a repository operation as a standalone operation without a transaction.
		/// It will connect to the database informed.
		/// </summary>
		/// <param name="dbName">Enumeration of type <see cref="DatabaseName"/>.</param>
		/// <param name="fn">Delegate function that will be executed with a parameter of type <see cref="IDbCommand"/>.</param>
		/// <param name="commandType"></param>
		protected void Run(DatabaseName dbName, Action<SqlCommand> fn, CommandType commandType = CommandType.StoredProcedure)
		{
			try
			{
				//we create a new connection without a transaction (for now)
				if (!IsEnlisted)
					InitDbConnection(dbName);
				else
				{
					if (CurrentDbName != dbName)
						throw new DatabaseException($"The current repository is operating on database '{dbName}', but is enlisted to a Unit Of Work on database '{CurrentDbName}'. The repository must be enlisted with the same database as it's Unit Of Work.");
				}

				//we create a command that will be used by the current repository
				using (var cmd = CurrentConnection.CreateCommand())
				{
					cmd.Transaction = CurrentTransaction;
					cmd.CommandType = commandType;

					fn((SqlCommand)cmd);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				//if the repository is not enlisted within an outer unit of work, 
				//we can close its connection.
				if (!IsEnlisted)
					CurrentConnection?.Close();
			}
		}

		public static DataTable CreateDataTable(IEnumerable<Guid> ids)
		{
			var table = new DataTable();

			table.Columns.Add("Id", typeof(Guid));

			foreach (var id in ids)
				table.Rows.Add(id);

			return table;
		}

		public static IEnumerable<SqlDataRecord> CreateSqlDataRecords(IEnumerable<Guid> ids)
		{
			var metaData = new SqlMetaData[1];
			metaData[0] = new SqlMetaData("Id", SqlDbType.UniqueIdentifier);

			var record = new SqlDataRecord(metaData);

			foreach (var id in ids)
			{
				record.SetGuid(0, id);

				yield return record;
			}
		}

		public static IEnumerable<SqlDataRecord> CreateSqlDataRecords(IEnumerable<string> array)
		{
			var metaData = new SqlMetaData[1];
			metaData[0] = new SqlMetaData("s", SqlDbType.NVarChar, -1);

			var record = new SqlDataRecord(metaData);

			foreach (var o in array)
			{
				record.SetString(0, o);

				yield return record;
			}
		}

		public static SqlParameter TableValueParameter(string name, IEnumerable<Guid> ids)
		{
			if (!name.StartsWith("@"))
				name = "@" + name;

			return new SqlParameter(name, CreateSqlDataRecords(ids))
			{
				SqlDbType = SqlDbType.Structured,
				TypeName = "dbo.ListOfIds"
			};
		}

		public static SqlParameter TableValueParameter(string name, IEnumerable<string> array)
		{
			if (!name.StartsWith("@"))
				name = "@" + name;

			return new SqlParameter(name, CreateSqlDataRecords(array))
			{
				SqlDbType = SqlDbType.Structured,
				TypeName = "dbo.ListOfStrings"
			};
		}
	}
}
