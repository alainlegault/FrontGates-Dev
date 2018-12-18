using Shared.Attributes;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public enum UserType
	{
		User = 0
	}

	public enum UserRole
	{
		[FriendlyDescription("User")]
		User = 1,
		[FriendlyDescription("Business Owner")]
		Moderator,
		[FriendlyDescription("Administrator")]
		Admin,
		[FriendlyDescription("Super Administrator")]
		SuperAdmin
	}


	[DbTableName("Users")]
	public class UserModel : BaseModel
	{
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Password { get; set; }
		public string Culture { get; set; } = "en-CA";
		public string StreetNumber { get; set; }
		public string City { get; set; }
		public string Province { get; set; } = "Ontario";
		public string Country { get; set; } = "Canada";
		public bool OptIn { get; set; }
		public string PhoneNumber { get; set; }
		public string StreetName { get; set; }
		public string PostalCode { get; set; }
		public int AccessLevel { get; set; }
		public UserRole Role => (UserRole)AccessLevel;
		public string RoleName => Role.GetFriendlyDescription();
		public DateTime LastLogin { get; set; }
		public UserType Type { get; set; } = 0;		
		public string FullName => $"{FirstName} {LastName}".Trim();
	}
}
