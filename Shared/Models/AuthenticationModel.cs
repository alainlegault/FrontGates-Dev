using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class AuthenticationModel
	{
		public string Login { get; set; }
		public string Password { get; set; }
		public string UserLanguageCode { get; set; }
	}
}
