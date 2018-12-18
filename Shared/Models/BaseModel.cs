using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class BaseModel : AuditBase
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public bool IsPublic { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateModified { get; set; }
		public DateTime DateDeleted { get; set; }
		public string CreatedBy { get; set; }
		public string UpdatedBy { get; set; }
		public string DeletedBy { get; set; }

		public static void SetDefaultValues(BaseModel model)
		{
			var dtUtc = DateTime.UtcNow;
			var isUpdate = model.Id != Guid.Empty;
			var email = Thread.CurrentPrincipal.Identity.Name ?? "System";

			if (!isUpdate)
			{
				model.CreatedBy = email;
				model.DateCreated = dtUtc;
			}

			model.UpdatedBy = email;
			model.DateModified = dtUtc;
		}

		public static void SetDeleteValues(BaseModel model)
		{
			model.DeletedBy = Thread.CurrentPrincipal.Identity.Name ?? "System";
			model.DateDeleted = DateTime.UtcNow;
		}

		public bool IsNew => Id == Guid.Empty;
	}
}
