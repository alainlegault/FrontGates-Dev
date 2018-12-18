using FrontGatesDev.Api.Client;
using Shared.Logging.Backend;
using Shared.Logging.Config;
using Shared.Logging.Models;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Api
{
	public class Operations
	{
		private const string EndPointUrl = "https://ourendpointurlhere";
		private const string XAuthHeaderName = "x-auth";
		private readonly IConnector connector = new Connector(EndPointUrl);
		private static Dictionary<string, object> DefaultHeaders = new Dictionary<string, object> { { XAuthHeaderName, TokenId } };
		private static string TokenId { get; set; }

		#region Uncomment if you want to log in a file
		private static readonly string _contentRootPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
		private const string _logFileName = "Logs{Date}.txt";
		public static IBackendLogger Logger = new BackendLogger(new BackendLoggingConfig { ContentRootPath = _contentRootPath, LogFileName = _logFileName });
		#endregion

		/// <summary>
		/// Send a request to be executed
		/// Used to encapsulate for logging purposes
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		/// <returns></returns>
		public async Task<T> SendRequest<T>(Func<Task<T>> func, Log log = null, [CallerMemberName]string memberName = "", [CallerFilePath]string fileName = "", [CallerLineNumber] int lineNumber = 0)
		{
			var sb = Shared.EventLogging.CreateInitialStringBuilderForSendRequest(log, memberName, fileName, lineNumber);

			T execute;

			try
			{
				//run our function
				execute = await func();

				Shared.EventLogging.PerformNormalLogging(log, sb, Logger);
				Shared.EventLogging.PerformRequestLogging(func, sb, Logger);

				//log returned results here
				Shared.EventLogging.CompleteRequest(execute, sb, Logger);
			}
			catch (Exception ex)
			{
				//peform error logging here
				Logger.LogError(ex, sb.ToString());

				//if we have no connection to the end point, throw, else return default
				if (ex?.InnerException is WebException)
				{
					var summary = $"{memberName}{Environment.NewLine}{fileName}{Environment.NewLine}{lineNumber}";

					connector.OnHttpRequestException?.Invoke(summary, ex);

					throw ex;
				}

				execute = default(T);
			}

			return execute;
		}



		#region Authentication
		/// <summary>
		/// Permet d'obtenir un token d'authentification
		/// </summary>
		/// <returns></returns>
		public async Task<AuthenticationResultModel> GetAuthenticationToken()
		{
			var args = new AuthenticationModel
			{
				Login = "loginvalue",
				Password = "passwordvalue",
				UserLanguageCode = "user's language (optional)"
			};

			return await Task.Run(() => SendRequest(async () =>
			{
				var results = await connector.PostValueAsync<AuthenticationModel, AuthenticationResultModel>("Authenticate/Token", args);

				TokenId = results.TokenId;

				DefaultHeaders = new Dictionary<string, object> { { XAuthHeaderName, TokenId } };

				return results;
			}));
		}

		/// <summary>
		/// Demande de déconnexion du token
		/// </summary>
		/// <returns></returns>
		public async Task<bool> Disconnect()
		{
			return await Task.Run(() => SendRequest(async () =>
			{
				var disconnected = await connector.DeleteValueAsync<bool>("Authenticate/Disconnect", DefaultHeaders);

				if (disconnected)
					TokenId = string.Empty;

				return disconnected;
			}));
		}
		#endregion
	}
}
