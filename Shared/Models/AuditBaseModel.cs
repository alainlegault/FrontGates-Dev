using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public interface IAuditableEntity
	{
		// Override this method to provide a description of the entity for audit purposes
		string AddEvent();
		string UpdateEvent();
		string DeleteEvent();
		Guid DbTablePkId();
		Guid DbObjectId();
		int DbSeverity();
		int DbCode();
	}

	public abstract class AuditBase : IAuditableEntity
	{
		public enum AuditCode
		{
			//1000  User
			UserAdded = 1001,
			UserUpdated = 1002,
			UserDeleted = 1003
		}

		public enum SeverityCode
		{
			//no notification
			Low = 1,
			//notification on selected areas
			Medium = 2,
			//notification everywhere
			High = 3,
		}

		public AuditCode Code { get; set; }

		public SeverityCode Severity { get; set; }

		public virtual string AddEvent()
		{
			return string.Empty;
		}

		public virtual string UpdateEvent()
		{
			return string.Empty;
		}

		public virtual string DeleteEvent()
		{
			return string.Empty;
		}

		public virtual Guid DbTablePkId()
		{
			return Guid.Empty;
		}

		public virtual Guid DbObjectId()
		{
			return Guid.Empty;
		}

		public virtual int DbSeverity()
		{
			return 0;
		}

		public virtual int DbCode()
		{
			return 0;
		}
	}
}
