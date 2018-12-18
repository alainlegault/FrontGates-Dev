using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public class Bootstrapper
	{
		public void ConfigureContainer(IUnityContainer container)
		{
			container.RegisterType<Managers.IUserManager, Managers.UserManager>();
			container.RegisterType<Repositories.IUserRepository, Repositories.UserRepository>();
		}
	}
}
