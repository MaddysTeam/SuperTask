using System.Web;
using System.Web.Optimization;

namespace XzNursery.Admin
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
			// JS			jquery
			bundles.Add(new ScriptBundle("~/js/jquery").Include(
							"~/assets/js/jquery-1.11.1.min.js"
							));

			// JS+CSS	bootstrap
			bundles.Add(new ScriptBundle("~/js/bootstrap").Include(
							"~/assets/js/bootstrap.min.js"
							));
			bundles.Add(new StyleBundle("~/css/bootstrap").Include(
							"~/assets/css/bootstrap.min.css"
							));

			// JS			jquery
			bundles.Add(new ScriptBundle("~/js/modernizr").Include(
							"~/assets/js/modernizr-2.6.2.min.js"
							));

			// CSS		font-awesome
			bundles.Add(new StyleBundle("~/css/font-awesome").Include(
							"~/assets/css/font-awesome.min.css"
							));

			// JS			jqueryval-mis
			bundles.Add(new ScriptBundle("~/js/jqueryval-mis").Include(
							"~/assets/js/jqueryval-mis.min.js"
							));

			// JS+CSS	plugins
			bundles.Add(new ScriptBundle("~/js/plugins").Include(
							"~/assets/js/plugins.min.js"
							));
			bundles.Add(new StyleBundle("~/css/plugins").Include(
							"~/assets/css/plugins.min.css"
							));

			// JS			jstree
			bundles.Add(new StyleBundle("~/css/jstree").Include(
							"~/assets/plugins/jstree-3.2.1/themes/default/style.min.css"
							));
			bundles.Add(new ScriptBundle("~/js/jstree").Include(
							"~/assets/js/jstree.min.js"
							));

			// JS			state - flot
			bundles.Add(new ScriptBundle("~/js/state/flot").Include(
							"~/assets/js/jquery.flot.min.js"
							));

			// JS+CSS	themes - king
			bundles.Add(new ScriptBundle("~/js/themes/king").Include(
							"~/assets/js/king-common.min.js"/*,
							"~/assets/js/deliswitch.js"*/));
			bundles.Add(new StyleBundle("~/css/themes/king").Include(
							"~/assets/css/main.min.css"/*,
							"~/assets/css/style-switcher.css"*/));

			// alertify
			bundles.Add(new ScriptBundle("~/js/alertify").Include(
							"~/assets/plugins/alertify.js-0.3.11/src/alertify.js"));
			bundles.Add(new StyleBundle("~/css/alertify").Include(
							"~/assets/plugins/alertify.js-0.3.11/themes/alertify.core.css",
							"~/assets/plugins/alertify.js-0.3.11/themes/alertify.bootstrap.css"));

			// JS+CSS	app
			bundles.Add(new ScriptBundle("~/js/app").Include(
							"~/assets/js/app.js"));
			bundles.Add(new StyleBundle("~/css/app").Include(
							"~/assets/css/app.css"));
		}
    }
}
