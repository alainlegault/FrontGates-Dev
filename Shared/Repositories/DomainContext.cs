using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Repositories
{
	public enum DatabaseName
	{
		Default = 0,
		Tools = 1,
		Financials = 2
	}

	public class DomainContext
	{
		private readonly string _defaultConnectionString;
		private readonly string _toolsConnectionString;
		private readonly string _financialsConnectionString;

		public DomainContext(string defaultConnectionString)
		{
			_defaultConnectionString = defaultConnectionString;
		}

		public DomainContext(string defaultConnectionString, string toolsConnectionString, string financialsConnectionString, string cardsConnectionString)
		{
			if (defaultConnectionString.IsNotNullOrEmpty())
				_defaultConnectionString = defaultConnectionString;

			if (toolsConnectionString.IsNotNullOrEmpty())
				_toolsConnectionString = toolsConnectionString;

			if (financialsConnectionString.IsNotNullOrEmpty())
				_financialsConnectionString = financialsConnectionString;
		}

		/// <summary>
		/// Returns a new open connection to the Axis database.
		/// </summary>
		public SqlConnection CreateDbConnection()
		{
			return !_defaultConnectionString.ToLower().Contains(".mdf") && !_defaultConnectionString.ToLower().Contains("attachdbfilename") ?
				DbUtils.GetDbConnection(_defaultConnectionString) :
				DbUtils.GetDbConnectionNoEncryption(_defaultConnectionString);
		}

		/// <summary>
		/// Returns a new open connection to the Axis database.
		/// </summary>
		public SqlConnection CreateToolsDbConnection()
		{
			return !_toolsConnectionString.ToLower().Contains(".mdf") && !_toolsConnectionString.ToLower().Contains("attachdbfilename") ?
				DbUtils.GetDbConnection(_toolsConnectionString) :
				DbUtils.GetDbConnectionNoEncryption(_toolsConnectionString);
		}

		/// <summary>
		/// Returns a new open connection to the Axis database.
		/// </summary>
		public SqlConnection CreateFinancialsDbConnection()
		{
			return !_financialsConnectionString.ToLower().Contains(".mdf") && !_financialsConnectionString.ToLower().Contains("attachdbfilename") ?
				DbUtils.GetDbConnection(_financialsConnectionString) :
				DbUtils.GetDbConnectionNoEncryption(_financialsConnectionString);
		}

		public SqlConnection CreateConnection(DatabaseName dbName)
		{
			SqlConnection c;

			switch (dbName)
			{
				default:
					c = CreateDbConnection();
					break;
				case DatabaseName.Tools:
					c = CreateToolsDbConnection();
					break;
				case DatabaseName.Financials:
					c = CreateFinancialsDbConnection();
					break;
			}

			return c;
		}
	}
}
