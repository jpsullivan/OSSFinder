using System.Web.Optimization;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Transformers;
using BundleTransformer.UglifyJs.Minifiers;
using BundleTransformer.Yui.Minifiers;

namespace OSSFinder.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            var cssTransformer = new CssTransformer(new YuiCssMinifier());
            var jsTransformer = new JsTransformer(new UglifyJsMinifier());
            var nullOrderer = new NullOrderer();

            // Lib CSS
            Bundle libCss = new Bundle("~/bundles/css_lib").IncludeDirectory("~/Content/css/lib/aui", "*.css");
            //libCss.Transforms.Add(cssTransformer);
            bundles.Add(libCss);

            // App CSS
            Bundle appCss = new Bundle("~/bundles/css_app").Include("~/Content/css/app/bootstrap.less", new CssRewriteUrlTransform());
            appCss.Transforms.Add(cssTransformer);
            bundles.Add(appCss);

//            // App CSS
//            Bundle appCss = new Bundle("~/bundles/css_app").Include("~/Content/less/app/bootstrap.less", new CssRewriteUrlTransform());
//            appCss.Transforms.Add(cssTransformer);
//            bundles.Add(appCss);

            // Lib JS
            Bundle libJs = new Bundle("~/bundles/js_lib").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Content/js/lib/aui/aui-all.js");
            libJs.Transforms.Add(jsTransformer);
            libJs.Orderer = nullOrderer;
            bundles.Add(libJs);


#if (DEBUG && !DEBUGMINIFIED)
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}