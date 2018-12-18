using Newtonsoft.Json;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace Web.Api
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			var json = config.Formatters.JsonFormatter;

			json.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
			config.Formatters.Remove(config.Formatters.XmlFormatter);

			if ((ConfigurationManager.AppSettings["EnableCache"] ?? "false").To<bool>())
				GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

			//for demo purposes, leave commented out
			//if ((ConfigurationManager.AppSettings["EnableApiDomainKeyValidation"] ?? "true").To<bool>())
			//	GlobalConfiguration.Configuration.MessageHandlers.Add(new ApiKeyHandler());

			//for demo purposes, leave commented out
			//config.Filters.Add(new ApiAuthorizeAttribute());

			// Web API configuration and services
			Bootstrapper.Initialize(config);

			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{version}/{controller}/{id}",
				defaults: new { version = RouteParameter.Optional, id = RouteParameter.Optional }
			);
		}
	}
}
