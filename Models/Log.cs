using FrontGatesDev.Logs.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logs.Models
{
	public class Log
	{
		public Guid ID { get; set; }
		public DateTime Date { get; set; } = DateTime.UtcNow;
		public LogLevel Level { get; set; }
		public string Message { get; set; }
		public string LevelDisplay => Level.ToString();
		public List<object> ObjectParameters { get; set; } = new List<object>();
		public int Index { get; set; }
		public int BatchNumber { get; set; }

		public Log()
		{

		}

		public Log(string message)
		{
			Message = message;
			Date = DateTime.Now;
			Level = LogLevel.Debug;
		}
	}
}
