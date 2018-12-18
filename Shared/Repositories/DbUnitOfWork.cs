using Microsoft.Practices.Unity.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Repositories
{
	/// <summary>
	/// Implementation of a unit of work for relational databases.
	/// </summary>
	public sealed class DbUnitOfWork : IUnitOfWork
	{
		private readonly IList<DbRepository> _repositories;
		private DomainContext _context;
		private IDbConnection _conn;
		private IDbTransaction _tran;

		/// <summary>
		/// Initializes a new instance of the <see cref="DbUnitOfWork"/> class.
		/// </summary>
		/// <param name="conn">An open connection to a database.</param>
		public DbUnitOfWork(IDbConnection conn)
		{
			Guard.ArgumentNotNull(conn, "conn");

			_conn = conn;
			_repositories = new List<DbRepository>();

			InitTransaction();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbUnitOfWork"/> class.
		/// </summary>
		/// <param name="context">A <see cref="DomainContext"/> object.</param>
		/// <param name="dbName">An enumation <see cref="DatabaseName"/> for the database name to connect to.</param>
		/// <param name="baseRepository"></param>
		public DbUnitOfWork(DomainContext context, DatabaseName dbName = DatabaseName.Default, object baseRepository = null)
		{
			Guard.ArgumentNotNull(context, "context");
			Guard.ArgumentNotNull(dbName, "dbName");

			_context = context;
			_conn = _context.CreateConnection(dbName);

			_repositories = new List<DbRepository>();

			if (baseRepository == null)
				return;

			var repositories = new List<IRepository> { (IRepository)baseRepository };

			Enlist(repositories.ToArray());
			InitTransaction();
		}


		/// <summary>
		/// Enlists one or more repositories into the current
		/// unit of work transaction.
		/// </summary>
		/// <param name="repositories"></param>
		public void Enlist(params IRepository[] repositories)
		{
			foreach (var r in repositories)
			{
				if (r == null)
					continue;

				if (!(r is DbRepository dbRepo))
					continue;

				dbRepo.CurrentConnection = _conn;
				dbRepo.CurrentTransaction = _tran;
				dbRepo.IsEnlisted = true;

				_repositories.Add(dbRepo);
			}
		}

		/// <summary>
		/// Commits the changes of the set of objects of the 
		/// unit of work to the underlying persistence store.
		/// </summary>
		public void Commit()
		{
			_tran?.Commit();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, 
		/// releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			//auto-rollback on dispose
			if (_tran?.Connection != null)
				_tran.Rollback();

			if (_conn != null && (_conn.State != ConnectionState.Closed || _conn.State != ConnectionState.Broken))
			{
				_conn.Close();

				_conn = null;
			}

			foreach (var dbRepo in _repositories)
			{
				dbRepo.CurrentConnection = null;
				dbRepo.CurrentTransaction = null;
				dbRepo.IsEnlisted = false;
			}
		}

		/// <summary>
		/// Begins a transaction.
		/// </summary>
		public void InitTransaction()
		{
			_tran = _conn.BeginTransaction(IsolationLevel.ReadUncommitted);

			foreach (var dbRepository in _repositories)
			{
				var r = (IRepository)dbRepository;

				if (r == null)
					continue;

				if (r is DbRepository dbRepo)
					dbRepo.CurrentTransaction = _tran;
			}
		}
	}
}
