using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logs.Logging
{
	/// <summary>
	/// A set of values indicating the log level of an event.
	/// </summary>
	public enum LogLevel
	{
		/// <summary>
		/// A debug message.
		/// </summary>
		Debug = 0,
		/// <summary>
		/// An information message.
		/// </summary>
		Information = 1,
		/// <summary>
		/// A warning message
		/// </summary>
		Warning = 2,
		/// <summary>
		/// A recoverable error.
		/// </summary>
		Error = 3,
		/// <summary>
		/// A fatal error or an application crash.
		/// </summary>
		Fatal = 4,
		/// <summary>
		/// A performance log message
		/// </summary>
		Performance = 5,
		/// <summary>
		/// A trace message
		/// </summary>
		Trace = 6,
		///<summary>
		/// A critical message
		///</summary>
		Critical = 7,
		/// <summary>
		/// Stack trace
		/// </summary>
		StackTrace = 8
	}
}
