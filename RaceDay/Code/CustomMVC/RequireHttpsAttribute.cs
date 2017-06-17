using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using RaceDay;
using RaceDay.Utilities;

namespace ActionFilters
{
	public class RequireHttpsAttribute : System.Web.Mvc.RequireHttpsAttribute
	{
		public bool RequireSecure = false;
		public bool NoRedirect = false;
		public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
		{
			if ((!NoRedirect) && (RaceDayConfiguration.Instance.UseHttps))
			{
				// default RequireHttps functionality only if allowed in the web.config
				//
				if (RequireSecure)
					HandleHttpsRequest(filterContext);
				else
					HandleNonHttpsRequest(filterContext);
			}
		}

		protected virtual void HandleHttpsRequest(AuthorizationContext filterContext)
		{
			if (!filterContext.HttpContext.Request.IsSecureConnection)
				HandleRequest(filterContext, "https://", (RaceDayConfiguration.Instance.HttpsPort != 443 ? (Int32?)RaceDayConfiguration.Instance.HttpsPort : null));
		}

		protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
		{
			if (filterContext.HttpContext.Request.IsSecureConnection)
				HandleRequest(filterContext, "http://", (RaceDayConfiguration.Instance.HttpPort != 80 ? (Int32?)RaceDayConfiguration.Instance.HttpPort : null));
		}

		protected virtual void HandleRequest(AuthorizationContext filterContext, String protocol, Int32? port)
		{
			if (String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
			{
				// redirect to HTTP version of page
				string url = protocol + filterContext.HttpContext.Request.Url.Host + (port.HasValue ? String.Format(":{0}", port.Value) : String.Empty) + filterContext.HttpContext.Request.RawUrl;
				filterContext.Result = new RedirectResult(url);
			}
		}
	}
}
