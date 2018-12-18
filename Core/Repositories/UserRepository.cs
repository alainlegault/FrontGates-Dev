using Shared.Database;
using Shared.Models;
using Shared.Repositories;
using Shared.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
	public interface IUserRepository : IRepository
	{
		IEnumerable<UserModel> GetAll();
		UserModel GetByEmail(string email);
		UserModel GetById(Guid id);
		bool Delete(string email);
		UserModel Save(UserModel user, bool updatePassword = false);
	}

	internal class UserRepository : BaseRepository, IUserRepository
	{
		public UserRepository(DomainContext context) : base(context)
		{

		}

		public IEnumerable<UserModel> GetAll()
		{
			return Cache.GetModelFromCache($"User-GetAll", new List<UserModel>(), (model) =>
			{
				var list = new List<UserModel>();

				Run(cmd =>
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "[dbo].[GetUsers]";

					using (var reader = cmd.ExecuteReader())
						while (reader.Read())
							list.Add(MapObject(reader));
				});

				return list;
			}, UseTypeFullNameInKey);
		}

		public UserModel GetByEmail(string email)
		{
			return Cache.GetModelFromCache($"User-GetByEmail-{email}", new UserModel(), (model) =>
			{
				var obj = new UserModel();

				Run(cmd =>
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "[dbo].[GetUserByEmail]";

					cmd.AddParameter("@email", email);

					using (var reader = cmd.ExecuteReader())
						while (reader.Read())
							obj = MapObject(reader);
				});

				return obj;
			}, UseTypeFullNameInKey);
		}

		public UserModel GetById(Guid id)
		{
			return Cache.GetModelFromCache($"User-GetById-{id}", new UserModel(), (model) =>
			{
				var obj = new UserModel();

				Run(cmd =>
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "[dbo].[GetUserById]";

					cmd.AddParameter("@id", id);

					using (var reader = cmd.ExecuteReader())
						while (reader.Read())
							obj = MapObject(reader);
				});

				return obj;
			}, UseTypeFullNameInKey);
		}

		public bool Delete(string email)
		{
			var deletedSuccessfully = false;

			Run(cmd =>
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "[dbo].[DeleteUserByEmail]";

				RepositoryHelper.PopulateDefaultDeleteParameters(cmd);

				cmd.AddParameter("@email", email);

				deletedSuccessfully = cmd.ExecuteNonQuery() > 0;
			});

			return deletedSuccessfully;
		}

		public UserModel Save(UserModel user, bool updatePassword = false)
		{
			var obj = new UserModel();
			var hashedPassword = user.Password;

			if (updatePassword)
			{
				var salt = SaltedHash.CreateSalt(user.Email);

				hashedPassword = SaltedHash.HashPassword(salt, user.Password);
			}

			Run(cmd =>
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "[dbo].[SaveUser]";

				var dtUtc = DateTime.UtcNow;

				RepositoryHelper.PopulateDefaultParametersDatesAndBy(user);

				cmd.AddParameter("@email", user.Email);
				cmd.AddParameter("@city", user.City ?? "Alexandria");
				cmd.AddParameter("@country", user.Country ?? "Canada");
				cmd.AddParameter("@culture", user.Culture ?? "en-CA");
				cmd.AddParameter("@dateModified", dtUtc);
				cmd.AddParameter("@firstName", user.FirstName ?? "");
				cmd.AddParameter("@isActive", user.IsActive);
				cmd.AddParameter("@isPublic", user.IsPublic);
				cmd.AddParameter("@isDeleted", user.IsDeleted);
				cmd.AddParameter("@lastName", user.LastName ?? "");
				cmd.AddParameter("@optIn", user.OptIn);
				cmd.AddParameter("@phoneNumber", user.PhoneNumber ?? "");
				cmd.AddParameter("@postalCode", user.PostalCode ?? "K0C1A0");
				cmd.AddParameter("@province", user.Province ?? "Ontario");
				cmd.AddParameter("@streetName", user.StreetName ?? "");
				cmd.AddParameter("@streetNumber", user.StreetNumber ?? "");
				cmd.AddParameter("@updatedBy", user.UpdatedBy);
				cmd.AddParameter("@type", (int)user.Type);

				cmd.AddParameter("@dateCreated", user.DateCreated);
				cmd.AddParameter("@id", user.Id);
				cmd.AddParameter("@password", hashedPassword);
				cmd.AddParameter("@accessLevel", user.AccessLevel);
				cmd.AddParameter("@createdBy", user.CreatedBy);

				using (var reader = cmd.ExecuteReader())
					while (reader.Read())
						obj = MapObject(reader);
			});

			return obj;
		}

		public static UserModel MapObject(IDataReader reader)
		{
			return new UserModel
			{
				Id = reader.MapGuid("Id"),
				IsActive = reader.MapBoolean("IsActive"),
				IsPublic = reader.MapBoolean("IsPublic"),
				IsDeleted = reader.MapBoolean("IsDeleted"),
				DateCreated = reader.MapDateTime("DateCreated"),
				DateModified = reader.MapDateTime("DateModified"),
				CreatedBy = reader.MapString("CreatedBy"),
				UpdatedBy = reader.MapString("UpdatedBy"),

				Email = reader.MapString("Email"),
				FirstName = reader.MapString("FirstName"),
				LastName = reader.MapString("LastName"),
				Culture = reader.MapString("Culture"),
				StreetNumber = reader.MapString("StreetNumber"),
				City = reader.MapString("City"),
				Province = reader.MapString("Province"),
				Country = reader.MapString("Country"),
				OptIn = reader.MapBoolean("OptIn"),
				PhoneNumber = reader.MapString("PhoneNumber"),
				StreetName = reader.MapString("StreetName"),
				PostalCode = reader.MapString("PostalCode"),
				Password = reader.MapString("Password"),
				AccessLevel = reader.MapInt32("AccessLevel"),
				LastLogin = reader.MapNullableDateTime("LastLogin") ?? DateTime.MinValue,
				Type = (UserType)reader.MapInt32("Type")
			};
		}
	}
}
