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
            PdfSharp.Fonts.GlobalFontSettings.FontResolver = new PickupAPi.Utils.ArialFontResolver();
            System.Diagnostics.Debug.WriteLine("[AppStart] Font resolver set to ArialFontResolver");

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            Context.Response.Headers.Remove("Access-Control-Allow-Origin");
            Context.Response.Headers.Remove("Access-Control-Allow-Methods");
            Context.Response.Headers.Remove("Access-Control-Allow-Headers");
            Context.Response.Headers.Remove("Access-Control-Max-Age");

            Context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            Context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            Context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept, X-Requested-With");
            Context.Response.AddHeader("Access-Control-Max-Age", "86400");

            if (Context.Request.HttpMethod == "OPTIONS")
            {
                Context.Response.StatusCode = 200;
                Context.Response.End();
            }
        }       
    }
}
