using Swashbuckle.Application;
using System.Linq;
using System.Web.Http;


namespace PickupAPi
{
    public static class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", "PickupAPi");
                        c.PrettyPrint();
                        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                    })
                .EnableSwaggerUi(c =>
                    {
                        c.DocumentTitle("PickupAPi API Docs");
                    });
        }
    }
}
