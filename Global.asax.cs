using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PickupAPi
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Set font resolver BEFORE any PdfSharp type is loaded
            PdfSharp.Fonts.GlobalFontSettings.FontResolver = new PickupAPi.Utils.ArialFontResolver();
            System.Diagnostics.Debug.WriteLine("[AppStart] Font resolver set to ArialFontResolver");

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_EndRequest()
        {
            if (Context.Response.StatusCode == 405 && Context.Request.HttpMethod == "OPTIONS")
            {
                Response.Clear();
                Response.StatusCode = 200;
                Response.End();
            }
        }
    }
}
