using System.Web;
using System.Web.Optimization;

namespace ChildrenAdoptionApi
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
            //added src for chosen: searchable ddl
            bundles.Add(new StyleBundle("~/chosen_v1.8.2/css")
                .Include("~/chosen_v1.8.2/chosen.css"));
            bundles.Add(new ScriptBundle("~/chosen_v1.8.2/js")
                .Include("~/chosen_v1.8.2/chosen.jquery.js"));
        }
    }
}
