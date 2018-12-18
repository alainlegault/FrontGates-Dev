using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Security
{
	public static class SaltedHash
	{
		public static string CreateSalt(string email)
		{
			var bytes = Encoding.Default.GetBytes("thissecretykeyshouldbebetterandshouldbechanged");
			var hasher = new Rfc2898DeriveBytes(email, bytes, 10000);

			return Convert.ToBase64String(hasher.GetBytes(25));
		}

		public static string HashPassword(string salt, string password) => Convert.ToBase64String(new Rfc2898DeriveBytes(password, Encoding.Default.GetBytes(salt), 10000).GetBytes(25));
	}
}
