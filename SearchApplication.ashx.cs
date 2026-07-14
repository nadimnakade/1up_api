using DialerAPi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace PickupAPi
{
    /// <summary>
    /// Summary description for SearchApplication
    /// </summary>
    public class SearchApplication : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("ASHX WORKING");
        }
        //public void ProcessRequest(HttpContext context)
        //{

        //    LogError("SQL ERROR: Into the function");

        //    context.Response.ContentType = "application/json";

        //    try
        //    {
        //        // 1. Parameter Parsing (Corrected to use safer TryParse/null coalescing)
        //        int displayLength = int.TryParse(context.Request["iDisplayLength"], out int dl) ? dl : 10;
        //        // Your original code added 1 to displayStart here, retaining that logic:
        //        int displayStart = (int.TryParse(context.Request["iDisplayStart"], out int ds) ? ds : 0) + 1;
        //        int sortCol = int.TryParse(context.Request["iSortCol_0"], out int sc) ? sc : 0;
        //        int UserKey = int.TryParse(context.Request["UserKey"], out int uk) ? uk : -1;

        //        string sortDir = context.Request["sSortDir_0"] ?? string.Empty;
        //        string search = context.Request["sSearch"] ?? string.Empty;
        //        string strLoginRegion = context.Request["sSearch_3"] ?? string.Empty;
        //        string strAppDate = context.Request["sSearch_4"] ?? string.Empty;
        //        string strBank = context.Request["sSearch_6"] ?? string.Empty;
        //        string strStatus = context.Request["sSearch_12"] ?? string.Empty;
        //        string strCaller = context.Request["sSearch_9"] ?? string.Empty;
        //        string strSupervisor = context.Request["sSearch_10"] ?? string.Empty;

        //        int filteredCount = 0;

        //        clientdetails objClientData = new clientdetails();
        //        DataSet dsClientData = null;

        //        // 2. Data Retrieval
        //        dsClientData = objClientData.get_ClientDetailsCustomPaging(
        //            displayStart, displayLength, sortCol, sortDir, search,
        //            UserKey, strLoginRegion, strBank, strSupervisor, strCaller, strStatus, strAppDate
        //        );

        //        List<clientdetails> Objclnt_data = new List<clientdetails>();

        //        // 3. Data Mapping and Total Count Extraction
        //        if (dsClientData != null && dsClientData.Tables.Count > 0 && dsClientData.Tables[0].Rows.Count > 0)
        //        {
        //            DataTable dt = dsClientData.Tables[0];
        //            DataRow firstRow = dt.Rows[0];

        //            // Filtered count should only be read once, typically from the first row
        //            if (dt.Columns.Contains("TotalCount") && firstRow["TotalCount"] != DBNull.Value)
        //            {
        //                filteredCount = Convert.ToInt32(firstRow["TotalCount"]);
        //            }

        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                DataRow row = dt.Rows[i];
        //                clientdetails objapp = new clientdetails();

        //                // --- Data Mapping ---
        //                objapp.UniqId = row["Id"].ToString();
        //                objapp.app_no = row["APP_NO"].ToString();
        //                objapp.bank_name = row["BANK_NAME"].ToString();
        //                objapp.loan_name = row["LOAN_NAME"].ToString();
        //                objapp.loan_scheme_name = row["LOAN_SCHEME_NAME"].ToString();
        //                objapp.loan_amnt = row["LOAN_AMNT"].ToString();
        //                objapp.purpose_of_loan = row["PURPOSE_OF_LOAN"].ToString();
        //                objapp.login_channel_name = row["LOGIN_CHANNEL_NAME"].ToString();
        //                objapp.login_region_name = row["LOGIN_REGION_NAME"].ToString();
        //                objapp.client_name = row["CLIENT_NAME"].ToString();

        //                // Safely handle client_mbl_no conversion
        //                if (row["CLIENT_MBL_NO"] != DBNull.Value)
        //                {
        //                    long mblNo;
        //                    if (long.TryParse(row["CLIENT_MBL_NO"].ToString(), out mblNo))
        //                    {
        //                        objapp.client_mbl_no = mblNo;
        //                    }
        //                }

        //                objapp.client_DOB = row["CLIENT_DOB"].ToString();
        //                objapp.client_pan = row["CLIENT_PAN"].ToString();
        //                objapp.client_aadhar_no = row["CLIENT_AADHAR_NO"].ToString();
        //                objapp.gender = row["GENDER"].ToString();
        //                objapp.marital_status = row["MARIATAL_STATUS"].ToString();
        //                objapp.desig_name = row["DESIG_NAME"].ToString();
        //                objapp.tel_caller_name = row["TEL_CALLER_NAME"].ToString();
        //                objapp.supervisor_name = row["SUPERVISOR_NAME"].ToString();
        //                objapp.los_no = row["LOS_NO"].ToString();
        //                objapp.app_date = row["app_date"].ToString();
        //                objapp.cpv_status = row["cpv_status"].ToString();
        //                objapp.remarks = row["Remarks"].ToString();
        //                objapp.StatusDesc = row["StatusDesc"].ToString();
        //                objapp.UpdatedOn = row["UpdatedOn"].ToString();
        //                objapp.hold_status = row["HOLD_STATUS"].ToString();
        //                objapp.add_doc_req = row["add_doc_req"].ToString();
        //                objapp.img_rel_remarks = row["img_rel_remarks"].ToString();
        //                objapp.disbursed_amnt = row["amnt_disbursed"].ToString();
        //                objapp.disbursal_date = row["disbursal_date"].ToString();

        //                Objclnt_data.Add(objapp);
        //            }
        //        }

        //        // 4. Construct Final Response
        //        var result = new
        //        {
        //            iTotalRecords = GetEmployeeTotalCount(),
        //            iTotalDisplayRecords = filteredCount,
        //            aaData = Objclnt_data
        //        };

        //        JavaScriptSerializer js = new JavaScriptSerializer();
        //        context.Response.Write(js.Serialize(result));
        //    }
        //    // 5. CRITICAL FIX: Robust, graceful error handling for production stability
        //    catch (SqlException sqlex)
        //    {
        //        LogError("SQL ERROR: " + sqlex.ToString());

        //        context.Response.StatusCode = 500;
        //        context.Response.Write("{\"error\":\"Database error occurred\"}");
        //    }

        //    catch (Exception ex)
        //    {
        //        LogError("GENERAL ERROR: " + ex.ToString());

        //        context.Response.StatusCode = 500;
        //        context.Response.Write("{\"error\":\"Internal server error\"}");
        //    }
        //}

        private int GetEmployeeTotalCount()
        {
            int totalEmployeeCount = 0;
            // Fetch connection string from Web.config using ConfigurationManager
            ConnectionStringSettings csSettings = ConfigurationManager.ConnectionStrings["DBCS"];

            if (csSettings == null)
            {
                System.Diagnostics.Trace.WriteLine("Configuration Warning: Connection string 'DBCS' is missing.");
                return 0;
            }

            using (SqlConnection con = new SqlConnection(csSettings.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("select count(*) from TBL_CIP_CLIENT_APPLICATION_DETAILS_COUNT", con);
                cmd.CommandTimeout = 30;
                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        totalEmployeeCount = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Error getting total count: {ex.Message}");
                    return 0;
                }
            }
            return totalEmployeeCount;
        }

        public bool IsReusable => false;

        private void LogError(string message)
        {
            try
            {
                string logPath = HttpContext.Current.Server.MapPath("~/Logs/error_log.txt");

                string logMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    + " - " + message + Environment.NewLine;

                System.IO.File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // avoid crash if logging fails
            }
        }
    }



}
