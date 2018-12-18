using Shared.Logging;
using Shared.Logging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Api.Shared
{
	public static class EventLogging
	{
		/// <summary>
		/// Create an initial instance of a StringBuilder to use in requests for logging purposes
		/// </summary>
		/// <param name="log"></param>
		/// <param name="memberName"></param>
		/// <param name="fileName"></param>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public static StringBuilder CreateInitialStringBuilderForSendRequest(Log log, string memberName = "", string fileName = "", int lineNumber = 0)
		{
			var sb = new StringBuilder();

			sb.AppendLine();

			if (!string.IsNullOrEmpty(log?.Message))
				sb.AppendLine($"Title: {log.Message}");

			if (!string.IsNullOrEmpty(memberName))
				sb.AppendLine($"Method: {memberName}");

			if (!string.IsNullOrEmpty(fileName))
				sb.AppendLine($"File: {fileName}");

			if (lineNumber > 0)
				sb.AppendLine($"Line number: {lineNumber}");

			return sb;
		}

		/// <summary>
		/// Create a log entry with the data in the StringBuilder instance
		/// </summary>
		/// <param name="log"></param>
		/// <param name="sb"></param>
		public static void PerformNormalLogging(Log log, StringBuilder sb, ICustomLogger logger)
		{
			if (log == null)
				logger.Log(new Log(sb.ToString()));
			else
			{
				log.Message = sb.ToString();

				logger.Log(log);
			}
		}

		/// <summary>
		/// Logs the object being sent in the request
		/// </summary>
		/// <typeparam name="TRequest"></typeparam>
		/// <param name="func"></param>
		/// <param name="sb"></param>
		public static void PerformRequestLogging<TRequest>(Func<TRequest> func, StringBuilder sb, ICustomLogger logger) where TRequest : class
		{
			var target = func.Target;

			if (target != null)
			{
				sb.AppendLine($"Requested value of type {typeof(TRequest).FullName}: ");

				foreach (var field in target.GetType().GetFields())
				{
					sb.AppendLine($"Name: {field.Name}");
					sb.AppendLine($"Value: {CustomLogger.GetArgsData(new object[] { field.GetValue(target) }, true)}");
				}

				logger.Log(sb.ToString());
			}
		}

		/// <summary>
		/// Complete logging a request
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public static void CompleteRequest<T>(T obj, StringBuilder sb, ICustomLogger logger)
		{
			sb.AppendLine();

			//get return type data to put in the log
			sb.AppendLine($"Returned value of type {typeof(T).FullName}: {CustomLogger.GetArgsData(new object[] { obj })}");

			//perform normal logging here
			logger.Log(sb.ToString());
		}
	}
}
