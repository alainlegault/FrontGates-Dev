using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logs.Extensions
{
	public static class DbCommandExtensions
	{
		public static void AddParameter(this IDbCommand cmd, string parameterName, object value)
		{
			if (string.IsNullOrEmpty(parameterName))
				throw new Exception("The provided parameter name is null");

			if (!parameterName.Contains("@"))
				throw new Exception($"The parameter name '{parameterName}' must start with an '@'.");

			object val = DBNull.Value;

			if (value != null)
				val = value;

			var p = new SqlParameter(parameterName, val);

			cmd.Parameters.Add(p);
		}
	}
}
