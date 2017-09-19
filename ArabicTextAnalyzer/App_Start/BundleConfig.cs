using System.Web;
using System.Web.Optimization;

namespace ArabicTextAnalyzer
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/custom").Include("~/Scripts/script.js"));

            // local to train page
            bundles.Add(new StyleBundle("~/Content/css_train").Include(
                      "~/Content/mysite_train.css",
                      "~/Content/mysite_train_themepanel.css",
                      "~/Content/mysite_train_keywordfiltering.css"
                      ));

            // related to mark in tables
            bundles.Add(new ScriptBundle("~/bundles/js_train_mark").Include(
                "~/bower_components/datatables.net/js/jquery.dataTables.min.js",
                "~/bower_components/datatables/media/js/dataTables.bootstrap.min.js",
                "~/bower_components/mark.js/dist/jquery.mark.min.js",
                "~/bower_components/datatables.mark.js/dist/datatables.mark.js", // needed before mysite_train_keywordfiltering
                "~/Scripts/mysite_train.js",
                "~/Scripts/mysite_train_keywordfiltering.js"
                ));

            // local to train page
            bundles.Add(new ScriptBundle("~/bundles/js_train").Include(
                "~/Scripts/mysite_train.js",
                "~/Scripts/mysite_train_keywordfiltering.js"
                ));
        }
    }
}
