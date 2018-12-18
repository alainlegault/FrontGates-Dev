using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Utils
{
	public static class UserUtils
	{
		public static string GetCurrentEmail()
		{
			if (Thread.CurrentPrincipal != null && !Thread.CurrentPrincipal.Identity.Name.IsNullOrEmpty())
				return Thread.CurrentPrincipal.Identity.Name;

			//throw new Exception("Cannot get current principal identity name");
			return "anonymous@frontgatesdev.com";
		}
	}
}
