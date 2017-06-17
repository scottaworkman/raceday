using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RaceDay
{
	public class AjaxRedirectResult : ActionResult
	{
		ActionResult redirectTo = null;

		private AjaxRedirectResult() { }

		public AjaxRedirectResult(ActionResult redirectTo) { this.redirectTo = redirectTo; }

		public override void ExecuteResult(ControllerContext context)
		{
			context.HttpContext.Response.Buffer = true;
			context.HttpContext.Response.Clear();

			context.HttpContext.Response.AddHeader("AJAX_REDIRECT", new UrlHelper(context.RequestContext).Action(redirectTo));
		}
	}
}