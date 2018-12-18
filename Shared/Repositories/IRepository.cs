using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Repositories
{
	/// <summary>
	/// Defines the interface that must be implemented by all
	/// data repositories.
	/// </summary>
	public interface IRepository
	{
		bool DisableAudit { get; set; }
	}
}
