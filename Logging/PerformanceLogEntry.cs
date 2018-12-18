using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logger.Logging
{
	/// <summary>
	/// Represents a performance log entry.
	/// </summary>
	[Serializable]
	public class PerformanceLogEntry
	{
		private Stopwatch _stopWatch;

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceLogEntry"/> class.
		/// </summary>
		private PerformanceLogEntry()
		{
			_stopWatch = new Stopwatch();
		}

		/// <summary>
		/// Returns the elapsed time since the current log entry has started.
		/// </summary>
		public TimeSpan ElapsedTime => _stopWatch.Elapsed;

		/// <summary>
		/// Creates a new performance log entry.
		/// </summary>
		/// <returns>A performance log entry of type <see cref="PerformanceLogEntry"/>.</returns>
		public static PerformanceLogEntry Create()
		{
			return new PerformanceLogEntry();
		}

		/// <summary>
		/// Creates and starts a new performance log entry.
		/// </summary>
		/// <returns>A performance log entry of type <see cref="PerformanceLogEntry"/>.</returns>
		public static PerformanceLogEntry CreateAndStart()
		{
			var entry = new PerformanceLogEntry();

			entry.Start();

			return entry;
		}

		/// <summary>
		/// Starts the timer of the current performance log entry.
		/// </summary>
		public void Start()
		{
			_stopWatch.Start();
		}

		/// <summary>
		/// Stops the time of the current performance log entry.
		/// </summary>
		public void Stop()
		{
			if (_stopWatch.IsRunning)
				_stopWatch.Stop();
		}
	}
}
