using Newtonsoft.Json.Linq;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Configurations
{
	public enum ConnectionStringEnvironmentType
	{
		Dev = 0,
		Test = 1,
		Staging = 2,
		Demo = 3,
		Prod = 4
	}

	public class Configuration
	{
		public const string LocalShoppingMallAccessTokenHeaderName = "X-LSM-AccessToken";
		public const string LocalShoppingMallApiDomainKeyHeaderName = "X-LSM-DomainApiKey";

		public static JObject SystemConfig(string file) => File.Exists(file) ? JObject.Parse(File.ReadAllText(file)) : null;

		public static string Env => (ConfigurationManager.AppSettings["Env"] ?? "DEV").To<string>();

		public static string SupportEmailAddress => (ConfigurationManager.AppSettings["SupportEmailAddress"] ?? "insert default email address here").To<string>();
		public static string SupportEmailAddressPassword => (ConfigurationManager.AppSettings["SupportEmailPassword"] ?? "insert default password here").To<string>().DecodeBase64().DecodeBase64();
		public static string SmtpServer => (ConfigurationManager.AppSettings["SmtpServer"] ?? "insert default smtp server here").To<string>();
		public static int SmtpServerPort => (ConfigurationManager.AppSettings["SmtpServerPort"] ?? "insert default smtp port here").To<int>();
		public static string SmtpServerUserName => (ConfigurationManager.AppSettings["SmtpServerUserName"] ?? SupportEmailAddress).To<string>();
		public static string SmtpServerPassword => (ConfigurationManager.AppSettings["SmtpServerPassword"] ?? SupportEmailAddressPassword).To<string>().DecodeBase64().DecodeBase64();
		public static bool SmtpServerEnableSsl => (ConfigurationManager.AppSettings["SmtpServerEnableSsl"] ?? "false").To<bool>();
		public static bool SmtpServerIsBodyHtml => (ConfigurationManager.AppSettings["SmtpServerIsBodyHtml"] ?? "true").To<bool>();

		public static bool IsRewardsPointEnabled = (ConfigurationManager.AppSettings["IsRewardsPointEnabled"] ?? "false").To<bool>();

		public static string DefaultConnectionString
		{
			get
			{
				var defaultEnvironment = (ConfigurationManager.AppSettings["Env"] ?? "DEV").To<string>();
				ConnectionStringEnvironmentType connectionStringType;

				switch (defaultEnvironment.ToLower())
				{
					default:
						connectionStringType = ConnectionStringEnvironmentType.Dev;
						break;
					case "test":
						connectionStringType = ConnectionStringEnvironmentType.Test;
						break;
					case "staging":
						connectionStringType = ConnectionStringEnvironmentType.Staging;
						break;
					case "demo":
						connectionStringType = ConnectionStringEnvironmentType.Demo;
						break;
					case "prod":
						connectionStringType = ConnectionStringEnvironmentType.Prod;
						break;
				}

				return GetDefaultConnectionString(connectionStringType);
			}
		}

		internal static string GetDefaultConnectionString(ConnectionStringEnvironmentType type)
		{
			var conn = "";

			switch (type)
			{
				case ConnectionStringEnvironmentType.Dev:
					conn = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\workspaces\VS2017\ECommerce\Database\ECommerce.mdf;Integrated Security=True";
					break;
				case ConnectionStringEnvironmentType.Test:
					conn = @"insert testing database connection string here";
					break;
				case ConnectionStringEnvironmentType.Staging:
					conn = @"insert staging database connection string here";
					break;
				case ConnectionStringEnvironmentType.Demo:
					conn = @"insert demo database connection string here";
					break;
				case ConnectionStringEnvironmentType.Prod:
					conn = @"insert production database connection string here";
					break;
			}

			return conn;
		}

		public static string ToolsConnectionString
		{
			get
			{
				var defaultEnvironment = (ConfigurationManager.AppSettings["Env"] ?? "DEV").To<string>();
				ConnectionStringEnvironmentType connectionStringType;

				switch (defaultEnvironment.ToLower())
				{
					default:
						connectionStringType = ConnectionStringEnvironmentType.Dev;
						break;
					case "test":
						connectionStringType = ConnectionStringEnvironmentType.Test;
						break;
					case "staging":
						connectionStringType = ConnectionStringEnvironmentType.Staging;
						break;
					case "demo":
						connectionStringType = ConnectionStringEnvironmentType.Demo;
						break;
					case "prod":
						connectionStringType = ConnectionStringEnvironmentType.Prod;
						break;
				}

				return GetToolsConnectionString(connectionStringType);
			}
		}

		internal static string GetToolsConnectionString(ConnectionStringEnvironmentType type)
		{
			var conn = "";

			switch (type)
			{
				case ConnectionStringEnvironmentType.Dev:
					conn = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\workspaces\VS2017\ECommerce\Database\ECommerceTools.mdf;Integrated Security=True";
					break;
				case ConnectionStringEnvironmentType.Test:
					conn = @"insert staging database connection string here";
					break;
				case ConnectionStringEnvironmentType.Staging:
					conn = @"insert staging database connection string here";
					break;
				case ConnectionStringEnvironmentType.Demo:
					conn = @"insert staging database connection string here";
					break;
				case ConnectionStringEnvironmentType.Prod:
					conn = @"insert staging database connection string here";
					break;
			}

			return conn;
		}

		public static string FinancialsConnectionString
		{
			get
			{
				var defaultEnvironment = (ConfigurationManager.AppSettings["Env"] ?? "DEV").To<string>();
				ConnectionStringEnvironmentType connectionStringType;

				switch (defaultEnvironment.ToLower())
				{
					default:
						connectionStringType = ConnectionStringEnvironmentType.Dev;
						break;
					case "test":
						connectionStringType = ConnectionStringEnvironmentType.Test;
						break;
					case "staging":
						connectionStringType = ConnectionStringEnvironmentType.Staging;
						break;
					case "demo":
						connectionStringType = ConnectionStringEnvironmentType.Demo;
						break;
					case "prod":
						connectionStringType = ConnectionStringEnvironmentType.Prod;
						break;
				}

				return GetFinancialsConnectionString(connectionStringType);
			}
		}

		internal static string GetFinancialsConnectionString(ConnectionStringEnvironmentType type)
		{
			var conn = "";

			switch (type)
			{
				case ConnectionStringEnvironmentType.Dev:
					conn = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\workspaces\VS2017\ECommerce\Database\ECommerceFinancials.mdf;Integrated Security=True";
					break;
				case ConnectionStringEnvironmentType.Test:
					conn = @"insert staging database connection string here";
					break;
				case ConnectionStringEnvironmentType.Staging:
					conn = @"insert staging database connection string here";
					break;
				case ConnectionStringEnvironmentType.Demo:
					conn = @"insert staging database connection string here";
					break;
				case ConnectionStringEnvironmentType.Prod:
					conn = @"insert staging database connection string here";
					break;
			}

			return conn;
		}
	}
}
