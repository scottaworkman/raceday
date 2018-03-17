using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using RaceDay;
using RaceDay.ViewModels;

namespace System.Web.Mvc.Html
{
	public static class HelperExtensions
	{
		public static IHtmlString PageMessage(this HtmlHelper helper, PageMessageModel message)
		{
			if (message != null)
				return helper.Raw(message.ToString());
			else
				return new MvcHtmlString("");
		}

		public static IHtmlString GoogleCalendar(this HtmlHelper helper, DateTime eventDate, String eventName, String eventLocation, String eventUrl)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("https://www.google.com/calendar/event?action=TEMPLATE");
			sb.Append("&text=" + HttpUtility.UrlEncode(eventName));
			sb.Append("&dates=" + eventDate.ToString("yyyyMMdd") + "/" + eventDate.AddDays(1).ToString("yyyyMMdd"));
			if (!String.IsNullOrEmpty(eventLocation))
				sb.Append("&location=" + HttpUtility.UrlEncode(eventLocation));
			if (!String.IsNullOrEmpty(eventUrl))
				sb.Append("&details=" + HttpUtility.UrlEncode(eventUrl));

			return new MvcHtmlString(sb.ToString());
		}

		public static IHtmlString PageMessageFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression)
			where TModel : BaseViewModel
			where TProperty : PageMessageModel
		{
			if (helper.ViewData.Model == null)
				return new MvcHtmlString("");

			PageMessageModel model = expression.Compile()(helper.ViewData.Model);
			return helper.PageMessage(model);
		}

		public static MvcHtmlString ActionUrl(this HtmlHelper htmlHelper, String action, String controller, Object routeValues)
		{
			var urlhelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
			var url = urlhelper.Action(action, controller, routeValues);

			return MvcHtmlString.Create(url);
		}

		public static MvcHtmlString ActionImage(this HtmlHelper htmlHelper, String altText, String action, String controller, Object routeValues, Object attributes)
		{
			var urlhelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
			var url = urlhelper.Action(action, controller, routeValues);

			var propString = String.Empty;
			PropertyInfo[] props = attributes.GetType().GetProperties();
			foreach (PropertyInfo prop in props)
			{
				propString += String.Format("{0}{1}=\"{2}\"", (String.IsNullOrEmpty(propString) ? "" : " "), prop.Name.Replace('_', '-'), prop.GetValue(attributes));
			}

			var image = String.Format("<img src=\"{0}\" alt=\"{1}\" {2} />", url, altText, propString);
			return MvcHtmlString.Create(image);
		}

		/// <summary>
		/// Checks the ModelState for an error, and returns the given error string if there is one, or null if there is no error
		/// Used to set class="error" on elements to present the error to the user
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="htmlHelper"></param>
		/// <param name="expression"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static MvcHtmlString ValidationErrorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string error)
		{
			if (HasError(htmlHelper, ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData), ExpressionHelper.GetExpressionText(expression)))
				return new MvcHtmlString(error);
			else
				return null;
		}


		private static bool HasError(this HtmlHelper htmlHelper, ModelMetadata modelMetadata, string expression)
		{
			string modelName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression);
			FormContext formContext = htmlHelper.ViewContext.FormContext;
			if (formContext == null)
				return false;

			if (!htmlHelper.ViewData.ModelState.ContainsKey(modelName))
				return false;

			ModelState modelState = htmlHelper.ViewData.ModelState[modelName];
			if (modelState == null)
				return false;

			ModelErrorCollection modelErrors = modelState.Errors;
			if (modelErrors == null)
				return false;

			return (modelErrors.Count > 0);
		}
	}
}