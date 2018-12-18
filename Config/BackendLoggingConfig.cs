using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logs.Config
{
	/// <summary>
	/// Represents a configuration fragment for the "Logging' section in the appsettings.json file.
	/// </summary>
	public class BackendLoggingConfig : CustomLoggingConfig
	{
		public bool UseDatabase { get; set; }
		public string ConnectionString { get; set; }
	}
}
