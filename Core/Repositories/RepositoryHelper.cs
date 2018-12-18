using Shared.Database;
using Shared.Extensions;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Repositories
{
	internal static class RepositoryHelper
	{
		public static DateTime SafeSqlDate(DateTime date)
		{
			var dt = DateTime.MinValue;

			if (date.IsDateEarlierThanSource(dt) || date.IsDateEqualToSource(dt))
				date = new DateTime(1755, 1, 1);

			return date;
		}

		public static void PopulateDefaultParametersDatesAndBy(BaseModel model)
		{
			var dt = new DateTime(1755, 1, 1);
			var dtUtcNow = DateTime.UtcNow;
			const string systemName = "System";

			if (model.DateCreated < dt)
				model.DateCreated = dtUtcNow;
			if (model.DateModified < dt)
				model.DateModified = dtUtcNow;
			if (model.CreatedBy.IsNullOrEmpty())
				model.CreatedBy = systemName;
			if (model.UpdatedBy.IsNullOrEmpty())
				model.UpdatedBy = systemName;
		}

		public static void PopulateDefaultDeleteParameters(IDbCommand cmd)
		{
			cmd.AddParameter("@deletedBy", Thread.CurrentPrincipal.Identity.Name ?? "System");
		}

		public static void PopulateDefaultParameters(IDbCommand cmd, BaseModel model)
		{
			var dt = new DateTime(1755, 1, 1);
			var dtUtcNow = DateTime.UtcNow;
			const string systemName = "System";

			if (model.DateCreated < dt)
				model.DateCreated = dtUtcNow;
			if (model.DateModified < dt)
				model.DateModified = dtUtcNow;
			if (model.CreatedBy.IsNullOrEmpty())
				model.CreatedBy = systemName;
			if (model.UpdatedBy.IsNullOrEmpty())
				model.UpdatedBy = systemName;

			cmd.AddParameter("@id", model.Id);
			cmd.AddParameter("@name", model.Name);
			cmd.AddParameter("@isActive", model.IsActive);
			cmd.AddParameter("@isPublic", model.IsPublic);
			cmd.AddParameter("@isDeleted", model.IsDeleted);
			cmd.AddParameter("@createdBy", model.CreatedBy);
			cmd.AddParameter("@updatedBy", model.UpdatedBy);
			cmd.AddParameter("@dateCreated", model.DateCreated);
			cmd.AddParameter("@dateModified", model.DateModified);
		}

		public static T DefaultReader<T>(IDataReader reader) where T : BaseModel, new()
		{
			return new T()
			{
				Id = reader.MapGuid("Id"),
				IsActive = reader.MapBoolean("IsActive"),
				IsPublic = reader.MapBoolean("IsPublic"),
				IsDeleted = reader.MapBoolean("IsDeleted"),
				DateCreated = reader.MapDateTime("DateCreated"),
				DateModified = reader.MapDateTime("DateModified"),
				CreatedBy = reader.MapString("CreatedBy"),
				UpdatedBy = reader.MapString("UpdatedBy"),
				Name = reader.MapString("Name"),
			};
		}
	}
}
