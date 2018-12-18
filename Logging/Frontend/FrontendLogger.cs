using FrontGatesDev.Logs.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Logs.Logging.Frontend
{
	public interface IFrontendLogger : ICustomLogger
	{

	}

	public sealed class FrontendLogger : CustomLogger, IFrontendLogger
	{
		public FrontendLogger(FrontendLoggingConfig config) : base(config)
		{

		}
	}
}
