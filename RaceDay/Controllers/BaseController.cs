using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RaceDay.Facebook;
using RaceDay.Models;
using RaceDay.Utilities;

namespace RaceDay.Controllers
{
	public interface IRenderPartialView
	{
		string RenderPartialViewToString(string viewName, object model);
	}

	public interface IRenderView
	{
		string RenderViewToString(string viewName, object model, string masterName);
	}

	[ActionFilters.RequireHttps(RequireSecure = true)]
	public abstract partial class BaseController : Controller, IRenderPartialView, IRenderView
	{
		// Application specific functionality
		//
		protected Boolean AppEventNotification(String EventName, DateTime EventDate)
		{
			if (!String.IsNullOrEmpty(RaceDayConfiguration.Instance.NotifyAdminUser))
			{
				String message = String.Format("{0} added the event {1} for {2}",
					FacebookConnection.UserTemplateToken(FacebookUser.CurrentUser.id), EventName, EventDate.ToShortDateString());
				FacebookConnection fb = new FacebookConnection(FacebookUser.CurrentUser.Identity);
				return fb.UserNotification(
					FacebookConnection.AppToken(RaceDayConfiguration.Instance.ApplicationId, RaceDayConfiguration.Instance.ApplicationSecret), 
					RaceDayConfiguration.Instance.NotifyAdminUser, 
					"event", 
					"/", 
					message);
			}

			return true;
		}

		protected Boolean AppUserNotification(String UserId, String Group)
		{
			if (!String.IsNullOrEmpty(RaceDayConfiguration.Instance.NotifyAdminUser))
			{
				String message = String.Format("{0} joined RaceDay group {1}", FacebookConnection.UserTemplateToken(UserId), Group);
				FacebookConnection fb = new FacebookConnection(FacebookUser.CurrentUser.Identity);
				return fb.UserNotification(
					FacebookConnection.AppToken(RaceDayConfiguration.Instance.ApplicationId, RaceDayConfiguration.Instance.ApplicationSecret),
					RaceDayConfiguration.Instance.NotifyAdminUser,
					"newuser",
					"/",
					message);
			}

			return true;
		}

		// Page Message functionality
		//
		public const string TempData_PageMessage = "TempData_PageMessage";

		protected ActionResult CustomErrorPage(CssMessageClassEnum cssClass, string msg)
		{
			return View(MVC.Shared.Views.Error, new PageMessageModel(MessageDismissEnum.none, cssClass, msg));
		}

		protected ActionResult PageMessage(PageMessageModel messageModel)
		{
			return PartialView(MVC.Shared.Views.Partials._PageMessage, messageModel);
		}

		public virtual ActionResult RedirectToActionWithMessage(ActionResult redirectAction, PageMessageModel model)
		{
			SetTempDataPageMessage(model);
			return RedirectToRoute(redirectAction.GetT4MVCResult().RouteValueDictionary);
		}

		public virtual ActionResult AjaxRedirect(ActionResult redirectAction)
		{
			return new AjaxRedirectResult(redirectAction);
		}

		protected void SetTempDataPageMessage(PageMessageModel model)
		{
			TempData[TempData_PageMessage] = model;
		}

		protected PageMessageModel GetTempDataPageMessage()
		{
			return TempData[TempData_PageMessage] as PageMessageModel;
		}

		protected HttpException Get404Exception()
		{
			return new HttpException(404, "Not found");
		}

		// Handles custom rendering of views and partial views to string.  Not controlled through IoC as these are typically set to itself
		//
		protected IRenderPartialView partialRenderer;
		protected IRenderView viewRenderer;

		public IRenderPartialView PartialRenderer
		{
			get { return partialRenderer; }
			set { partialRenderer = value; }
		}

		public IRenderView ViewRenderer
		{
			get { return viewRenderer; }
			set { viewRenderer = value; }
		}

		public BaseController()
		{
			PartialRenderer = this;
			ViewRenderer = this;
		}

		#region IRenderView and IRenderPartialView implementation

		public string RenderPartialViewToString(string viewName, object model)
		{
			ViewData.Model = model;
			using (var sw = new StringWriter())
			{
				var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
				var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
				viewResult.View.Render(viewContext, sw);
				viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
				return sw.GetStringBuilder().ToString();
			}
		}

		public string RenderViewToString(string viewName, object model, string masterName)
		{
			if (string.IsNullOrEmpty(viewName))
				viewName = ControllerContext.RouteData.GetRequiredString("action");

			ViewBag.BaseUrl = MvcApplication.ConvertToAbsoluteHref(Url.Action("Index", "Home"));
			ViewData.Model = model;

			using (StringWriter sw = new StringWriter())
			{
				ViewEngineResult viewResult = ViewEngines.Engines.FindView(ControllerContext, viewName, masterName);
				ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
				viewResult.View.Render(viewContext, sw);

				return sw.GetStringBuilder().ToString();
			}
		}

		#endregion
	}
}