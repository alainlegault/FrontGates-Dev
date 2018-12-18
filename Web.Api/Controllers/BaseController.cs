using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Web.Api.ViewModels;

namespace Web.Api.Controllers
{
	public class BaseController : ApiController
	{
		public static bool IsViewModelRequestEnabled = true;
		public string CallerId => HttpContext.Current.User.Identity.Name ?? "Guest";

		public T GetRequestFromHeaders<T>() where T : IViewModelRequest
		{
			if (!Request.Headers.TryGetValues("SignatureRequest", out IEnumerable<string> signatureRequests))
				return default(T);

			var signatureRequest = signatureRequests.FirstOrDefault();

			if (signatureRequest.IsNullOrEmpty())
				return default(T);

			var request = JsonConvert.DeserializeObject<T>(signatureRequest);

			return request;
		}

		public string CurrentAuthorizer
		{
			get
			{
				if (!Request.Headers.TryGetValues("SignatureAuthorizer", out IEnumerable<string> signatureAuthorizers))
					return string.Empty;

				var signatureAuthorizer = signatureAuthorizers.FirstOrDefault();

				if (signatureAuthorizer.IsNullOrEmpty())
					return string.Empty;

				var identity = new GenericIdentity(signatureAuthorizer);
				var principal = new GenericPrincipal(identity, null);

				Thread.CurrentPrincipal = principal;

				return signatureAuthorizer;
			}
		}

		protected int CurrentUserTimezoneOffset
		{
			get
			{
				if (!Request.Headers.TryGetValues("TimezoneOffset", out IEnumerable<string> timezoneOffsets))
					return -4;

				var timezoneOffset = timezoneOffsets.FirstOrDefault();

				if (timezoneOffset.IsNullOrEmpty())
					return -4;

				return timezoneOffset.To<int>();
			}
		}

		protected string CurrentCultureCode => Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

		public class PagedRequest
		{
			public int Take { get; set; }
			public int PageNumber { get; set; }
		}
	}
}