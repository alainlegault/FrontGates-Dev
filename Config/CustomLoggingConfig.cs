using FrontGatesDev.Logs.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logs.Config
{
	public class CustomLoggingConfig
	{
		/// <summary>
		/// Use UTC or local timezone in dates
		/// </summary>
		public bool UseUtcInDates { get; set; }

		/// <summary>
		/// File name of the logs.
		/// </summary>
		public string LogFileName { get; set; }

		//
		// Summary:
		//     Gets or sets the absolute path to the directory that contains the application
		//     content files.
		public string ContentRootPath { get; set; }

		public LogLevel DefaultLogLevel { get; set; }

		public bool EnableLogging { get; set; } = true;


		/// <summary>
		/// Sets the default minimum level to accept logging entries
		/// </summary>
		/// <param name="minimumLevel"></param>
		public void SetDefaultLogLevel(string minimumLevel)
		{
			Enum.TryParse(minimumLevel, out LogLevel defaultMinimumLogLevel);

			DefaultLogLevel = defaultMinimumLogLevel;
		}
	}
}
