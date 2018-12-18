using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class AuditModel : BaseModel
	{
		public string Email { get; set; }
		public string TableName { get; set; }
		public string EventType { get; set; }
		public string EventValue { get; set; }
		public Guid TablePkId { get; set; }
		public Guid ObjectId { get; set; }
		public int Severity { get; set; }
		public int Code { get; set; }

		public UserModel User { get; set; }
	}
}
