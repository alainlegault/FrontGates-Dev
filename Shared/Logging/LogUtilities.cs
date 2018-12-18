using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Logging
{
	/// <summary>
	/// Logging utilities class.
	/// </summary>
	internal class LogUtilities
	{
		/// <summary>
		/// Builds an detailed and formatted exception message from an <see cref="Exception"/> object.
		/// </summary>
		/// <param name="ex">The exception to process.</param>
		/// <returns>A string representing a detailed and formatted exception message.</returns>
		public static string BuildExceptionMessage(Exception ex)
		{
			var rv = string.Empty;

			rv = BuildExceptionMessageRecursive(ex, rv);

			return rv;
		}

		/// <summary>
		/// Builds an detailed and formatted exception message from an <see cref="Exception"/> object
		/// and a custom string.
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="msg">A custom user message</param>
		/// <returns>A string representing a detailed and formatted exception message.</returns>
		private static string BuildExceptionMessageRecursive(Exception exception, string msg)
		{
			if (exception == null)
				return msg;

			if (exception is AggregateException aex)
				foreach (var inner in aex.InnerExceptions)
					msg = BuildExceptionMessageRecursive(inner, msg);
			else if (exception.InnerException != null)
				msg = BuildExceptionMessageRecursive(exception.InnerException, msg);

			var format = "   {0}: {1}";

			msg += $"  [EXCEPTION]  {exception.Message}";

			if (!string.IsNullOrEmpty(exception.Source))
				msg += string.Format(format, "Source", exception.Source);

			if (!string.IsNullOrEmpty(exception.StackTrace))
				msg += string.Format(format, "Stack Trace", exception.StackTrace.Replace("\r\n", string.Empty));

			if (exception.TargetSite != null)
				msg += string.Format(format, "TargetSite", exception.TargetSite);

			return msg;
		}
	}
}
