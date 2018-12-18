using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Api.Client
{
	public class HttpClientRequest<TRequest> : HttpClientRequest
	{
		public TRequest Object { get; set; }
	}

	public class HttpClientRequest
	{
		public string Route { get; set; }
		public ExecuteResponseType Method { get; set; }
	}
}
