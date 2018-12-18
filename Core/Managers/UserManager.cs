using Core.Repositories;
using Shared.Extensions;
using Shared.Models;
using Shared.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Managers
{
	public interface IUserManager
	{
		IEnumerable<UserModel> GetAll(bool includeInActive = false, bool includeNonPublic = false);
		UserModel GetByEmail(string email, bool includeInActive = false, bool includeNonPublic = false);
		UserModel GetById(Guid id, bool includeInActive = false, bool includeNonPublic = false);
		bool Delete(string email);
		UserModel Save(UserModel model, bool updatePassword = false);


		Task<IEnumerable<UserModel>> GetAllAsync(bool includeInActive = false, bool includeNonPublic = false);
		Task<UserModel> GetByEmailAsync(string email, bool includeInActive = false, bool includeNonPublic = false);
		Task<UserModel> GetByIdAsync(Guid id, bool includeInActive = false, bool includeNonPublic = false);
		Task<bool> DeleteAsync(string email);
		Task<UserModel> SaveAsync(UserModel model, bool updatePassword = false);
	}

	public class UserManager : BaseManager, IUserManager
	{
		private readonly DomainContext _context;
		private readonly IUserRepository _repository;

		public UserManager(DomainContext context, IUserRepository repository)
		{
			_context = context;
			_repository = repository;
		}

		private static readonly bool EnableApplicationCache = ConfigurationManager.AppSettings["EnableApplicationCache"].To<bool>();
		private static readonly int ApplicationCacheTimeoutInSeconds = ConfigurationManager.AppSettings["ApplicationCacheTimeoutInSeconds"].To<int>();

		public UserModel Populate(UserModel model, bool includeInActive = false, bool includeNonPublic = false)
		{
			//if we had references to other objects, we would include them here
			//ex : model.ExtendedObject = _extendedObjectRepository.GetById(model.ExtendedObjectId)

			return model;
		}

		public IEnumerable<UserModel> GetAll(bool includeInActive = false, bool includeNonPublic = false)
		{
			using (var uow = new DbUnitOfWork(_context))
			{
				var repositories = new IRepository[] { _repository };

				uow.Enlist(repositories);
				uow.InitTransaction();

				return Filter(_repository.GetAll().Select(o => Populate(o, includeInActive, includeNonPublic)), includeInActive, includeNonPublic);
			}
		}

		public UserModel GetByEmail(string email, bool includeInActive = false, bool includeNonPublic = false)
		{
			using (var uow = new DbUnitOfWork(_context))
			{
				var repositories = new IRepository[] { _repository };

				uow.Enlist(repositories);
				uow.InitTransaction();

				return Filter(Populate(_repository.GetByEmail(email), includeInActive, includeNonPublic), includeInActive, includeNonPublic);
			}
		}

		public UserModel GetById(Guid id, bool includeInActive = false, bool includeNonPublic = false)
		{
			using (var uow = new DbUnitOfWork(_context))
			{
				var repositories = new IRepository[] { _repository };

				uow.Enlist(repositories);
				uow.InitTransaction();

				return Filter(Populate(_repository.GetById(id), includeInActive, includeNonPublic), includeInActive, includeNonPublic);
			}
		}

		public bool Delete(string email)
		{
			return _repository.Delete(email);
		}

		public UserModel Save(UserModel model, bool updatePassword = false)
		{
			ValidateBasicValues(model, _repository.GetById);
			BaseModel.SetDefaultValues(model);

			//run all business rules here

			//save our object
			var obj = _repository.Save(model, updatePassword);

			//return our updated object
			return GetByEmail(obj.Email, true, true);
		}


		public async Task<IEnumerable<UserModel>> GetAllAsync(bool includeInActive = false, bool includeNonPublic = false)
		{
			return await Task.Run(() => GetAll(includeInActive, includeNonPublic));
		}

		public async Task<UserModel> GetByEmailAsync(string email, bool includeInActive = false, bool includeNonPublic = false)
		{
			return await Task.Run(() => GetByEmail(email, includeInActive, includeNonPublic));
		}

		public async Task<UserModel> GetByIdAsync(Guid id, bool includeInActive = false, bool includeNonPublic = false)
		{
			return await Task.Run(() => GetById(id, includeInActive, includeNonPublic));
		}

		public async Task<bool> DeleteAsync(string email)
		{
			return await Task.Run(() => Delete(email));
		}

		public async Task<UserModel> SaveAsync(UserModel model, bool updatePassword = false)
		{
			return await Task.Run(() => Save(model, updatePassword));
		}
	}
}
