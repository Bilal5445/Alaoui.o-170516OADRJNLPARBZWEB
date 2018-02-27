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
                      "~/Content/site.css",
                    "~/Content/mysite_adekwasy.css"
                    ));

            bundles.Add(new ScriptBundle("~/bundles/custom").Include("~/Scripts/script.js"));

            // local to train page
            bundles.Add(new StyleBundle("~/Content/css_train").Include(
                      "~/Content/mysite_train.css",
                      "~/Content/mysite_train_themepanel.css",
                      "~/Content/mysite_train_keywordfiltering.css",
                      "~/Content/mysite_train_bulkimport.css"
                      ));

            // local to train page
            bundles.Add(new ScriptBundle("~/bundles/js_train").Include(
                "~/Scripts/mysite_train.js"
                ));

            // local to train page
            bundles.Add(new ScriptBundle("~/bundles/js_train_fbs").Include(
                "~/Scripts/mysite_train_fbs.js"
                ));

            // local to train page
            bundles.Add(new ScriptBundle("~/bundles/js_train_workingdata").Include(
                "~/Scripts/mysite_train_workingdata.js"
                ));

            // local to train page : translate single only
            bundles.Add(new ScriptBundle("~/bundles/js_train_translate_single").Include(
                "~/Scripts/mysite_train_translate_single.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js_train_keywordfiltering").Include(
                "~/Scripts/mysite_train_keywordfiltering.js"
                ));

            // klipfolio
            bundles.Add(new StyleBundle("~/Content/css_train_klpfl").Include(
                      "~/Content/mysite_train_klpfl.css"));

            // related to tags input
            bundles.Add(new ScriptBundle("~/bundles/js_train_tagsinput").Include(
                "~/bower_components/bootstrap-tagsinput/dist/bootstrap-tagsinput.min.js"
                ));
            bundles.Add(new StyleBundle("~/Content/css_train_tagsinput").Include(
                "~/bower_components/bootstrap-tagsinput/dist/bootstrap-tagsinput.css"
                ));
        }
    }
}
