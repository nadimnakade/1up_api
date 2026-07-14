using DialerAPi.Models;
using PickupAPi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace PickupAPi
{
    /// <summary>
    /// Summary description for BankSearchApplication
    /// </summary>
    public class BankSearchApplication : HttpTaskAsyncHandler
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            const string handlerName = "BankSearchApplication";

            try
            {
                // 1. Parameter Parsing - Using TryParse for safety against crashes from bad input
                int displayLength = int.TryParse(context.Request["iDisplayLength"], out int dl) ? dl : 10;
                // DataTables uses 0-based start; OFFSET expects 0-based. Do not add +1.
                int displayStart = int.TryParse(context.Request["iDisplayStart"], out int ds) ? ds : 0;
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
                List<BankSearch> lstBankSearch = await BO.BankListAsync(
                    displayStart, displayLength, sortCol, sortDir, search
                    
                ) ?? new List<BankSearch>();

                // 3. Data Aggregation
                var totalCount = (lstBankSearch.Count > 0) ? lstBankSearch[0].TotalCount : 0;

                var result = new
                {
                    iTotalRecords = totalCount,
                    iTotalDisplayRecords = totalCount,
                    aaData = lstBankSearch
                };

                // 4. Send Response
                JavaScriptSerializer js = new JavaScriptSerializer();
                context.Response.Write(js.Serialize(result));
            }
            // 5. CRITICAL FIX: Robust, graceful error handling for production stability
            catch (SqlException sqlex)
            {
                // Handles SQL errors, including timeouts, and returns a clean 500 status with useful diagnostics.
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var sqlErrorCode = sqlex.Number; // e.g., 3701 for "cannot drop table" errors
                var sqlProcedure = sqlex.Procedure; // may be empty if not from a stored procedure
                context.Response.Write(
                    $"{{\"error\":\"Database operation failed or timed out.\", \"code\":{sqlErrorCode}, \"procedure\":\"{sqlProcedure}\", \"details\":\"SQL Exception: {sqlex.Message}\"}}"
                );
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
