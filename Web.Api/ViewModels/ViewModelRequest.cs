using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Api.ViewModels
{
	public interface IViewModelRequest
	{
		int Level { get; set; }
	}

	public class BaseViewModelRequest : IViewModelRequest
	{
		public int Level { get; set; } = 1;
	}
}