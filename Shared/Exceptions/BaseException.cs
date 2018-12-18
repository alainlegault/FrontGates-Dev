using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Exceptions
{
	/// <summary>
	/// The exception that is thrown when an error occurs.
	/// </summary>
	public abstract class BaseException : Exception
	{
		/// <summary>
		///  Initializes a new instance of the <see cref="BaseException"/> class.
		/// </summary>
		public BaseException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseException"/> class with  a specified error message.
		/// </summary>  
		public BaseException(string message) : base(message) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="BaseException"/> class with
		///  a specified error message and a reference to the inner exception that is
		///  the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">
		///  The exception that is the cause of the current exception. If the innerException
		///  parameter is not a null reference, the current exception is raised in a catch
		///  block that handles the inner exception.
		/// </param>   
		public BaseException(string message, Exception innerException) : base(message, innerException) { }
	}
}
