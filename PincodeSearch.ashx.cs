using PickupAPi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.SqlClient;

namespace PickupAPi
{
    /// <summary>
    /// Generic handler for Pincode-based bank search.
    /// </summary>
    public class PincodeSearchApplication : HttpTaskAsyncHandler
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            const string handlerName = "PincodeSearchApplication";

            try
            {
                // 1) Parse DataTables-like paging/sorting params
                int displayLength = int.TryParse(context.Request["iDisplayLength"], out int dl) ? dl : 10;
                int displayStart = int.TryParse(context.Request["iDisplayStart"], out int ds) ? ds : 0; // 0-based
                int sortCol = int.TryParse(context.Request["iSortCol_0"], out int sc) ? sc : 0;
                string sortDir = context.Request["sSortDir_0"] ?? string.Empty;

                // 2) Pincode input (fallback to sSearch)
                string searchText = context.Request["Pincode"]
                    ?? context.Request["PinCode"]
                    ?? context.Request["sSearch"]
                    ?? string.Empty;

                var pickup = new Pickup();

                // 3) Fetch data via existing stored procedure
                List<BankSearch> records = await pickup.BankListAsync(
                    displayStart,
                    displayLength,
                    sortCol,
                    sortDir,
                    searchText
                ) ?? new List<BankSearch>();

                // 4) Aggregate total count (the proc returns it in each row)
                int totalCount = (records.Count > 0) ? records[0].TotalCount : 0;

                var result = new
                {
                    iTotalRecords = totalCount,
                    iTotalDisplayRecords = totalCount,
                    aaData = records
                };

                JavaScriptSerializer js = new JavaScriptSerializer();
                context.Response.Write(js.Serialize(result));
            }
            catch (SqlException sqlex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var sqlErrorCode = sqlex.Number;
                var sqlProcedure = sqlex.Procedure;
                context.Response.Write(
                    $"{{\"error\":\"Database operation failed or timed out.\", \"code\":{sqlErrorCode}, \"procedure\":\"{sqlProcedure}\", \"details\":\"SQL Exception: {sqlex.Message}\"}}"
                );
                context.Trace.Write(handlerName, "SQL Exception: " + sqlex.ToString());
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Write($"{{\"error\":\"An unhandled internal server error occurred.\", \"details\":\"General Exception: {ex.Message}\"}}");
                context.Trace.Write(handlerName, "General Exception: " + ex.ToString());
            }
        }

        public override bool IsReusable => false;
    }
}
