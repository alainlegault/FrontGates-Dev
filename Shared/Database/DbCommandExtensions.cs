using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Database
{
	public static class DbCommandExtensions
	{
		/// <summary>
		/// Adds a parameter to the <see cref="IDbCommand"/> implementation.
		/// </summary>
		/// <param name="cmd">The command to which we want to add a parameter.</param>
		/// <param name="parameterName">The name of the parameter, that must begin with an "@".</param>
		/// <param name="value">The value of the parameter. If this value is null, the DBNull.Value 
		///                     will be put as the sql parameter value.</param>
		public static void AddParameter(this IDbCommand cmd, string parameterName, object value)
		{
			if (string.IsNullOrEmpty(parameterName))
				throw new DomainException("The provided parameter name is null.");

			if (parameterName.Contains("@") == false)
				throw new DomainException($"The parameter name '{parameterName}' must start with an '@'.");

			object val = DBNull.Value;

			if (value != null)
				val = value;

			var p = new SqlParameter(parameterName, val);

			cmd.Parameters.Add(p);
		}
	}
}
