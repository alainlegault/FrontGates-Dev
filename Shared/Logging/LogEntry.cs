using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Logging
{
	public class LogEntry
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LogEntry"/> class.
		/// </summary>
		public LogEntry()
		{
			CreationDate = new DateTime();
		}

		/// <summary>
		/// Gets or sets the log entry creation date and time.
		/// </summary>
		public DateTime CreationDate { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="LogLevel"/> in string form.
		/// </summary>
		public string LogLevel { get; set; }

		/// <summary>
		/// Gets or sets the id of the component from which the log entry originated.
		/// </summary>
		public string ComponentId { get; set; }
		/// <summary>
		/// Gets or sets the log message;
		/// </summary>
		public string LogMessage { get; set; }

		public static IEnumerable<LogEntry> FromSerilogLoggerLine(IEnumerable<string> lines)
		{
			var list = new List<LogEntry>();
			var sb = new StringBuilder();
			var count = 0;
			LogEntry entry = null;

			while (true)
			{
				if (count >= lines.Count())
					break;

				var line = lines.ElementAt(count);
				var isStart = IsLogEntryLineStart(line);

				if (isStart == false)
				{
					sb.AppendLine(line);

					count++;

					continue;
				}

				if (entry != null)
				{
					entry.LogMessage = sb.ToString();

					list.Add(entry);
				}

				// next log
				sb = new StringBuilder();
				entry = new LogEntry();

				var parts = line.Split(' ');

				if (DateTime.TryParse(parts[0], out DateTime logDate))
					entry.CreationDate = logDate;

				var componentId = parts[1];

				if (componentId.Trim().Length > 0)
					entry.ComponentId = componentId;

				var level = parts[2];

				entry.LogLevel = GetLevel(level);

				var startMsg = line.IndexOf(level) + level.Length;
				var msg = line.Substring(startMsg).Trim();

				sb.AppendLine(msg);

				count++;
			}

			return list.OrderByDescending(n => n.CreationDate);
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

		private static string GetLevel(string level)
		{
			if (level == "[DBG]")
				return "Debug";
			else if (level == "[INF]")
				return "Information";
			else if (level == "[WRN]")
				return "Warning";
			else if (level == "[ERR]")
				return "Error";
			else if (level == "[CRI]")
				return "Critical";

			return "None";
		}
	}
}
