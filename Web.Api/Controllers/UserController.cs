using Core.Managers;
using Shared.Extensions;
using Shared.Models;
using Shared.Utils;
using System.Threading.Tasks;
using System.Web.Http;

namespace Web.Api.Controllers
{
	public class UserController : BaseController
	{
		private readonly IUserManager _userManager;

		public UserController(IUserManager userManager)
		{
			_userManager = userManager;
		}

		[Route("api/v1/user")]
		[HttpGet]
		public async Task<object> Get()
		{
			return await _userManager.GetByEmailAsync(CurrentAuthorizer);
		}

		[Route("api/v1/user/{email}")]
		[HttpGet]
		public async Task<object> Get(string email)
		{
			return await _userManager.GetByEmailAsync(email.DecodeBase64());
		}

		[Route("api/v1/user")]
		[HttpPut]
		public async Task<object> Put(UserModel model)
		{
			//do what we have to do
			//update "model" properties accordingly

			//save
			return await _userManager.SaveAsync(model);
		}

		[Route("api/v1/user")]
		[HttpPost]
		public async Task<object> Post([FromBody]UserModel model)
		{
			//do what we have to do

			//save object
			return await _userManager.SaveAsync(model);
		}

		[Route("api/v1/user")]
		[HttpDelete]
		public async Task<object> Delete(string email)
		{
			return await _userManager.DeleteAsync(email);
		}
	}
}