using FrontGatesDev.Logs.Config;
using FrontGatesDev.Logs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logs.Logging.InMemory
{
	public interface IInMemoryLogger : ICustomLogger
	{
		
	}

	public sealed class InMemoryLogger : CustomLogger, IInMemoryLogger
	{
		private List<LogEntry> _logs;


		public InMemoryLogger(CustomLoggingConfig config) : base(config)
		{
			_logs = new List<LogEntry>();
		}

		public override IEnumerable<Log> GetLogs()
		{
			return FromLogEntries(_logs);
		}

		public override IEnumerable<Log> GetLogs(DateTime dt)
		{
			return FromLogEntries(_logs).Where(o => o.Date == dt);
		}

		public override void Log(Log log, params object[] args)
		{
			if (args != null)
				log.ObjectParameters.AddRange(args);

			_logs.Add(ToLogEntry(log));
		}

		public override void Log(string message, LogLevel logLevel = LogLevel.Debug, params object[] args)
		{
			_logs.Add(new LogEntry
			{
				CreationDate = DateTime.Now,
				LogLevel = logLevel.ToString(),
				LogMessage = message
			});
		}

		public override void LogDebug(string message, params object[] args)
		{
			_logs.Add(new LogEntry
			{
				CreationDate = DateTime.Now,
				LogLevel = LogLevel.Debug.ToString(),
				LogMessage = message
			});
		}

		public override void LogError(string message, params object[] args)
		{
			_logs.Add(new LogEntry
			{
				CreationDate = DateTime.Now,
				LogLevel = LogLevel.Error.ToString(),
				LogMessage = message
			});
		}

		public override void LogError(Exception ex, string message, params object[] args)
		{
			message = $"{ex.ToString()} ------------- {message}";

			_logs.Add(new LogEntry
			{
				CreationDate = DateTime.Now,
				LogLevel = LogLevel.Error.ToString(),
				LogMessage = message
			});
		}

		public override void LogInformation(string message, params object[] args)
		{
			_logs.Add(new LogEntry
			{
				CreationDate = DateTime.Now,
				LogLevel = LogLevel.Information.ToString(),
				LogMessage = message
			});
		}

		public override void LogWarning(string message, params object[] args)
		{
			_logs.Add(new LogEntry
			{
				CreationDate = DateTime.Now,
				LogLevel = LogLevel.Warning.ToString(),
				LogMessage = message
			});
		}



		private static IEnumerable<LogEntry> ToLogEntries(IEnumerable<Log> logs)
		{
			return logs.Select(o => ToLogEntry(o));
		}

		private static LogEntry ToLogEntry(Log log)
		{
			var logDate = DateTime.Now;

			if (log.Date == DateTime.MinValue)
				log.Date = logDate;

			return new LogEntry
			{
				ComponentId = string.Empty,
				CreationDate = log.Date,
				LogLevel = log.Level.ToString(),
				LogMessage = log.Message + GetArgsData(log.ObjectParameters)
			};
		}

		private static IEnumerable<Log> FromLogEntries(IEnumerable<LogEntry> logEntries)
		{
			return logEntries.Select(FromLogEntry);
		}

		private static Log FromLogEntry(LogEntry logEntry)
		{
			var logDate = DateTime.Now;

			if (logEntry.CreationDate == DateTime.MinValue)
				logEntry.CreationDate = logDate;

			return new Log
			{
				Date = logEntry.CreationDate,
				Level = (LogLevel)Enum.Parse(typeof(LogLevel), logEntry.LogLevel),
				Message = logEntry.LogMessage
			};
		}
	}
}
