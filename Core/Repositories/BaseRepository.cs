using Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Attributes;
using Shared.Database;
using Shared.Extensions;
using Shared.Models;
using Shared.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
	public class BaseRepository : DbRepository
	{
		internal static int RepositoryCacheDurationInSeconds { get; set; } = (ConfigurationManager.AppSettings["RepositoryCacheDurationInSeconds"] ?? "120").To<int>();
		internal static bool EnableCache { get; set; } = ((ConfigurationManager.AppSettings["EnableCache"] ?? "false").To<bool>() && (ConfigurationManager.AppSettings["EnableCacheRepository"] ?? "false").To<bool>());
		internal static readonly Cache.MemoryCacheProvider Cache = new Cache.MemoryCacheProvider(EnableCache, RepositoryCacheDurationInSeconds);
		internal const bool UseTypeFullNameInKey = false;

		public enum EntityState
		{
			Added = 0,
			Updated = 1,
			Deleted = 2,
			Default = 3,
			Unknown = 4,
		}

		public bool DisableAudit { get; set; }

		public BaseRepository(DomainContext context, DatabaseName dbName = DatabaseName.Default)
			: base(context, dbName)
		{
			DisableAudit = false;
		}

		public IList<T> SaveChanges<T>(IDbCommand cmd, T model, IList<T> outObjects, Func<IDataReader, T> mapObject, string signatureAuthorizer, bool doAudit = false, EntityState state = EntityState.Default) where T : class, new()
		{
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					outObjects.Add(mapObject(reader));

					if (doAudit)
						DoAudit(model, state, signatureAuthorizer);
				}

			return outObjects;
		}

		public T SaveChanges<T>(IDbCommand cmd, T model, T outObject, Func<IDataReader, T> mapObject, string signatureAuthorizer, bool doAudit = false, EntityState state = EntityState.Default) where T : class, new()
		{
			using (var reader = cmd.ExecuteReader())
			{
				try
				{
					if (cmd.Connection.State == ConnectionState.Closed)
						cmd.Connection.Open();

					while (reader.Read())
					{
						outObject = mapObject(reader);

						if (doAudit)
							try
							{
								DoAudit(outObject, state, signatureAuthorizer);
							}
							catch (Exception ex)
							{
								var a = ex;
							}
					}
				}
				catch (Exception ex)
				{
					var a = ex;
				}
			}

			return outObject;
		}

		public void DoAudit<T>(T obj, EntityState state = EntityState.Default, string email = "", DatabaseName databaseName = DatabaseName.Default) where T : class, new()
		{
			try
			{
				Task.Delay(1000).ContinueWith(o => DoAuditAsync(obj, state, email, databaseName));
			}
			catch
			{
				//no need to fail this process
			}
		}

		public async Task<T> DoAuditAsync<T>(T obj, EntityState state = EntityState.Default, string email = "", DatabaseName databaseName = DatabaseName.Default) where T : class, new()
		{
			if (DisableAudit)
				return obj;

			if (email.IsNullOrEmpty())
				email = UserUtils.GetCurrentEmail();

			if (state == EntityState.Default && (obj as BaseModel) != null)
				state = ((obj as BaseModel).Id == Guid.Empty) ?
					EntityState.Added :
					EntityState.Updated;

			var utcNow = DateTime.UtcNow;
			var entityType = typeof(T).GetCustomAttributes(typeof(DbTableNameAttribute), false);
			var dbTableNameAttribute = entityType.SingleOrDefault() as DbTableNameAttribute;
			string dbTableName;

			if (dbTableNameAttribute == null)
			{
				var entityBaseType = (!typeof(T).IsAbstract) ? typeof(T) : typeof(T).BaseType;

				dbTableName = entityBaseType?.Name ?? typeof(T).Name;

				dbTableNameAttribute = new DbTableNameAttribute(dbTableName);

				dbTableName = dbTableNameAttribute.TableName;
			}
			else
				dbTableName = dbTableNameAttribute.TableName;

			await Task.Run(() => Run(databaseName, cmd =>
			{
				var eventValue = (obj as IAuditableEntity);

				if (eventValue == null)
					return;

				var audit = new AuditModel
				{
					TablePkId = eventValue.DbTablePkId(),
					ObjectId = eventValue.DbObjectId(),
					Email = email,
					DateCreated = utcNow,
					DateModified = utcNow,
					IsPublic = true,
					IsActive = true,
					IsDeleted = false,
					TableName = dbTableName
				};

				switch (state)
				{
					case EntityState.Added:
						audit.EventType = "A";
						audit.EventValue = eventValue.AddEvent();
						break;
					case EntityState.Deleted:
						audit.EventType = "D";
						audit.EventValue = eventValue.DeleteEvent();
						break;
					case EntityState.Updated:
						audit.EventType = "U";
						audit.EventValue = eventValue.UpdateEvent();
						break;
				}

				audit.Code = eventValue.DbCode();
				audit.Severity = eventValue.DbSeverity();

				cmd.CommandText = "[dbo].[SaveAudit]";

				cmd.AddParameter("@id", audit.Id);
				cmd.AddParameter("@isActive", audit.IsActive);
				cmd.AddParameter("@isPublic", audit.IsPublic);
				cmd.AddParameter("@isDeleted", audit.IsDeleted);
				cmd.AddParameter("@dateCreated", audit.DateCreated);
				cmd.AddParameter("@dateModified", audit.DateModified);
				cmd.AddParameter("@createdBy", audit.CreatedBy ?? "System");
				cmd.AddParameter("@updatedBy", audit.UpdatedBy ?? "System");
				cmd.AddParameter("@email", audit.Email);
				cmd.AddParameter("@tableName", audit.TableName);
				cmd.AddParameter("@eventType", audit.EventType);
				cmd.AddParameter("@eventValue", audit.EventValue);
				cmd.AddParameter("@tablePkId", audit.TablePkId);
				cmd.AddParameter("@objectId", audit.ObjectId);
				cmd.AddParameter("@severity", audit.Severity);
				cmd.AddParameter("@code", audit.Code);

				using (var reader = cmd.ExecuteReader())
					while (reader.Read())
					{
						var jsonString = JsonConvert.SerializeObject(audit, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });

						//_logger.BeautifyLog(string.Format("Created {0} audit : {1}{2}{1}", typeof(T).FullName, Environment.NewLine, jsonString));
					}
			}));

			return obj;
		}

		public void RemoveAudit(Guid id)
		{
			try
			{
				Task.Run(() => RemoveAuditAsync(id));
			}
			catch
			{
				//no need to fail this process
			}
		}

		public async Task<bool> RemoveAuditAsync(Guid id, DatabaseName databaseName = DatabaseName.Default)
		{
			var deletedSuccessfully = false;

			await Task.Run(() => Run(databaseName, cmd =>
			{
				cmd.CommandText = "UPDATE Audits SET IsDeleted = 1, DateDeleted = GETUTCDATE(), DeletedBy = @deletedBy WHERE TablePkId = @tablePkId";

				RepositoryHelper.PopulateDefaultDeleteParameters(cmd);

				cmd.AddParameter("@tablePkId", id);

				deletedSuccessfully = cmd.ExecuteNonQuery() > 0;
			}, CommandType.Text));

			return deletedSuccessfully;
		}

		public IEnumerable<T> GetByIds<T>(IEnumerable<Guid> ids, string dboSp, Func<IDataReader, T> mapObject) where T : class, new()
		{
			var list = new List<T>();

			if (!ids.AnyOrNotNull())
				return list;

			Run(cmd =>
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = !dboSp.Contains("[dbo]") ? $"[dbo].[Get{dboSp}ByIds]" : dboSp;

				cmd.Parameters.Add(TableValueParameter("@ids", ids.Distinct()));

				using (var reader = cmd.ExecuteReader())
					while (reader.Read())
						list.Add(mapObject(reader));
			});

			return list;
		}
	}
}
