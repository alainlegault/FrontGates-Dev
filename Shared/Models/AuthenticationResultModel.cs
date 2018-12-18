using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class AuthenticationResultModel
	{
		public bool IsValid { get; set; }
		public string TokenId { get; set; }
	}
}
