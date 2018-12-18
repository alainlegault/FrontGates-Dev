using Shared.Extensions;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Managers
{
	public class BaseManager
	{
		public static readonly DateTime MinDate = new DateTime(1755, 1, 1);

		public static void ValidateBasicValues<T>(T model, Func<Guid, T> getById) where T : BaseModel
		{
			var newModel = default(T);

			if (model.Id == Guid.Empty || model.DateCreated >= MinDate || model.DateModified >= MinDate)
				return;

			newModel = getById(model.Id);

			model.DateCreated = newModel.DateCreated;
			model.CreatedBy = newModel.CreatedBy;
			model.DateModified = newModel.DateModified;
			model.CreatedBy = newModel.CreatedBy;
		}

		public static IList<T> Filter<T>(IList<T> list, bool includeInActive = false, bool includeNonPublic = false) where T : BaseModel
		{
			return list.Where(obj => obj != null).Where(obj => obj.IsActive || includeInActive).Where(obj => obj.IsPublic || includeNonPublic).Where(o => !o.IsDeleted).ToList();
		}

		public static IEnumerable<T> Filter<T>(IEnumerable<T> list, bool includeInActive = false, bool includeNonPublic = false) where T : BaseModel
		{
			return list.Where(obj => obj != null).Where(obj => obj.IsActive || includeInActive).Where(obj => obj.IsPublic || includeNonPublic).Where(o => !o.IsDeleted).ToList();
		}

		public static T Filter<T>(T obj, bool includeInActive = false, bool includeNonPublic = false) where T : BaseModel
		{
			if (obj == null)
				return null;

			return obj.IsDeleted ? null : !obj.IsActive && !includeInActive ? null : (obj.IsPublic || includeNonPublic ? obj : null);
		}

		public static IEnumerable<Guid> GetIds<T>(IEnumerable<T> list, bool removeEmptyGuid = true)
		{
			var results = list.GroupBy(o => o).Select(o => o.FirstOrDefault()?.ToString().AsGuid() ?? Guid.Empty);

			if (removeEmptyGuid)
				results = results.Where(o => o != Guid.Empty);

			return results;
		}
	}
}
