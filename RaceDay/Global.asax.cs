using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

using RaceDay.Models;

namespace RaceDay
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801
	public class MvcApplication : System.Web.HttpApplication
	{
		public static InMemoryCache cache = null;

		protected void Application_Start()
		{
			cache = new InMemoryCache();

			AreaRegistration.RegisterAllAreas();

			GlobalConfiguration.Configure(WebApiConfig.Register);

			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
			if (HttpContext.Current.User != null)
			{
				if (HttpContext.Current.User.Identity.AuthenticationType == "Forms")
				{
					System.Web.Security.FormsIdentity id = (System.Web.Security.FormsIdentity)HttpContext.Current.User.Identity;
					FormsAuthenticationTicket ticket = id.Ticket;

					// Get Facebook information
					//
					Facebook.FacebookConnection fb = new Facebook.FacebookConnection(id);
					HttpContext.Current.User = fb.GetFacebookUser(ticket.Name);
				}
			}
		}

		/// <summary>
		/// returns an absolute url for the current application
		/// </summary>
		/// <returns></returns>
		public static string GetAbsoluteHref()
		{
			return GetAbsoluteHref(HttpContext.Current);
		}

		/// <summary>
		/// returns an absolute url for the current application
		/// </summary>
		/// <returns></returns>
		public static string GetAbsoluteHref(HttpContext context)
		{
			HttpRequest req = context.Request;

			string host = req.Url.Host;

			if (host == "localhost")
				host += ":" + req.Url.Port.ToString();

			string href = "http://" + (host).Replace("//", "/");

			if (href.Substring(href.Length - 1, 1) != "/")
				href += "/";

			if (req.IsSecureConnection)
				href = href.Replace("http://", "https://");

			return href;
		}

		/// <summary>
		/// creates an absolute href for a file.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string ConvertToAbsoluteHref(string filePath)
		{
			//chop a leading /
			if (!String.IsNullOrEmpty(filePath))
			{
				if (filePath.Substring(0, 1) == "/")
					filePath = filePath.Substring(1);
			}

			return GetAbsoluteHref() + filePath;
		}
	}
}