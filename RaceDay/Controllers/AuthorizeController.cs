using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using RaceDay.Facebook;
using RaceDay.ViewModels;
using RaceDay.Utilities;

namespace RaceDay.Controllers
{
	[HandleError]
	public partial class AuthorizeController : BaseController
    {

		/// <summary>
		/// GET: /Authorize/FBApp
		/// 
		/// Called by the other controllers when it has been determined that
		/// the app should redirect back to to the facebook canvas page
		/// 
		/// i.e. apps.facebook.com/RaceDay
		/// </summary>
		/// <returns></returns>
		/// 
		public virtual ActionResult FBApp()
		{
			FBAppViewModel viewModel = new FBAppViewModel()
			{
				ApplicationProtocol = Request.Url.Scheme.ToString(),
				ApplicationPath = RaceDayConfiguration.Instance.ApplicationPath
			};

			return View(viewModel);
		}

		/// <summary>
		/// GET: /Authorize/Logon
		/// 
		/// Called when authorization fails.  This will typically happen when the user
		/// loads the app for the first time and the Facebook authorization needs to 
		/// be initiated.  Javascript is needed to set the window.top.location so the 
		/// FB login can be executed outside of the iframe.  OAuth requires Application ID
		/// and RedirectURI.
		/// 
		/// Also, check for error message if Facebook login fails or the user does not
		/// allow the application permissions.  This is the only action run when not
		/// authorized so the occurs here.
		/// </summary>
		/// <returns></returns>
		/// 
		public virtual ActionResult Logon()
		{
			LogonViewModel viewModel = new LogonViewModel();

			// Check for error parameters
			//
			if (!String.IsNullOrEmpty(Request.QueryString["error"]))
			{
				String error = Request.QueryString["error"];
				String reason = Request.QueryString["error_reason"];
				String description = Request.QueryString["error_description"];

				// If user denied access, then display view that just redirects back to Facebook so it
				// loads the user page by default
				//
				if (String.Compare(reason, "user_denied", StringComparison.OrdinalIgnoreCase) == 0)
					return View(MVC.Authorize.Views.ViewNames.Denied);

				// Otherwise, display Error view with message defined
				//
				viewModel.PageMessage  = new PageMessageModel(MessageDismissEnum.none, CssMessageClassEnum.alertblock, String.Format("{0} : {1} : {2]", error, reason, description));
				return View(MVC.Authorize.Views.ViewNames.Error, viewModel);
			}

			// Set the Oauth url parameters
			//
			string returnUrl = Request.QueryString["ReturnUrl"];
			if (String.IsNullOrEmpty(returnUrl))
				returnUrl = VirtualPathUtility.ToAbsolute("~/");
			viewModel.ApplicationId = RaceDayConfiguration.Instance.ApplicationId;
			viewModel.RedirectUri = String.Concat(Request.Url.Scheme, "://", Request.Url.Host, (!Request.Url.IsDefaultPort ? ":" + Request.Url.Port : ""), returnUrl);
			viewModel.Permissions = RaceDayConfiguration.Instance.ApplicationPermissions;

			return View(viewModel);
		}
    }
}
