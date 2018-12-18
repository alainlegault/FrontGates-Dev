using Shared.Repositories;
using Shared.Unity;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Shared.Configurations;
using Shared.Extensions;
using Web.Api.Resolver;

namespace Web.Api
{
	public class Bootstrapper
	{
		public static void Initialize(HttpConfiguration config)
		{
			var container = new UnityContainer();

			container.RegisterType<DomainContext>(new HttpContextLifetimeManager<DomainContext>(), new InjectionConstructor(Shared.Configurations.Configuration.DefaultConnectionString, Shared.Configurations.Configuration.ToolsConnectionString, Shared.Configurations.Configuration.FinancialsConnectionString));

			var bootstrapper = new Core.Bootstrapper();

			bootstrapper.ConfigureContainer(container);

			config.DependencyResolver = new UnityResolver(container);
		}
	}
}