using FrontGatesDev.Logger.Config;
using FrontGatesDev.Logger.Extensions;
using FrontGatesDev.Logger.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logger.Logging.Backend
{
	public interface IBackendLogger : ICustomLogger
	{
		void EnableLogging(bool enable);
	}

	public sealed class BackendLogger : CustomLogger, IBackendLogger
	{
		private readonly List<Log> _logs = new List<Log>();
		private readonly BackendLoggingConfig _config;
		private static readonly object _indexLocker = new object();
		private static int _indexIndicator;
		private static readonly Random _random = new Random();
		private static int _currentBatchNumber;

		public BackendLogger(BackendLoggingConfig config) : base(config)
		{
			_config = config;

			if (_config.UseDatabase && string.IsNullOrEmpty(_config.ConnectionString))
				throw new NullReferenceException(nameof(_config.ConnectionString));
		}

		public void EnableLogging(bool enable)
		{
			_config.EnableLogging = enable;
		}

        public override Log GetLog(Guid id)
        {
            if (!_config.UseDatabase)
                return base.GetLog(id);

            return GetLogFromDatabase(id);
        }

        public override IEnumerable<Log> GetLogs()
		{
			if (!_config.UseDatabase)
				return base.GetLogs();

			return GetLogsFromDatabase(null);
		}

		public override IEnumerable<Log> GetLogs(DateTime dt)
		{
			if (!_config.UseDatabase)
				return base.GetLogs(dt);

			return GetLogsFromDatabase(null, dt);
		}

		public override IEnumerable<Log> GetLogs(DateTime dt1, DateTime dt2)
		{
			if (!_config.UseDatabase)
				return base.GetLogs(dt1, dt2);

			if (dt2.IsEarlierThanSource(dt1))
				return GetLogsFromDatabase(null, dt2, dt1);

			return GetLogsFromDatabase(null, dt1, dt2);
		}

        public override IEnumerable<Log> GetLogs(LogLevel? logLevel, DateTime from, DateTime to)
        {
			if (!_config.UseDatabase)
				return base.GetLogs(logLevel, from, to);

			return GetLogsFromDatabase(logLevel, from, to);
		}

		public override void Log(Log log, params object[] args)
		{
			if (args != null)
				log.ObjectParameters.AddRange(args);

			_logs.Add(LogInternal(log));
		}

		public override void Log(string message, LogLevel logLevel = LogLevel.Debug, params object[] args)
		{
			_logs.Add(LogInternal(new Log
			{
				Date = GetDefaultLogDate(),
				Level = logLevel,
				Message = message
			}));
		}

		public override void LogDebug(string message, params object[] args)
		{
			_logs.Add(LogInternal(new Log
			{
				Date = GetDefaultLogDate(),
				Level = LogLevel.Debug,
				Message = message
			}));
		}

		public override void LogError(string message, params object[] args)
		{
			_logs.Add(LogInternal(new Log
			{
				Date = GetDefaultLogDate(),
				Level = LogLevel.Error,
				Message = message
			}));
		}

		public override void LogError(Exception ex, string message, params object[] args)
		{
			message += Environment.NewLine;
			message += $"{ex.ToString()}";
			message += Environment.NewLine;

			_logs.Add(LogInternal(new Log
			{
				Date = GetDefaultLogDate(),
				Level = LogLevel.Error,
				Message = message
			}));
		}

		public override void LogInformation(string message, params object[] args)
		{
			_logs.Add(LogInternal(new Log
			{
				Date = GetDefaultLogDate(),
				Level = LogLevel.Information,
				Message = message
			}));
		}

		public override void LogWarning(string message, params object[] args)
		{
			_logs.Add(LogInternal(new Log
			{
				Date = GetDefaultLogDate(),
				Level = LogLevel.Warning,
				Message = message
			}));
		}

		public override void LogStackTrace(string message, params object[] args)
		{
			_logs.Add(LogInternal(new Log
			{
				Date = GetDefaultLogDate(),
				Level = LogLevel.StackTrace,
				Message = message
			}));
		}

		private Log LogInternal(Log log)
		{
			lock (_indexLocker)
			{
				_indexIndicator++;

				log.Index = _indexIndicator;

				if (log.Index == 1)
					_currentBatchNumber = GenerateBatchNumber();

				log.BatchNumber = _currentBatchNumber;

				//no database configured or availabled
				if (!_config.EnableLogging || !_config.UseDatabase || string.IsNullOrEmpty(_config.ConnectionString))
				{
					//try to write the log to a file if it's enabled
					base.Log(log);

					return log;
				}

				return SaveLog(log);
			}
		}

		private DateTime GetDefaultLogDate()
		{
			return _config.UseUtcInDates ? DateTime.UtcNow : DateTime.Now;
		}

		private int GenerateBatchNumber()
		{
			return _random.Next(10000, int.MaxValue - 1);
		}

		private Log SaveLog(Log model)
		{
			var log = new Log();

			Run(cmd =>
			{
				cmd.CommandText = "[dbo].[SaveLog]";

				cmd.AddParameter("@id", model.ID);
				cmd.AddParameter("@logLevel", model.LevelDisplay);
				cmd.AddParameter("@message", model.Message);
				cmd.AddParameter("@date", model.Date);
				cmd.AddParameter("@index", model.Index);
				cmd.AddParameter("@batchNumber", model.BatchNumber);

				using (var reader = cmd.ExecuteReader())
					while (reader.Read())
						log = MapObject(reader);
			});

			return log;
		}

        private Log GetLogFromDatabase(Guid id)
        {
            Log log = null;

            Run(cmd =>
            {
                cmd.CommandText = "SELECT * FROM [dbo].[Logging] WHERE Id = @id";
                

                cmd.AddParameter("@id", id);

                using (var reader = cmd.ExecuteReader())
                    if (reader.Read())
                        log = MapObject(reader);
            }, CommandType.Text);

            return log;
        }

        private IEnumerable<Log> GetLogsFromDatabase(LogLevel? logLevel, DateTime? dt1 = null, DateTime? dt2 = null)
		{
			var logs = new List<Log>();

			Run(cmd =>
			{
				cmd.CommandText = "[dbo].[GetLogs]";

				if (dt1.HasValue)
					cmd.AddParameter("@date1", dt1.Value.Date);

				if (dt2.HasValue)
					cmd.AddParameter("@date2", dt2.Value.Date.AddDays(1).AddSeconds(-1));
				else if (dt1.HasValue)
					cmd.AddParameter("@date2", dt1.Value.Date.AddDays(1).AddSeconds(-1));

                if(logLevel.HasValue)
                    cmd.AddParameter("@logLevel", logLevel.Value.ToString());

                using (var reader = cmd.ExecuteReader())
					while (reader.Read())
						logs.Add(MapObject(reader));
			});

			return logs;
		}

		private void Run(Action<SqlCommand> fn, CommandType commandType = CommandType.StoredProcedure)
		{
			if (!_config.UseDatabase)
				return;

			var connectionString = _config.ConnectionString;

			if (string.IsNullOrEmpty(connectionString))
				return;

			try
			{
				var builder = new SqlConnectionStringBuilder(connectionString);

				using (var connection = new SqlConnection(builder.ConnectionString))
				{
					connection.Open();

					if (connection.State != ConnectionState.Open)
						connection.Open();

					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandType = commandType;

						fn(cmd);
					}
				}
			}
			catch (FormatException)
			{
				throw new Exception($"The format for the connection string '{connectionString}' is invalid.");
			}
			catch (Exception ex)
			{
				throw new Exception(ex.ToString());
			}
		}

		private static Log MapObject(IDataReader reader)
		{
			return new Log
			{
				ID = reader.MapGuid("Id"),
				Level = (LogLevel)Enum.Parse(typeof(LogLevel), reader.MapString("LogLevel")),
				Message = reader.MapString("Message"),
				Date = reader.MapDateTime("Date"),
				BatchNumber = reader.MapInt32("BatchNumber")
			};
		}
	}
}
