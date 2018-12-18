using FrontGatesDev.Logger.Config;
using FrontGatesDev.Logger.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logger.Logging
{
	public interface ICustomLogger
	{
		void Log(Log log, params object[] args);
		void Log(string message, LogLevel logLevel = LogLevel.Debug, params object[] args);
		void LogDebug(string message, params object[] args);
		void LogInformation(string message, params object[] args);
		void LogWarning(string message, params object[] args);
		void LogError(string message, params object[] args);
		void LogError(Exception ex, string message, params object[] args);
		void LogStackTrace(StackTrace t, params object[] args);
		void LogStackTrace(string message, params object[] args);
		Log GetLog(Guid Id);
        IEnumerable<Log> GetLogs();
		IEnumerable<Log> GetLogs(DateTime dt);
		IEnumerable<Log> GetLogs(DateTime dt1, DateTime dt2);
        IEnumerable<Log> GetLogs(LogLevel? logLevel, DateTime from, DateTime to);
    }

	public class CustomLogger : ICustomLogger
	{
		private readonly ShortFileLogger _logger;
		public static readonly string LogLineSeparator = ShortFileLogger.LogLineSeparator;

		public CustomLogger(CustomLoggingConfig config)
		{
			_logger = new ShortFileLogger(config);
		}

		public virtual void Log(Log log, params object[] args)
		{
			if (args != null)
				log.ObjectParameters.AddRange(args);

			if (log.ObjectParameters.Any())
				log.Message += GetArgsData(log.ObjectParameters);

			_logger.Log(log.Message, log.Level, args);
		}

		public virtual void Log(string message, LogLevel logLevel = LogLevel.Debug, params object[] args)
		{
			_logger.Log(message, logLevel, args);
		}

		public virtual void LogDebug(string message, params object[] args)
		{
			_logger.Log(message, LogLevel.Debug, args);
		}

		public virtual void LogInformation(string message, params object[] args)
		{
			_logger.Log(message, LogLevel.Information, args);
		}

		public virtual void LogWarning(string message, params object[] args)
		{
			_logger.Log(message, LogLevel.Warning, args);
		}

		public virtual void LogError(string message, params object[] args)
		{
			_logger.Log(message, LogLevel.Error, args);
		}

		public virtual void LogError(Exception ex, string message, params object[] args)
		{
			_logger.LogError(ex, message, args);
		}

		public virtual void LogStackTrace(StackTrace t, params object[] args)
		{
			_logger.LogStackTrace(t, args);
		}

		public virtual void LogStackTrace(string message, params object[] args)
		{
			_logger.LogStackTrace(message, args);
		}

        public virtual Log GetLog(Guid id)
        {
            return _logger.GetLog(id);
        }
        
        public virtual IEnumerable<Log> GetLogs()
		{
			return _logger.GetLogs();
		}

		public virtual IEnumerable<Log> GetLogs(DateTime dt)
		{
			return _logger.GetLogs(dt);
		}

		public virtual IEnumerable<Log> GetLogs(DateTime dt1, DateTime dt2)
		{
			return _logger.GetLogs(dt1, dt2);
		}

        public virtual IEnumerable<Log> GetLogs(LogLevel? logLevel, DateTime from, DateTime to)
        {
            return _logger.GetLogs(logLevel, from, to);
        }

        /// <summary>
        /// Get and display the properties/values of the list of objects
        /// </summary>
        /// <param name="args"></param>
        /// <param name="useFields">True when requiring GetFields() instead of GetProperties() on object's Type</param>
        /// <returns></returns>
        public static string GetArgsData(IEnumerable<object> args, bool useFields = false)
		{
			if (!args.Any())
				return string.Empty;

			var sb = new StringBuilder();

			sb.AppendLine();

			var indent = 3;
			var padding = new string(' ', indent);

			foreach (var arg in args)
				if (!useFields)
					foreach (var property in GetProperties(arg.GetType()))
					{
						var value = property.GetValue(arg);

						sb.AppendLine($"{padding}Name: {property.Name}");

						if (!IsSimpleType(property.PropertyType))
							GetProperties(value, property.PropertyType, sb, indent * 2, false);
						else
							sb.AppendLine($"{padding}Value: {value}");
					}
				else
				{
					foreach (var field in GetFields(arg))
					{
						var value = field.GetValue(arg);

						sb.AppendLine($"{padding}Name: {field.Name}");

						if (!IsSimpleType(field.FieldType))
							GetProperties(value, field.FieldType, sb, indent * 2, true);
						else
							sb.AppendLine($"{padding}Value: {value}");
					}
				}

			return sb.ToString();
		}

		private static void GetProperties(object arg, Type type, StringBuilder sb, int indent = 3, bool useFields = false)
		{
			if (arg == null)
				return;

			var padding = new string(' ', indent);

			if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(IList<>)))
			{
				if (typeof(IEnumerable).IsAssignableFrom(type))
					foreach (var o in (IEnumerable)arg)
						GetProperties(o, type.GetGenericArguments()[0], sb, indent * 2, useFields);
			}
			else if (!useFields)
				foreach (var property in GetProperties(type))
				{
					var value = property.GetValue(arg);

					sb.AppendLine($"{padding}Name: {property.Name}");

					if (value == null)
						sb.AppendLine($"{padding}Value: (null)");
					else if (value.ToString() == "")
						sb.AppendLine($"{padding}Value: (empty string)");
					else if (IsSimpleType(property.PropertyType))
						sb.AppendLine($"{padding}Value: {value}");
					else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
					{
						if (property.PropertyType == typeof(string[]))
							sb.AppendLine($"{padding}Value: {string.Join(", ", (string[])value)}");
						else
							foreach (object child in (IEnumerable)value)
								GetProperties(child, child.GetType(), sb, indent * 2, false);
					}
					else
						GetProperties(value, property.PropertyType, sb, indent * 2, false);
				}
			else
			{
				foreach (var field in GetFields(arg))
				{
					var value = field.GetValue(arg);

					sb.AppendLine($"{padding}Name: {field.Name}");

					if (!IsSimpleType(field.FieldType))
						GetProperties(value, field.FieldType, sb, indent * 2, true);
					else
						sb.AppendLine($"{padding}Value: {value}");
				}
			}
		}

		private static bool IsSimpleType(Type type) => type.IsValueType || type.IsPrimitive || new Type[] { typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid) }.Contains(type) || Convert.GetTypeCode(type) != TypeCode.Object;

		private static PropertyInfo[] GetProperties(Type type) => type.GetProperties();

		private static FieldInfo[] GetFields(object arg) => arg?.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance) ?? new FieldInfo[] { };
    }
}
