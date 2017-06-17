using System.Web;
using System.Web.Optimization;

namespace RaceDay
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js",
						"~/Scripts/jquery.validate.js",
						"~/Scripts/jquery.validate.unobtrusive.js"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
					  "~/Scripts/bootstrap.js",
					  "~/Scripts/moment.js",
					  "~/Scripts/bootstrap-datetimepicker.js"));

			bundles.Add(new ScriptBundle("~/bundles/global").Include(
					  "~/Scripts/global.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
					  "~/Content/bootstrap.css",
					  "~/Content/bootstrap-datetimepicker.css",
					  "~/Content/site.css"));
		}
	}
}