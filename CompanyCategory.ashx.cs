using PickupAPi.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace PickupAPi.handler
{
    /// <summary>
    /// Summary description for CompanyCategory
    /// </summary>
    public class CompanyCategory : HttpTaskAsyncHandler
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            const string handlerName = "CompanyCategory";

            try
            {
                // 1. Parameter Parsing - Using TryParse for safety against crashes from bad input
                int displayLength = int.TryParse(context.Request["iDisplayLength"], out int dl) ? dl : 10;
                int displayStart = (int.TryParse(context.Request["iDisplayStart"], out int ds) ? ds : 0) + 1;
                int sortCol = int.TryParse(context.Request["iSortCol_0"], out int sc) ? sc : 0;

                int productKey = int.TryParse(context.Request["ProductKey"], out int pk) ? pk : 0;
                int bankKey = int.TryParse(context.Request["BankKey"], out int bk) ? bk : 0;

                string sortDir = context.Request["sSortDir_0"] ?? string.Empty;
                string search = context.Request["sSearch"] ?? string.Empty;

                //// Input validation for critical keys
                //if (productKey <= 0 || bankKey <= 0)
                //{
                //    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                //    context.Response.Write("{\"error\":\"ProductKey or BankKey is missing or invalid.\"}");
                //    return;
                //}

                Pickup BO = new Pickup();

                // 2. Data Retrieval
                List<CompCat> lstCompCat = await BO.CompanyCatListAsync(
                    displayStart, displayLength, sortCol, sortDir, search,
                    productKey, bankKey
                ) ?? new List<CompCat>();

                // 3. Data Aggregation
                var totalCount = (lstCompCat.Count > 0) ? lstCompCat[0].TOTALCOUNT : 0;

                var result = new
                {
                    iTotalRecords = totalCount,
                    iTotalDisplayRecords = totalCount,
                    aaData = lstCompCat
                };

                // 4. Send Response
                JavaScriptSerializer js = new JavaScriptSerializer();
                context.Response.Write(js.Serialize(result));
            }
            // 5. CRITICAL FIX: Robust, graceful error handling for production stability
            catch (SqlException sqlex)
            {
                // Handles SQL errors, including timeouts, and returns a clean 500 status.
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Write($"{{\"error\":\"Database operation failed or timed out.\", \"details\":\"SQL Exception: {sqlex.Message}\"}}");
                context.Trace.Write(handlerName, "SQL Exception: " + sqlex.ToString());
            }
            catch (Exception ex)
            {
                // Catches all other exceptions (parsing errors, configuration errors, etc.)
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Write($"{{\"error\":\"An unhandled internal server error occurred.\", \"details\":\"General Exception: {ex.Message}\"}}");
                context.Trace.Write(handlerName, "General Exception: " + ex.ToString());
            }
        }

        public override bool IsReusable
        {
            get { return false; }
        }
    }
}

