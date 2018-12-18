using FrontGatesDev.Logger.Config;
using FrontGatesDev.Logger.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrontGatesDev.Logger.Logging
{
	/// <summary>
	/// Implementation of the <see cref="ShortFileLogger"/> contract
	/// that logs short log messages for easier human reading.
	/// </summary>
	public class ShortFileLogger
	{
		private static ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim();
		private readonly CustomLoggingConfig _config;
		private LogLevel _minLevel;
		private readonly string _loggingPath;
		private const string _dateFormat = "yyyyMMdd";
		private const int levelPaddingNumber = 25;
		private readonly bool _useUtc;
		public static readonly string LogLineSeparator = "------------------------------";

		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryLogger"/> class.
		/// </summary>
		/// <param name="minLevel">The minimum log level that will be written.</param>
		public ShortFileLogger(CustomLoggingConfig config)
		{
			_config = config;
			_minLevel = config.DefaultLogLevel;
			_loggingPath = config.ContentRootPath;
			_useUtc = config.UseUtcInDates;
		}

		/// <summary>
		/// Summary:
		///     Checks if the given logLevel is enabled.
		///
		/// Parameters:
		///   logLevel:
		///     level to be checked.
		///
		/// Returns:
		///     true if enabled.
		/// </summary>
		/// <param name="logLevel"></param>
		/// <returns></returns>
		public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

		/// <summary>
		/// Summary:
		///     Writes a log entry.
		///
		/// Parameters:
		///   logLevel:
		///     Entry will be written on this level.
		///
		///   eventId:
		///     Id of the event.
		///
		///   state:
		///     The entry to be written. Can be also an object.
		///
		///   exception:
		///     The exception related to this entry.
		///
		///   formatter:
		///     Function to create a string message of the state and exception.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="logLevel"></param>
		/// <param name="args"></param>
		public void Log(string message, LogLevel logLevel, params object[] args)
		{
			var enabled = IsEnabled(logLevel);

			if (!enabled || string.IsNullOrEmpty(message))
				return;

			switch (logLevel)
			{
				default:
				case LogLevel.Debug:
					LogDebug(message, args);

					break;
				case LogLevel.Error:
					LogError(message, args);

					break;
				case LogLevel.Fatal:
					LogFatal(message, args);

					break;
				case LogLevel.Information:
					LogInformation(message, args);

					break;
				case LogLevel.Performance:

					break;
				case LogLevel.Warning:
					LogWarning(message, args);

					break;
				case LogLevel.StackTrace:
					LogStackTrace(message, args);

					break;
			}
		}

		/// <summary>
		/// Logs a formatted debug message.
		/// </summary>
		/// <param name="format">The format of the message to log.</param>
		/// <param name="args">The arguments to the formatted message.</param>
		public void LogDebug(string format, params object[] args)
		{
			var level = LogLevel.Debug;

			if (level < _minLevel)
				return;

			var message = format;

			if (args != null && args.Length > 0)
				message = string.Format(format, args);

			LogInternal(message, level);
		}

		/// <summary>
		/// Logs a formatted informational message.
		/// </summary>
		/// <param name="format">The format of the message to log.</param>
		/// <param name="args">The arguments to the formatted message.</param>
		public void LogInformation(string format, params object[] args)
		{
			var level = LogLevel.Information;

			if (level < _minLevel)
				return;

			var message = format;

			if (args != null && args.Length > 0)
				message = string.Format(format, args);

			LogInternal(message, level);
		}

		/// <summary>
		/// Logs a formatted warning message.
		/// </summary>
		/// <param name="format">The format of the message to log.</param>
		/// <param name="args">The arguments to the formatted message.</param>
		public void LogWarning(string format, params object[] args)
		{
			var level = LogLevel.Warning;

			if (level < _minLevel)
				return;

			var message = format;

			if (args != null && args.Length > 0)
				message = string.Format(format, args);

			LogInternal(message, level);
		}

		/// <summary>
		/// Logs a formatted error message.
		/// </summary>
		/// <param name="format">The format of the message to log.</param>
		/// <param name="args">The arguments to the formatted message.</param>
		public void LogError(string format, params object[] args)
		{
			var level = LogLevel.Error;

			if (level < _minLevel)
				return;

			var message = format;

			if (args != null && args.Length > 0)
				message = string.Format(format, args);

			LogInternal(message, level);

		}

		/// <summary>
		/// Logs a formatted error message in addition to the exception that was thrown.
		/// </summary>
		/// <param name="ex">An exception that was thrown and that will be included in the message.</param>
		/// <param name="format">The format of the message to log.</param>
		/// <param name="args">The arguments to the formatted message.</param>
		public void LogError(Exception ex, string format, params object[] args)
		{
			var level = LogLevel.Error;

			if (level < _minLevel)
				return;

			var message = format;

			if (args != null && args.Length > 0)
				message = string.Format(format, args);

			if (ex != null)
				message += string.Format("\t{0}", LogUtilities.BuildExceptionMessage(ex));

			LogInternal(message, level);

			var recursion = 0;
			var innerException = ex.InnerException;

			while (innerException != null && recursion < 10)
			{
				recursion++;

				LogInternal(LogUtilities.BuildExceptionMessage(ex), level);

				innerException = innerException.InnerException;
			}
		}

		/// <summary>
		/// Logs a formatted fatal message.
		/// </summary>
		/// <param name="format">The format of the message to log.</param>
		/// <param name="args">The arguments to the formatted message.</param>
		public void LogFatal(string format, params object[] args)
		{
			var level = LogLevel.Fatal;

			if (level < _minLevel)
				return;

			var message = format;

			if (args != null && args.Length > 0)
				message = string.Format(format, args);

			LogInternal(message, level);
		}

		/// <summary>
		/// Logs a performance message.
		/// </summary>
		/// <param name="message">A message to log in addition to the performance portion.</param>
		/// <param name="logEntry">An object representing the performance log entry.</param>
		public void LogPerformance(string message, PerformanceLogEntry logEntry)
		{
			logEntry.Stop();

			if (logEntry == null)
				return;

			var msg = $"{message}: {logEntry.ElapsedTime.TotalSeconds} seconds";

			LogInternal(msg, LogLevel.Performance);
		}

		public void LogStackTrace(StackTrace t, params object[] args)
		{
			var level = LogLevel.StackTrace;

			if (level < _minLevel)
				return;

			LogInternal(t.ToString(), level);
		}

		public void LogStackTrace(string message, params object[] args)
		{
			var level = LogLevel.StackTrace;

			if (level < _minLevel)
				return;

			LogInternal(message, level);
		}

        public Log GetLog(Guid id)
        {
            return null;
        }

        /// <summary>
        /// Get logs
        /// Intented to be overriden before this layer (i.e. : CustomLogger)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Log> GetLogs()
		{
			var list = Enumerable.Empty<Log>();

			if (string.IsNullOrEmpty(_config.LogFileName))
			{
				LogWarning($"The application log file does not exist on the server.");

				return list;
			}

			var pathFormat = _config.LogFileName.Replace("{Date}", DateTime.Now.ToString(_dateFormat));
			var fn = Path.Combine(_config.ContentRootPath, pathFormat);

			if (!File.Exists(fn))
			{
				LogWarning($"The application log file {fn} does not exist on the server.");

				return list;
			}

			var lines = new List<string>();

			using (var fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(fs))
				while (!sr.EndOfStream)
					lines.Add(sr.ReadLine());

			list = FromLogFileLine(lines);

			return list;
		}

		/// <summary>
		/// Get logs by DateTime value
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public IEnumerable<Log> GetLogs(DateTime dt)
		{
			var list = Enumerable.Empty<Log>();

			if (string.IsNullOrEmpty(_config.LogFileName))
			{
				LogWarning($"The application log file does not exist on the server.");

				return list;
			}

			var pathFormat = _config.LogFileName.Replace("{Date}", dt.ToString(_dateFormat));
			var fn = Path.Combine(_config.ContentRootPath, pathFormat);

			if (!File.Exists(fn))
			{
				LogWarning($"The application log file {fn} does not exist on the server.");

				return list;
			}

			var lines = new List<string>();

			using (var fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(fs))
				while (!sr.EndOfStream)
					lines.Add(sr.ReadLine());

			list = FromLogFileLine(lines);

			return list;
		}

		/// <summary>
		/// Get logs by date range
		/// Intented to be overriden before this layer (i.e. : CustomLogger)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Log> GetLogs(DateTime dt1, DateTime dt2)
		{
			var list = new List<Log>();

			var pathFormat1 = _config.LogFileName.Replace("{Date}", dt1.ToString(_dateFormat));
			var fn1 = Path.Combine(_config.ContentRootPath, pathFormat1);

			if (!File.Exists(fn1))
			{
				LogWarning($"The application log file {fn1} does not exist on the server.");

				return list;
			}

			var pathFormat2 = _config.LogFileName.Replace("{Date}", dt2.ToString(_dateFormat));
			var fn2 = Path.Combine(_config.ContentRootPath, pathFormat2);

			if (!File.Exists(fn2))
			{
				LogWarning($"The application log file {fn2} does not exist on the server.");

				return list;
			}

			var lines = new List<string>();

			using (var fs = new FileStream(fn1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(fs))
				while (!sr.EndOfStream)
					lines.Add(sr.ReadLine());

			list = FromLogFileLine(lines).ToList();

			using (var fs = new FileStream(fn2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(fs))
				while (!sr.EndOfStream)
					lines.Add(sr.ReadLine());

			list.AddRange(FromLogFileLine(lines));

			return list;
		}

        public IEnumerable<Log> GetLogs(LogLevel? logLevel, DateTime from, DateTime to)
        {
			return Enumerable.Empty<Log>();
		}


		private void LogInternal(string message, LogLevel level)
		{
			if (string.IsNullOrEmpty(_loggingPath))
				return;

			if (!Directory.Exists(_loggingPath))
				Directory.CreateDirectory(_loggingPath);

			var dt = _useUtc ? DateTime.UtcNow : DateTime.Now;
			var fileName = _config.LogFileName.Replace("{Date}", dt.ToString(_dateFormat));
			var path = Path.Combine(_loggingPath, fileName);

			_fileLock.EnterWriteLock();

			try
			{
				var user = Thread.CurrentPrincipal?.Identity?.Name ?? "(Undefined)";
				var maxLength = 15;

				if (user.Length > maxLength)
					user = user.Substring(0, maxLength);
				else if (user.Length < maxLength)
					user = user.PadRight(maxLength);

				var msg = $"{dt.ToString("yyyy-MM-dd HH:mm:ss.fff")}  {GetLevelPadded(level.ToString()).ToUpper()}  {user}  {message}{Environment.NewLine}{LogLineSeparator}";

				using (var w = new StreamWriter(path, true))
					w.WriteLine(msg);
			}
			finally
			{
				_fileLock.ExitWriteLock();
			}
		}

		private static IEnumerable<Log> FromLogFileLine(IEnumerable<string> lines)
		{
			Log entry = null;
			var list = new List<Log>();
			var sb = new StringBuilder();
			var count = 0;

			while (true)
			{
				if (entry != null)
				{
					entry.Message = sb.ToString();

					list.Add(entry);
				}

				if (count >= lines.Count())
					break;

				var line = lines.ElementAt(count);

				if (string.IsNullOrEmpty(line))
				{
					count++;

					continue;
				}

				if (!IsLogEntryLineStart(line))
				{
					count++;

					continue;
				}

				var parts = line.Split(' ');

				if (parts.Length <= 2)
				{
					count++;

					continue;
				}

				// next log
				sb = new StringBuilder();
				entry = new Log();

				if (DateTime.TryParse(parts[0], out DateTime logDate))
					entry.Date = logDate;

				TimeSpan.TryParse(parts[1], out TimeSpan ts);

				entry.Date = entry.Date.Add(ts);

				var level = parts[3];

				entry.Level = GetLogLevel(level);

				var startMsg = line.IndexOf(level) + level.Length;
				var msg = line.Substring(startMsg).Trim();

				sb.AppendLine(msg);

				count++;
			}

			return list.OrderByDescending(n => n.Date);
		}

		private static bool IsLogEntryLineStart(string line)
		{
			if (string.IsNullOrEmpty(line))
				return false;

			var parts = line.Split(' ');

			if (parts.Length == 0)
				return false;

			if (DateTime.TryParse(parts[0], out DateTime logDate))
				return true;

			return false;
		}

		private static LogLevel GetLogLevel(string level)
		{
			foreach (var lvl in Enum.GetNames(typeof(LogLevel)))
				if (GetLevelPadded(lvl.ToUpper()) == level)
					return (LogLevel)Enum.Parse(typeof(LogLevel), lvl);

			return LogLevel.Debug;
		}

		private static string GetLevelPadded(string level)
		{
			if (!string.IsNullOrEmpty(level) && level.Length >= levelPaddingNumber)
				return level.Substring(0, levelPaddingNumber);

			return level;
		}
	}
}
