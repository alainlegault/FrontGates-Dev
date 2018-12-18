using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Repositories
{
	/// <summary>
	/// Defines the contract that must be implemented b
	/// all units of work.
	/// </summary>
	public interface IUnitOfWork : IDisposable
	{
		/// <summary>
		/// Enlists one or more repositories into the current
		/// unit of work transaction.
		/// </summary>
		/// <param name="repository"></param>
		void Enlist(params IRepository[] repositories);

		/// <summary>
		/// Commits the changes of the set of objects of the 
		/// unit of work to the underlying persistence store.
		/// </summary>
		void Commit();

		/// <summary>
		/// Initiates a transaction for the current unit of work. If this method is called, Commit needs also to be called before the IUnitOfWork instance is disposed of.
		/// </summary>
		void InitTransaction();
	}
}
