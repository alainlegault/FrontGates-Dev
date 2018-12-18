using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	public class FriendlyDescriptionAttribute : Attribute
	{
		public string Text;

		public FriendlyDescriptionAttribute(string text)
		{
			Text = text;
		}
	}
}
