using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace PickupAPi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            //var cors = new EnableCorsAttribute("https://nomadix-kms.document360.io", "*", "*");
            //config.EnableCors();

            //config.Routes.MapHttpRoute
            //(
            //        name: "ActionApi",
            //        routeTemplate: "api/{controller}/{action}"
            //);
            config.Formatters.XmlFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data"));
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                            name: "DefaultApi",
                            routeTemplate: "api/{controller}/{action}/{id}",
                            defaults: new { id = RouteParameter.Optional }
                        );

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            SwaggerConfig.Register(config);
        }


    }
}
