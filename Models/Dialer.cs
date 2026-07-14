using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using DialerAPi.Models;
using System.Configuration;
using PickupAPi.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using iTextSharp.text;

namespace DialerAPi.Models
{
    public class RequestHeader
    {
        [Required]
        public string DeviceModel { get; set; }

        public string DeviceId { get; set; }

        [Required]
        public string Platform { get; set; }

        [Required]
        public string RequestType { get; set; }
    }

    public class RequestAppInfo
    {
        [Required]
        public string AppVersion { get; set; }
    }

    public class CallLogInput
    {
        //[Required]
        //public RequestHeader RequestHeader { get; set; }

        //[Required]
        //public RequestAppInfo AppInfo { get; set; }

        [Required]
        public CallLog RequestData { get; set; }


    }

    public class CallLog
    {
        public List<CallLogList> lstCallLog { get; set; }
    }
    public class CallLogList
    {
        public string date { get; set; }
        public string number { get; set; }
        public string type { get; set; }
        public string duration { get; set; }
        public string cachedNumberType { get; set; }
        public string phoneAccountId { get; set; }
        public string viaNumber { get; set; }
        public string name { get; set; }
        public string contact { get; set; }
        public string photo { get; set; }
        public string thumbPhotopublic { get; set; }
        public int userkey { get; set; }

    }

    public class CallLogUtilities
    {
        public List<CallLogUtilitiesList> lstCallLog { get; set; }
    }
    

    public class CallLogUtilitiesList
    {
        public string dateTime { get; set; }
        public string phoneNumber { get; set; }
        public string type { get; set; }
        public string duration { get; set; }        
        public string name { get; set; }        
        public int userkey { get; set; }

    }
    public class DashboardCounter
    {
        public string TotalDisbursedAmt { get; set; }
        public string Rank { get; set; }
        public string DailyLoginCount { get; set; }
        public string DailyLoginAmt { get; set; }
        public string Duration { get; set; }
        public string DisbursedAmt { get; internal set; }
        public string ClientConnected { get; internal set; }
        public string MonthlyLeadCount { get; set; }
        public string DailyLeadCount { get; set; }
        public string MonthlyLoginAmt { get; internal set; }
        public string MonthlyLoginCount { get; internal set; }
        public string Incentive { get; internal set; }
        public string Points { get; internal set; }
        public string PL { get; internal set; }
        public string HL { get; internal set; }
        public string BL { get; internal set; }
        public string LAP { get; internal set; }
        public string CC { get; internal set; }


    }
    public class DialerCounter
    {

        public string Location { get; set; }
        public string TL { get; set; }
        public string Caller { get; set; }
        public string DND { get; set; }
        public string Lead { get; set; }
        public string FollowUp { get; set; }
        public string NI { get; set; }
        public string NotDoable { get; set; }
        public string WrongNo { get; set; }
        public string Blank { get; set; }
        public string Auto { get; set; }
        public string Manual { get; set; }
        public string Total { get; set; }
        public string Duration { get; set; }
        public string DisbursedCount { get; internal set; }
        public string LoginAmt { get; internal set; }
        public string DisbursedAmt { get; internal set; }
        public string LoginCount { get; internal set; }
        public string Ringing { get; set; }
        public string TotalDisbursedAmt { get; set; }
        public string Rank { get; set; }
        public string DailyLogin { get; set; }
        public string DailyLoginAmt { get; set; }
        public string SameDayFollowUp { get; set; }
        public string LastCalledDate { get; internal set; }
    }
    public class Attendance
    {
        public string intime { get; set; }
        public string outtime { get; set; }
        public string entrydate { get; set; }
        public string IsHalfDay { get; set; }
        public string IsLate { get; set; }
        public string Salary { get; set; }
        public string SalaryEarned { get; set; }
        public string TotalDays { get; set; }
        public string AbsentDays { get; set; }
        public string Late { get; set; }
        public string HalfDay { get; set; }
        public string workingdays { get; set; }
    }
    public class Calling
    {
        public int CD_KEY { get; set; }
        public string CD_CLIENT_NAME { get; set; }
        public string CD_CONTACT { get; set; }
        public string CD_SALARY { get; set; }
        public string CD_COMPANY { get; set; }
        public string CD_REMARKS { get; set; }
        public string CD_CAMPAIGN_NAME { get; set; }
    }

    public class ClientDtl
    {
        public string ClientName { get; set; }
        public string MobileNo { get; set; }
        public string BirthDayOn { get; set; }
        public string Age { get; set; }

    }

    public class Performance
    {
        public string January { get; set; }
        public string February { get; set; }
        public string March { get; set; }
        public string April { get; set; }
        public string May { get; set; }
        public string June { get; set; }
        public string July { get; set; }
        public string August { get; set; }
        public string September { get; set; }
        public string October { get; set; }
        public string November { get; set; }
        public string December { get; set; }
        public string Caller { get; set; }


    }

    public class LiveCount
    {
        public int DND { get; set; }
        public int Lead { get; set; }
        public int FollowUp { get; set; }
        public int Followup { get; set; }
        public int NI { get; set; }
        public int NotDoable { get; set; }
        public int WrongNo { get; set; }
        public int Blank { get; set; }
        public int Interested { get; set; }
        public int NotContactable { get; set; }
        public int Ringing { get; set; }
        public int TotalCount { get; set; }
        public int TotalClientConnected { get; set; }
        public string Duration { get; set; }
        public string TotalLoginAmt { get; set; }
        public string TotalLogin { get; set; }
        public string TotalDisbursedAmt { get; set; }
        public string TotalDisbursedCount { get; set; }
    }

    public class ClientDocument
    {


        public int Id { get; set; }
        public string FileName { get; set; }
        public byte[] File { get; set; }
        public string FileType { get; set; }
        public string FileBase64String { get; set; }
        public DateTime CreatedOn { get; internal set; }
        public string Name { get; internal set; }
        public string MobileNo { get; internal set; }
    }
    public class Dialer
    {
        public int CD_KEY { get; set; }
        public string CD_CLIENT_NAME { get; set; }
        public string CD_CONTACT { get; set; }
        public string CD_SALARY { get; set; }
        public string CD_COMPANY { get; set; }
        public string CD_REMARKS { get; set; }
        public string CD_STATUS { get; set; }
        public string CD_ASSIGNED_TO { get; set; }
        public string CD_ASSIGNED_TO_NAME { get; set; }
        public string CD_ADDRESS { get; set; }
        public string CD_PRODUCT { get; set; }
        public string CD_PRODUCT_Desc { get; set; }
        public string CD_PINCODE { get; set; }
        public string CD_CAMPAIGN_NAME { get; set; }
        public string CD_CREATED_BY { get; set; }
        public string CD_CREATED_ON { get; set; }
        public string CD_MODIFIED_ON { get; set; }
        public string CD_CALLER_ID { get; set; }
        public int CD_IS_HLLAPBL { get; set; }
        public int CD_IS_CIBIL_BAD { get; set; }
        public string CD_CALLBACK_DATE
        {
            get; set;
        }

        public string CD_CALLBACK_TIME
        {
            get; set;
        }

        public string CD_CALLER_REMARKS
        {
            get; set;
        }

        public string CD_CALLER_DURATION
        {
            get; set;
        }

        public string CD_CALL_DATE_TIME
        {
            get; set;
        }

        public int CD_IS_REALESTATE
        {
            get; set;
        }


        public List<Calling> FetchDialerData(int UserKey)
        {
            List<Calling> lst = new List<Calling>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_DIALER_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Calling objDialer = new Calling();
                    objDialer.CD_KEY = Convert.ToInt32(dt.Rows[i]["CD_KEY"]);
                    objDialer.CD_CLIENT_NAME = Convert.ToString(dt.Rows[i]["CD_CLIENT_NAME"]);
                    objDialer.CD_CONTACT = Convert.ToString(dt.Rows[i]["CD_CONTACT"]);
                    objDialer.CD_SALARY = Convert.ToString(dt.Rows[i]["CD_SALARY"]);
                    objDialer.CD_COMPANY = Convert.ToString(dt.Rows[i]["CD_COMPANY"]);
                    objDialer.CD_REMARKS = Convert.ToString(dt.Rows[i]["CD_REMARKS"]);
                    objDialer.CD_CAMPAIGN_NAME = Convert.ToString(dt.Rows[i]["CD_CAMPAIGN_NAME"]);
                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Dialer> FetchDialerNotUpdateData(int UserKey)
        {
            List<Dialer> lst = new List<Dialer>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_DIALER_NOT_UPDATED_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Dialer objDialer = new Dialer();
                    objDialer.CD_KEY = Convert.ToInt32(dt.Rows[i]["CD_KEY"]);
                    objDialer.CD_CLIENT_NAME = Convert.ToString(dt.Rows[i]["CD_CLIENT_NAME"]);
                    objDialer.CD_CONTACT = Convert.ToString(dt.Rows[i]["CD_CONTACT"]);
                    objDialer.CD_SALARY = Convert.ToString(dt.Rows[i]["CD_SALARY"]);
                    objDialer.CD_COMPANY = Convert.ToString(dt.Rows[i]["CD_COMPANY"]);
                    objDialer.CD_STATUS = Convert.ToString(dt.Rows[i]["CD_STATUS"]);
                    objDialer.CD_ASSIGNED_TO = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO"]);
                    objDialer.CD_ASSIGNED_TO_NAME = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO_NAME"]);
                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Dialer> FetchLeadnFollowupData(int UserKey, string Type)
        {
            List<Dialer> lst = new List<Dialer>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.Parameters.AddWithValue("@TYPE", Type);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_LEAD_N_FOLLOWUP_DATA_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Dialer objDialer = new Dialer();
                    objDialer.CD_KEY = Convert.ToInt32(dt.Rows[i]["CD_KEY"]);
                    objDialer.CD_CLIENT_NAME = Convert.ToString(dt.Rows[i]["CD_CLIENT_NAME"]);
                    objDialer.CD_CONTACT = Convert.ToString(dt.Rows[i]["CD_CONTACT"]);
                    objDialer.CD_SALARY = Convert.ToString(dt.Rows[i]["CD_SALARY"]);
                    objDialer.CD_COMPANY = Convert.ToString(dt.Rows[i]["CD_COMPANY"]);
                    objDialer.CD_STATUS = Convert.ToString(dt.Rows[i]["CD_STATUS"]);
                    objDialer.CD_ASSIGNED_TO = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO"]);
                    objDialer.CD_ASSIGNED_TO_NAME = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO_NAME"]);
                    objDialer.CD_CALLBACK_DATE = Convert.ToString(dt.Rows[i]["CD_CALLBACK_DATE"]);
                    objDialer.CD_CALLBACK_TIME = Convert.ToString(dt.Rows[i]["CD_CALLBACK_TIME"]);
                    objDialer.CD_CALLER_REMARKS = Convert.ToString(dt.Rows[i]["CD_CALLER_REMARKS"]);
                    objDialer.CD_CAMPAIGN_NAME = Convert.ToString(dt.Rows[i]["CD_CAMPAIGN_NAME"]);
                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Dialer> GetLead(int UserKey, int PageIndex)
        {
            List<Dialer> lst = new List<Dialer>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.Parameters.AddWithValue("@PageIndex", PageIndex);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_FETCH_LEAD_MASTER_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Dialer objDialer = new Dialer();
                    objDialer.CD_KEY = Convert.ToInt32(dt.Rows[i]["LeadKey"]);
                    objDialer.CD_CLIENT_NAME = Convert.ToString(dt.Rows[i]["Name"]);
                    objDialer.CD_CONTACT = Convert.ToString(dt.Rows[i]["MobileNo"]);
                    objDialer.CD_STATUS = Convert.ToString(dt.Rows[i]["Status"]);
                    objDialer.CD_REMARKS = Convert.ToString(dt.Rows[i]["Remarks"]);
                    objDialer.CD_PRODUCT_Desc = Convert.ToString(dt.Rows[i]["ProductDesc"]);
                    objDialer.CD_ASSIGNED_TO_NAME = Convert.ToString(dt.Rows[i]["AssignedToName"]);
                    objDialer.CD_CALLBACK_DATE = Convert.ToString(dt.Rows[i]["CallBackDate"]);
                    objDialer.CD_CALLBACK_TIME = Convert.ToString(dt.Rows[i]["CallBackTime"]);
                    objDialer.CD_CALLER_REMARKS = Convert.ToString(dt.Rows[i]["CallerRemarks"]);
                    objDialer.CD_CREATED_BY = Convert.ToString(dt.Rows[i]["CreatedByName"]);
                    objDialer.CD_CREATED_ON = Convert.ToString(dt.Rows[i]["CreatedDate"]);
                    objDialer.CD_MODIFIED_ON = Convert.ToString(dt.Rows[i]["ModifiedOn"]);
                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Dialer> FetchCallerData(int UserKey, string Type, int PageIndex)
        {
            List<Dialer> lst = new List<Dialer>();
            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.Parameters.AddWithValue("@TYPE", Type);
                _cmd.Parameters.AddWithValue("@PAGEINDEX", PageIndex);
                _cmd.CommandTimeout = 180;

                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_LEAD_N_FOLLOWUP_DATA_API_TEST");
                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Dialer objDialer = new Dialer();

                    objDialer.CD_KEY = Convert.ToInt32(dt.Rows[i]["CD_KEY"]);
                    objDialer.CD_CLIENT_NAME = Convert.ToString(dt.Rows[i]["CD_CLIENT_NAME"]);
                    objDialer.CD_CONTACT = Convert.ToString(dt.Rows[i]["CD_CONTACT"]);
                    objDialer.CD_SALARY = Convert.ToString(dt.Rows[i]["CD_SALARY"]);
                    objDialer.CD_COMPANY = Convert.ToString(dt.Rows[i]["CD_COMPANY"]);
                    objDialer.CD_STATUS = Convert.ToString(dt.Rows[i]["CD_STATUS"]);
                    objDialer.CD_ASSIGNED_TO = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO"]);
                    objDialer.CD_ASSIGNED_TO_NAME = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO_NAME"]);
                    objDialer.CD_CALLBACK_DATE = Convert.ToString(dt.Rows[i]["CD_CALLBACK_DATE"]);
                    objDialer.CD_CALLBACK_TIME = Convert.ToString(dt.Rows[i]["CD_CALLBACK_TIME"]);
                    objDialer.CD_CALLER_REMARKS = Convert.ToString(dt.Rows[i]["CD_CALLER_REMARKS"]);
                    objDialer.CD_CAMPAIGN_NAME = Convert.ToString(dt.Rows[i]["CD_CAMPAIGN_NAME"]);

                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Dialer> FetchCallingDataIndividual(int UserKey, int Key)
        {
            List<Dialer> lst = new List<Dialer>();
            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.Parameters.AddWithValue("@KEY", Key);
                
                _cmd.CommandTimeout = 180;

                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_CALLING_DATA_API_INDIVIDUAL");
                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Dialer objDialer = new Dialer();

                    objDialer.CD_KEY = Convert.ToInt32(dt.Rows[i]["CD_KEY"]);
                    objDialer.CD_CLIENT_NAME = Convert.ToString(dt.Rows[i]["CD_CLIENT_NAME"]);
                    objDialer.CD_CONTACT = Convert.ToString(dt.Rows[i]["CD_CONTACT"]);
                    objDialer.CD_SALARY = Convert.ToString(dt.Rows[i]["CD_SALARY"]);
                    objDialer.CD_COMPANY = Convert.ToString(dt.Rows[i]["CD_COMPANY"]);
                    objDialer.CD_STATUS = Convert.ToString(dt.Rows[i]["CD_STATUS"]);
                    objDialer.CD_ASSIGNED_TO = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO"]);
                    objDialer.CD_ASSIGNED_TO_NAME = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO_NAME"]);
                    objDialer.CD_CALLBACK_DATE = Convert.ToString(dt.Rows[i]["CD_CALLBACK_DATE"]);
                    objDialer.CD_CALLBACK_TIME = Convert.ToString(dt.Rows[i]["CD_CALLBACK_TIME"]);
                    objDialer.CD_CALLER_REMARKS = Convert.ToString(dt.Rows[i]["CD_CALLER_REMARKS"]);
                    objDialer.CD_CAMPAIGN_NAME = Convert.ToString(dt.Rows[i]["CD_CAMPAIGN_NAME"]);

                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        /*Live Count*/
        public List<LiveCount> FetchLiveCount(int UserKey)
        {
            List<LiveCount> lst = new List<LiveCount>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);

                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_DIALER_LIVE_COUNT_API]");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    LiveCount objDialer = new LiveCount();
                    objDialer.Lead = Convert.ToInt32(dt.Rows[i]["Lead"]);
                    objDialer.NotDoable = Convert.ToInt32(dt.Rows[i]["Not Doable"]);
                    objDialer.FollowUp = Convert.ToInt32(dt.Rows[i]["FollowUp"]);
                    objDialer.Followup = Convert.ToInt32(dt.Rows[i]["FollowUp"]);
                    objDialer.DND = Convert.ToInt32(dt.Rows[i]["DND"]);
                    objDialer.NI = Convert.ToInt32(dt.Rows[i]["NI"]);
                    objDialer.WrongNo = Convert.ToInt32(dt.Rows[i]["WrongNo"]);
                    objDialer.Blank = Convert.ToInt32(dt.Rows[i]["Blank"]);
                    objDialer.TotalCount = Convert.ToInt32(dt.Rows[i]["TotalCount"]);
                    objDialer.TotalClientConnected = Convert.ToInt32(dt.Rows[i]["TotalClientConnected"]);
                    objDialer.Duration = Convert.ToString(dt.Rows[i]["Duration"]);
                    objDialer.TotalLogin = Convert.ToString(dt.Rows[i]["TotalLogin"]);
                    objDialer.TotalLoginAmt = Convert.ToString(dt.Rows[i]["TotalLoginAmt"]);
                    objDialer.TotalDisbursedAmt = Convert.ToString(dt.Rows[i]["DisbursedAmt"]);
                    objDialer.TotalDisbursedCount = Convert.ToString(dt.Rows[i]["DisburseLogin"]);
                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Attendance> GetAttendance(int UserKey)
        {
            List<Attendance> lst = new List<Attendance>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);

                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_ATTENDANCE_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Attendance objDialer = new Attendance();
                    objDialer.intime = Convert.ToString(dt.Rows[i]["intime"]);
                    objDialer.intime = Convert.ToString(dt.Rows[i]["intime"]);
                    objDialer.outtime = Convert.ToString(dt.Rows[i]["outtime"]);
                    objDialer.entrydate = Convert.ToString(dt.Rows[i]["entrydate"]);
                    objDialer.IsHalfDay = Convert.ToString(dt.Rows[i]["IsHalfDay"]);
                    objDialer.IsLate = Convert.ToString(dt.Rows[i]["IsLate"]);
                    objDialer.TotalDays = Convert.ToString(dt.Rows[i]["actualdays"]);
                    objDialer.Salary = Convert.ToString(dt.Rows[i]["Salary"]);
                    objDialer.SalaryEarned = Convert.ToString(dt.Rows[i]["salaryearned"]);
                    objDialer.AbsentDays = Convert.ToString(dt.Rows[i]["absentdays"]);
                    objDialer.HalfDay = Convert.ToString(dt.Rows[i]["halfday"]);
                    objDialer.Late = Convert.ToString(dt.Rows[i]["Late"]);
                    objDialer.workingdays = Convert.ToString(dt.Rows[i]["workingdays"]);
                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }



        public List<DashboardCounter> DashboardCounter(int UserKey)
        {
            DataSet dsClientData = new DataSet(); ;
            List<DashboardCounter> lstDialerCounter = new List<DashboardCounter>();
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);
                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_DASHBOARD_COUNT_API");
                DataTable dt = dsClientData.Tables[0];

                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DashboardCounter objDialerCounter = new DashboardCounter();

                        objDialerCounter.Duration = Convert.ToString(dt.Rows[i]["Duration"]);
                        objDialerCounter.DisbursedAmt = Convert.ToString(dt.Rows[i]["DisbursedAmt"]);
                        objDialerCounter.TotalDisbursedAmt = Convert.ToString(dt.Rows[i]["TotalDisbursedAmt"]);
                        objDialerCounter.Rank = Convert.ToString(dt.Rows[i]["Rank"]);
                        objDialerCounter.Incentive = Convert.ToString(dt.Rows[i]["Incentive"]);
                        objDialerCounter.DailyLoginCount = Convert.ToString(dt.Rows[i]["DailyLoginCount"]);
                        objDialerCounter.DailyLoginAmt = Convert.ToString(dt.Rows[i]["DailyLoginAmt"]);
                        objDialerCounter.DailyLeadCount = Convert.ToString(dt.Rows[i]["DailyLeadCount"]);
                        objDialerCounter.MonthlyLeadCount = Convert.ToString(dt.Rows[i]["MonthlyLeadCount"]);
                        objDialerCounter.MonthlyLoginAmt = Convert.ToString(dt.Rows[i]["MonthlyLoginAmt"]);
                        objDialerCounter.MonthlyLoginCount = Convert.ToString(dt.Rows[i]["MonthlyLoginCount"]);
                        objDialerCounter.ClientConnected = Convert.ToString(dt.Rows[i]["ClientConnected"]);
                        objDialerCounter.Points = Convert.ToString(dt.Rows[i]["Points"]);

                        objDialerCounter.PL = Convert.ToString(dt.Rows[i]["PL"]);
                        objDialerCounter.BL = Convert.ToString(dt.Rows[i]["BL"]);
                        objDialerCounter.HL = Convert.ToString(dt.Rows[i]["HL"]);
                        objDialerCounter.LAP = Convert.ToString(dt.Rows[i]["LAP"]);
                        objDialerCounter.CC = Convert.ToString(dt.Rows[i]["CC"]);

                        lstDialerCounter.Add(objDialerCounter);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstDialerCounter;
        }


        public List<DialerCounter> DialerLive(int UserKey)
        {
            DataSet dsClientData = new DataSet(); ;
            List<DialerCounter> lstDialerCounter = new List<DialerCounter>();
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);
                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_DIALER_LIVE_COUNT_REPORT");
                DataTable dt = dsClientData.Tables[0];

                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DialerCounter objDialerCounter = new DialerCounter();
                        objDialerCounter.Location = Convert.ToString(dt.Rows[i]["Location"]);
                        objDialerCounter.TL = Convert.ToString(dt.Rows[i]["TL"]);
                        objDialerCounter.Caller = Convert.ToString(dt.Rows[i]["Caller"]);
                        objDialerCounter.DND = Convert.ToString(dt.Rows[i]["DND"]);
                        objDialerCounter.Lead = Convert.ToString(dt.Rows[i]["Lead"]);
                        objDialerCounter.NI = Convert.ToString(dt.Rows[i]["NI"]);
                        objDialerCounter.FollowUp = Convert.ToString(dt.Rows[i]["FollowUp"]);
                        objDialerCounter.NotDoable = Convert.ToString(dt.Rows[i]["Not Doable"]);
                        objDialerCounter.WrongNo = Convert.ToString(dt.Rows[i]["WrongNo"]);
                        objDialerCounter.Blank = Convert.ToString(dt.Rows[i]["Blank"]);
                        objDialerCounter.Auto = Convert.ToString(dt.Rows[i]["Auto"]);
                        objDialerCounter.Manual = Convert.ToString(dt.Rows[i]["Manual"]);
                        objDialerCounter.Total = Convert.ToString(dt.Rows[i]["TotalCount"]);
                        objDialerCounter.Duration = Convert.ToString(dt.Rows[i]["Duration"]);
                        objDialerCounter.DisbursedAmt = Convert.ToString(dt.Rows[i]["DisbursedAmt"]);
                        objDialerCounter.Ringing = Convert.ToString(dt.Rows[i]["Ringing"]);
                        objDialerCounter.TotalDisbursedAmt = Convert.ToString(dt.Rows[i]["TotalDisbursedAmt"]);
                        objDialerCounter.Rank = Convert.ToString(dt.Rows[i]["Rank"]);
                        objDialerCounter.DailyLogin = Convert.ToString(dt.Rows[i]["DailyLogin"]);
                        objDialerCounter.DailyLoginAmt = Convert.ToString(dt.Rows[i]["DailyLoginAmt"]);
                        objDialerCounter.SameDayFollowUp = Convert.ToString(dt.Rows[i]["SameDayFollowUp"]);
                        objDialerCounter.LastCalledDate = Convert.ToString(dt.Rows[i]["LastCalledDate"]);
                        lstDialerCounter.Add(objDialerCounter);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstDialerCounter;
        }


        public List<DialerCounter> DialerLiveIndv(int UserKey)
        {
            DataSet dsClientData = new DataSet(); ;
            List<DialerCounter> lstDialerCounter = new List<DialerCounter>();
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);
                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_DIALER_LIVE_COUNT_REPORT_API");
                DataTable dt = dsClientData.Tables[0];

                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DialerCounter objDialerCounter = new DialerCounter();
                        objDialerCounter.Location = Convert.ToString(dt.Rows[i]["Location"]);
                        objDialerCounter.TL = Convert.ToString(dt.Rows[i]["TL"]);
                        objDialerCounter.Caller = Convert.ToString(dt.Rows[i]["Caller"]);
                        objDialerCounter.DND = Convert.ToString(dt.Rows[i]["DND"]);
                        objDialerCounter.Lead = Convert.ToString(dt.Rows[i]["Lead"]);
                        objDialerCounter.NI = Convert.ToString(dt.Rows[i]["NI"]);
                        objDialerCounter.FollowUp = Convert.ToString(dt.Rows[i]["FollowUp"]);
                        objDialerCounter.NotDoable = Convert.ToString(dt.Rows[i]["Not Doable"]);
                        objDialerCounter.WrongNo = Convert.ToString(dt.Rows[i]["WrongNo"]);
                        objDialerCounter.Blank = Convert.ToString(dt.Rows[i]["Blank"]);
                        objDialerCounter.Auto = Convert.ToString(dt.Rows[i]["Auto"]);
                        objDialerCounter.Manual = Convert.ToString(dt.Rows[i]["Manual"]);
                        objDialerCounter.Total = Convert.ToString(dt.Rows[i]["TotalCount"]);
                        objDialerCounter.Duration = Convert.ToString(dt.Rows[i]["Duration"]);
                        objDialerCounter.DisbursedAmt = Convert.ToString(dt.Rows[i]["DisbursedAmt"]);
                        objDialerCounter.Ringing = Convert.ToString(dt.Rows[i]["Ringing"]);
                        objDialerCounter.TotalDisbursedAmt = Convert.ToString(dt.Rows[i]["TotalDisbursedAmt"]);
                        objDialerCounter.Rank = Convert.ToString(dt.Rows[i]["Rank"]);
                        objDialerCounter.DailyLogin = Convert.ToString(dt.Rows[i]["DailyLogin"]);
                        objDialerCounter.DailyLoginAmt = Convert.ToString(dt.Rows[i]["DailyLoginAmt"]);
                        objDialerCounter.SameDayFollowUp = Convert.ToString(dt.Rows[i]["SameDayFollowUp"]);
                        objDialerCounter.LastCalledDate = Convert.ToString(dt.Rows[i]["LastCalledDate"]);
                        lstDialerCounter.Add(objDialerCounter);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstDialerCounter;
        }

        public List<DialerCounter> CallLogLive(int UserKey)
        {

            DataSet dsClientData = new DataSet(); ;
            List<DialerCounter> lstDialerCounter = new List<DialerCounter>();
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);
                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_DIALER_LIVE_COUNT_REPORT");
                DataTable dt = dsClientData.Tables[0];

                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DialerCounter objDialerCounter = new DialerCounter();
                        objDialerCounter.Location = Convert.ToString(dt.Rows[i]["Location"]);
                        objDialerCounter.TL = Convert.ToString(dt.Rows[i]["TL"]);
                        objDialerCounter.Caller = Convert.ToString(dt.Rows[i]["Caller"]);
                        objDialerCounter.DND = Convert.ToString(dt.Rows[i]["DND"]);
                        objDialerCounter.Lead = Convert.ToString(dt.Rows[i]["Lead"]);
                        objDialerCounter.NI = Convert.ToString(dt.Rows[i]["NI"]);
                        objDialerCounter.FollowUp = Convert.ToString(dt.Rows[i]["FollowUp"]);
                        objDialerCounter.NotDoable = Convert.ToString(dt.Rows[i]["Not Doable"]);
                        objDialerCounter.WrongNo = Convert.ToString(dt.Rows[i]["WrongNo"]);
                        objDialerCounter.Blank = Convert.ToString(dt.Rows[i]["Blank"]);
                        objDialerCounter.Auto = Convert.ToString(dt.Rows[i]["Auto"]);
                        objDialerCounter.Manual = Convert.ToString(dt.Rows[i]["Manual"]);
                        objDialerCounter.Total = Convert.ToString(dt.Rows[i]["TotalCount"]);
                        objDialerCounter.Duration = Convert.ToString(dt.Rows[i]["Duration"]);
                        objDialerCounter.DisbursedCount = Convert.ToString(dt.Rows[i]["DisbursedCount"]);
                        objDialerCounter.LoginAmt = Convert.ToString(dt.Rows[i]["LoginAmt"]);
                        objDialerCounter.DisbursedAmt = Convert.ToString(dt.Rows[i]["DisbursedAmt"]);
                        objDialerCounter.LoginCount = Convert.ToString(dt.Rows[i]["LoginCount"]);
                        objDialerCounter.SameDayFollowUp = Convert.ToString(dt.Rows[i]["LoginCount"]);
                        lstDialerCounter.Add(objDialerCounter);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstDialerCounter;
        }

        public List<Employee> GetRanking(int UserKey)
        {
            List<Employee> lst = new List<Employee>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);

                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_CALLERS_RANKING");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Employee objEmployee = new Employee();
                    objEmployee.Rank = Convert.ToString(dt.Rows[i]["Rank"]);
                    objEmployee.Incentives = Convert.ToDecimal(dt.Rows[i]["Incentives"]);
                    objEmployee.Points = Convert.ToDecimal(dt.Rows[i]["Points"]);
                    objEmployee.EmployeeName = Convert.ToString(dt.Rows[i]["EMP_NAME"]);
                    lst.Add(objEmployee);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Employee> GetRankingOld(int UserKey)
        {
            List<Employee> lst = new List<Employee>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);

                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_CALLERS_RANKING_OLD");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Employee objEmployee = new Employee();
                    objEmployee.Rank = Convert.ToString(dt.Rows[i]["Rank"]);
                    objEmployee.Incentives = Convert.ToDecimal(dt.Rows[i]["Incentives"]);
                    objEmployee.Points = Convert.ToDecimal(dt.Rows[i]["Points"]);
                    objEmployee.EmployeeName = Convert.ToString(dt.Rows[i]["EMP_NAME"]);
                    lst.Add(objEmployee);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Performance> GetPerformance(int UserKey)
        {
            List<Performance> lst = new List<Performance>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);

                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_MONTHLY_MIS_REPORT_DISBURSED_INDV");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Performance objPerformance = new Performance();
                    objPerformance.January = dt.Columns.Contains("January") ? Convert.ToString(dt.Rows[i]["January"]) : "-1";
                    objPerformance.February = dt.Columns.Contains("February") ? Convert.ToString(dt.Rows[i]["February"]) : "-1";
                    objPerformance.March = dt.Columns.Contains("March") ? Convert.ToString(dt.Rows[i]["March"]) : "-1";

                    objPerformance.April = dt.Columns.Contains("April") ? Convert.ToString(dt.Rows[i]["April"]) : "-1";
                    objPerformance.May = dt.Columns.Contains("May") ? Convert.ToString(dt.Rows[i]["May"]) : "-1";
                    objPerformance.June = dt.Columns.Contains("June") ? Convert.ToString(dt.Rows[i]["June"]) : "-1";

                    objPerformance.July = dt.Columns.Contains("July") ? Convert.ToString(dt.Rows[i]["July"]) : "-1";
                    objPerformance.August = dt.Columns.Contains("August") ? Convert.ToString(dt.Rows[i]["August"]) : "-1";
                    objPerformance.September = dt.Columns.Contains("September") ? Convert.ToString(dt.Rows[i]["September"]) : "-1";

                    objPerformance.October = dt.Columns.Contains("October") ? Convert.ToString(dt.Rows[i]["October"]) : "-1";
                    objPerformance.November = dt.Columns.Contains("November") ? Convert.ToString(dt.Rows[i]["November"]) : "-1";
                    objPerformance.December = dt.Columns.Contains("December") ? Convert.ToString(dt.Rows[i]["December"]) : "-1";

                    objPerformance.Caller = Convert.ToString(dt.Rows[i]["Caller"]);
                    lst.Add(objPerformance);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public int IsAttendanceMarked(int UserKey, int Type)
        {
            DataSet ds = null;
            int intRetVal = 0;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);
                _cmd.Parameters.AddWithValue("@Type", Type);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_IF_ATTENDANCE_MARKED_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    intRetVal = Convert.ToInt32(dt.Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                intRetVal = -1;
            }
            return intRetVal;
        }

        public List<ClientDtl> GetClientsBirthday(int UserKey)
        {
            List<ClientDtl> lst = new List<ClientDtl>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_CLIENT_BIRTHDAY_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ClientDtl objClientDtl = new ClientDtl();
                    objClientDtl.ClientName = Convert.ToString(dt.Rows[i]["ClientName"]);
                    objClientDtl.MobileNo = Convert.ToString(dt.Rows[i]["MobileNo"]);
                    objClientDtl.BirthDayOn = Convert.ToString(dt.Rows[i]["BirthDayOn"]);
                    objClientDtl.Age = Convert.ToString(dt.Rows[i]["Age"]);
                    lst.Add(objClientDtl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }


        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {

                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }


        //public int FnSaveCallLogs(CallLogInput objCallLogInput)
        public int FnSaveCallLogs(List<CallLogList> lstCallLog)
        {
            //List<CallLogList> lstCallLog = objCallLogInput.RequestData.lstCallLog;            
            int intRetVal = 0;
            try
            {
                //if (lstCallLog[0].userkey == 24510 || lstCallLog[0].userkey == 1447)
                //{
                using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["DBCS"].ToString()))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {
                        con.Open();
                        sqlBulkCopy.DestinationTableName = "[TBL_CALL_LOGS]";
                        sqlBulkCopy.ColumnMappings.Add("name", "name");
                        sqlBulkCopy.ColumnMappings.Add("date", "date");
                        sqlBulkCopy.ColumnMappings.Add("duration", "duration");
                        sqlBulkCopy.ColumnMappings.Add("type", "type");
                        sqlBulkCopy.ColumnMappings.Add("userkey", "userkey");
                        sqlBulkCopy.ColumnMappings.Add("number", "number");
                        DataTable dt = ToDataTable(lstCallLog);
                        sqlBulkCopy.WriteToServer(dt);

                        con.Close();
                    }

                    intRetVal = 1;
                }
                //}
                //else
                //{
                //    for (int i = 0; i < lstCallLog.Count; i++)
                //    {
                //        SqlCommand _cmd = new SqlCommand();
                //        _cmd.Parameters.AddWithValue("@Name", lstCallLog[i].name);
                //        _cmd.Parameters.AddWithValue("@Date", lstCallLog[i].date);
                //        _cmd.Parameters.AddWithValue("@Duration", lstCallLog[i].duration);
                //        _cmd.Parameters.AddWithValue("@Type", lstCallLog[i].type);
                //        _cmd.Parameters.AddWithValue("@number", lstCallLog[i].number);
                //        _cmd.Parameters.AddWithValue("@UserKey", lstCallLog[i].userkey);

                //        _cmd.CommandTimeout = 180;
                //        SqlHelper.ExecuteNonQuery(ref _cmd, "USP_CALL_LOG");



                //        intRetVal = 1;
                //    }
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return intRetVal;
        }

        public int FnSaveCallLogsAll(List<CallLogList> CallLog)
        {
            //List<CallLogList> lstCallLog = objCallLogInput.RequestData.lstCallLog;            
            int intRetVal = 0;
            try
            {
                //if (lstCallLog[0].userkey == 24510 || lstCallLog[0].userkey == 1447)
                //{
                using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["DBCS"].ToString()))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {
                        con.Open();
                        sqlBulkCopy.DestinationTableName = "TBL_CALL_LOGS_ATTENDANCE";
                        sqlBulkCopy.ColumnMappings.Add("name", "name");
                        sqlBulkCopy.ColumnMappings.Add("date", "date");
                        sqlBulkCopy.ColumnMappings.Add("duration", "duration");
                        sqlBulkCopy.ColumnMappings.Add("type", "type");
                        sqlBulkCopy.ColumnMappings.Add("userkey", "userkey");
                        sqlBulkCopy.ColumnMappings.Add("number", "number");
                        DataTable dt = ToDataTable(CallLog);
                        sqlBulkCopy.WriteToServer(dt);

                        con.Close();
                    }

                    intRetVal = 1;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return intRetVal;
        }

        public int FnIndvCallLog(List<CallLogList> CallLog)
        {
            //List<CallLogList> lstCallLog = objCallLogInput.RequestData.lstCallLog;            
            int intRetVal = 0;
            try
            {
                //if (lstCallLog[0].userkey == 24510 || lstCallLog[0].userkey == 1447)
                //{
                using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["DBCS"].ToString()))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {
                        con.Open();
                        sqlBulkCopy.DestinationTableName = "TBL_CALL_LOGS_ATTENDANCE";
                        sqlBulkCopy.ColumnMappings.Add("name", "name");
                        sqlBulkCopy.ColumnMappings.Add("date", "date");
                        sqlBulkCopy.ColumnMappings.Add("duration", "duration");
                        sqlBulkCopy.ColumnMappings.Add("type", "type");
                        sqlBulkCopy.ColumnMappings.Add("userkey", "userkey");
                        sqlBulkCopy.ColumnMappings.Add("number", "number");
                        sqlBulkCopy.ColumnMappings.Add("source", "Dialer");
                        DataTable dt = ToDataTable(CallLog);
                        sqlBulkCopy.WriteToServer(dt);

                        con.Close();
                    }

                    intRetVal = 1;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return intRetVal;
        }



        public int FnSaveCallLogsIndv(CallLogList objCallLogList)
        {


            int intRetVal = 0;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@name", objCallLogList.name);
                _cmd.Parameters.AddWithValue("@date", objCallLogList.date);
                _cmd.Parameters.AddWithValue("@duration", objCallLogList.duration);
                _cmd.Parameters.AddWithValue("@type", objCallLogList.type);
                _cmd.Parameters.AddWithValue("@number", objCallLogList.number);
                _cmd.Parameters.AddWithValue("@userkey", objCallLogList.userkey);
                _cmd.CommandTimeout = 180;
                SqlHelper.ExecuteNonQuery(ref _cmd, "USP_CALL_LOG");



                intRetVal = 1;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return intRetVal;
        }




        public List<Application> GetDisbursalDtl(int CallerKey, string LoanId)
        {
            List<Application> lstApplication = new List<Application>();
            try
            {
                DataSet dsClientData = new DataSet();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;


                _cmd.Parameters.Add(new SqlParameter("@UserKey", CallerKey));
                _cmd.Parameters.Add(new SqlParameter("@LoanId", LoanId));

                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_DISBURSAL_TWISE_API");

                if (dsClientData.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsClientData.Tables[0].Rows.Count; i++)
                    {
                        Application objapp = new Application();
                        objapp.Bank = Convert.ToString(dsClientData.Tables[0].Rows[i]["Bank"]);
                        objapp.AppNo = Convert.ToString(dsClientData.Tables[0].Rows[i]["App No"]);
                        objapp.LosNo = Convert.ToString(dsClientData.Tables[0].Rows[i]["Los No"]);
                        objapp.ClientName = Convert.ToString(dsClientData.Tables[0].Rows[i]["Client Name"]);
                        objapp.Product = Convert.ToString(dsClientData.Tables[0].Rows[i]["Product"]);
                        objapp.DisbAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["Disbursal Amt"]);
                        objapp.DisbDate = Convert.ToString(dsClientData.Tables[0].Rows[i]["Disbursed On"]);

                        lstApplication.Add(objapp);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstApplication;
        }

        public List<DailyLogin> GetLoginDtls(int CallerKey, int Type)
        {
            List<DailyLogin> lstApplication = new List<DailyLogin>();
            try
            {
                DataSet dsClientData = new DataSet();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;

                _cmd.Parameters.Add(new SqlParameter("@SearchText", DBNull.Value));
                _cmd.Parameters.Add(new SqlParameter("@Caller", CallerKey));
                _cmd.Parameters.Add(new SqlParameter("@Type", Type));

                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_SEARCH_LOGIN_DAILY_API");

                if (dsClientData.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsClientData.Tables[0].Rows.Count; i++)
                    {
                        DailyLogin objapp = new DailyLogin();
                        objapp.Id = Convert.ToInt32(dsClientData.Tables[0].Rows[i]["Id"]);
                        objapp.Banks = Convert.ToString(dsClientData.Tables[0].Rows[i]["Banks"]);
                        objapp.Remarks = Convert.ToString(dsClientData.Tables[0].Rows[i]["Remarks"]);
                        objapp.LoginAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["LoginAmt"]);
                        objapp.Product = Convert.ToString(dsClientData.Tables[0].Rows[i]["Product"]);
                        objapp.Region = Convert.ToString(dsClientData.Tables[0].Rows[i]["Region"]);
                        objapp.Client = Convert.ToString(dsClientData.Tables[0].Rows[i]["Client"]);
                        objapp.LosNo = Convert.ToString(dsClientData.Tables[0].Rows[i]["LosNo"]);
                        objapp.CreatedOn = Convert.ToString(dsClientData.Tables[0].Rows[i]["Date"]);
                        lstApplication.Add(objapp);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstApplication;
        }

        public List<Dialer> GetLeadCount(int UserKey, int Type)
        {
            List<Dialer> lst = new List<Dialer>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.Parameters.AddWithValue("@Type", Type);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_FETCH_LEAD_MASTER_COUNT_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Dialer objDialer = new Dialer();
                    objDialer.CD_CLIENT_NAME = Convert.ToString(dt.Rows[i]["Name"]);
                    objDialer.CD_STATUS = Convert.ToString(dt.Rows[i]["Status"]);
                    objDialer.CD_REMARKS = Convert.ToString(dt.Rows[i]["Remarks"]);
                    objDialer.CD_PRODUCT_Desc = Convert.ToString(dt.Rows[i]["ProductDesc"]);
                    objDialer.CD_CALLER_REMARKS = Convert.ToString(dt.Rows[i]["CallerRemarks"]);
                    objDialer.CD_MODIFIED_ON = Convert.ToString(dt.Rows[i]["ModifiedOn"]);
                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public Employee LoginCheck(string UserName)
        {

            Employee objEmployee = new Employee();

            DataSet ds = null;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@UserName", UserName);


                ds = SqlHelper.ExecuteDS(ref com, "USP_FINANS_GET_EMPLOYEE_LOGIN_DETAIL_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {



                    objEmployee.Key = Convert.ToInt32(dt.Rows[i]["Key"]);
                    objEmployee.EmployeeNo = Convert.ToInt32(dt.Rows[i]["EmployeeNo"]);
                    objEmployee.EmployeeName = Convert.ToString(dt.Rows[i]["EmployeeName"]);
                    objEmployee.UserName = Convert.ToString(dt.Rows[i]["UserName"]);
                    objEmployee.Password = Convert.ToString(dt.Rows[i]["Password"]);
                    objEmployee.DesignationID = Convert.ToInt32(dt.Rows[i]["DesignationID"]);
                    objEmployee.DesignationDesc = Convert.ToString(dt.Rows[i]["DesignationDesc"]);
                    objEmployee.RoleID = Convert.ToInt64(dt.Rows[i]["RoleID"]);
                    objEmployee.RoleDesc = Convert.ToString(dt.Rows[i]["RoleDesc"]);
                    objEmployee.Salary = Convert.ToDecimal(dt.Rows[i]["Salary"]);
                    objEmployee.BossId = Convert.ToInt64(dt.Rows[i]["BossId"]);
                    objEmployee.ReportsToName = Convert.ToString(dt.Rows[i]["ReportsToName"]);
                    objEmployee.EmailId = Convert.ToString(dt.Rows[i]["EmailId"]);
                    objEmployee.ProgrammeManagerID = Convert.ToInt64(dt.Rows[i]["ProgrammeManagerID"]);
                    objEmployee.ProgrammeManagerName = Convert.ToString(dt.Rows[i]["ProgrammeManagerName"]);
                    objEmployee.ReportingManagerID = Convert.ToInt64(dt.Rows[i]["ReportingManagerID"]);
                    objEmployee.ReportingManagerName = Convert.ToString(dt.Rows[i]["ReportingManagerName"]);
                    objEmployee.BankAccountNo = Convert.ToString(dt.Rows[i]["BankAccountNo"]);
                    objEmployee.LocationDesc = Convert.ToString(dt.Rows[i]["LocationDesc"]);
                    objEmployee.ReportsTo = Convert.ToInt64(dt.Rows[i]["BossId"]);
                    objEmployee.DateOfJoining = Convert.ToString(dt.Rows[i]["DateOfJoining"]);
                    objEmployee.DateOfBirth = Convert.ToString(dt.Rows[i]["DateOfBirth"]);
                    objEmployee.Address = Convert.ToString(dt.Rows[i]["Address"]);
                    objEmployee.OfficeContactNo = Convert.ToInt64(dt.Rows[i]["OfficeContactNo"]);
                    objEmployee.MobileNo1 = Convert.ToInt64(dt.Rows[i]["MobileNo1"]);
                    objEmployee.MobileNo2 = Convert.ToInt64(dt.Rows[i]["MobileNo2"]);
                    objEmployee.AadharNo = Convert.ToString(dt.Rows[i]["AadharNo"]);
                    objEmployee.PanNo = Convert.ToString(dt.Rows[i]["PanNo"]);
                    objEmployee.PhotoURL = Convert.ToString(dt.Rows[i]["PhotoURL"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objEmployee;
        }

        public int fnSaveVersion(int UserKey, string Version)
        {
            int intRetVal = 0;

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);
                _cmd.Parameters.AddWithValue("@Version", Version);
                _cmd.CommandTimeout = 180;
                SqlHelper.ExecuteNonQuery(ref _cmd, "USP_MANAGE_VERSION_LOG");



                intRetVal = 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return intRetVal;
        }

        public int fnSaveDocuments(byte[] File, string Caller, string FileName, string MobileNo, string FileType, string Name)
        {

            int i = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@File", File);
                com.Parameters.AddWithValue("@FileName", FileName);
                com.Parameters.AddWithValue("@Name", Name);
                com.Parameters.AddWithValue("@MobileNo", MobileNo);
                com.Parameters.AddWithValue("@FileType", FileType);
                com.Parameters.AddWithValue("@UserKey", Caller);

                SqlHelper.ExecuteNonQuery(ref com, "[USP_INSERT_CLIENT_DOCUMENT_API]");
                i = 1;
            }
            catch (Exception)
            {
                i = 0;
            }
            return i;
        }

        public int fnSaveClientInfo(string fullName, string email, string mobileNo, string pinCode, string loanType)
        {
            int i = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@fullName", fullName);
                com.Parameters.AddWithValue("@email", email);
                com.Parameters.AddWithValue("@Name", pinCode);
                com.Parameters.AddWithValue("@MobileNo", mobileNo);
                com.Parameters.AddWithValue("@FileType", loanType);

                SqlHelper.ExecuteNonQuery(ref com, "[USP_INSERT_CLIENT_INFO_API]");
                i = 1;
          }
            catch (Exception)
            {
                i = 0;
            }
            return i;
        }

        public List<ClientDocument> GetClientDocuments(int UserKey, string MobileNo)
        {
            List<ClientDocument> lst = new List<ClientDocument>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_CLIENT_DOCUMENT_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ClientDocument objClientDocument = new ClientDocument();

                    objClientDocument.FileName = Convert.ToString(dt.Rows[i]["FileName"]);
                    objClientDocument.Name = Convert.ToString(dt.Rows[i]["Name"]);
                    objClientDocument.Id = Convert.ToInt32(dt.Rows[i]["Id"]);
                    objClientDocument.FileType = Convert.ToString(dt.Rows[i]["FileType"]);
                    objClientDocument.CreatedOn = Convert.ToDateTime(dt.Rows[i]["CreatedOn"]);
                    byte[] fileBytes = (byte[])dt.Rows[i]["File"];
                    objClientDocument.FileBase64String = Convert.ToBase64String(fileBytes);
                    objClientDocument.MobileNo = Convert.ToString(dt.Rows[i]["MobileNo"]);

                    lst.Add(objClientDocument);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<ClientDocument> GetCallerClients(int UserKey, string MobileNo)
        {
            List<ClientDocument> lst = new List<ClientDocument>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_CALLER_CLIENT_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ClientDocument objClientDocument = new ClientDocument();

                    objClientDocument.MobileNo = Convert.ToString(dt.Rows[i]["MobileNo"]);
                    objClientDocument.Name = Convert.ToString(dt.Rows[i]["Name"]);

                    lst.Add(objClientDocument);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Eligibility> LoanEligibility(LoanEligibility.Header objLoanEligibility)
        {
            List<Eligibility> lstEligibility = new List<Eligibility>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.AddWithValue("@NAME", objLoanEligibility.EH_NAME);
                _cmd.Parameters.AddWithValue("@GROSS", objLoanEligibility.EH_NET_GROSS);
                _cmd.Parameters.AddWithValue("@NET", objLoanEligibility.EH_NET_SALARY);
                _cmd.Parameters.AddWithValue("@COMPANY_KEY", objLoanEligibility.EH_COMPANY_NAME_KEY);
                _cmd.Parameters.AddWithValue("@ROI", objLoanEligibility.EH_ROI);
                _cmd.Parameters.AddWithValue("@TENURE", objLoanEligibility.EH_TENURE);
                _cmd.Parameters.AddWithValue("@CATEGORY", objLoanEligibility.EH_CATEGORY);
                _cmd.Parameters.AddWithValue("@OBLIGATION", objLoanEligibility.EH_OBLIGATION);
                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_CIP_LOAN_ELIGIBILITY]");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (Convert.ToString(dt.Rows[i]["Bank"]) != "")
                    {
                        lstEligibility.Add(new Eligibility
                        {
                            Amt = Convert.ToString(dt.Rows[i]["Amt"]),
                            Slab = Convert.ToString(dt.Rows[i]["Slab"]),
                            Bank = Convert.ToString(dt.Rows[i]["Bank"]),
                            Category = Convert.ToString(dt.Rows[i]["Category"]),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lstEligibility;

        }

        public List<PickupAPi.Models.ListItem> ListCompanyCategory(string value)
        {
            List<PickupAPi.Models.ListItem> lst = new List<PickupAPi.Models.ListItem>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;

                _cmd.Parameters.AddWithValue("@PARAM", value);

                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_CIP_FILL_COMPANY_CATEGORY");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    lst.Add(new PickupAPi.Models.ListItem
                    {
                        Value = Convert.ToInt32(dt.Rows[i][0]),
                        Text = Convert.ToString(dt.Rows[i][1]),
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }


        public List<SpinWheelConfig> GetSpinConfig(int UserKey)
        {
            List<SpinWheelConfig> lstApplication = new List<SpinWheelConfig>();
            try
            {
                DataSet ds = new DataSet();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;

                
                _cmd.Parameters.Add(new SqlParameter("@UserKey", UserKey));
                

                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_SPIN_WHEEL_CONFIG");

                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        SpinWheelConfig objapp = new SpinWheelConfig();
                        objapp.id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        objapp.description = Convert.ToString(ds.Tables[0].Rows[i]["Description"]);
                        objapp.fillStyle = Convert.ToString(ds.Tables[0].Rows[i]["FillStyle"]);
                        objapp.amount = Convert.ToDouble(ds.Tables[0].Rows[i]["Amount"]);
                        objapp.CreatedOn = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreatedOn"]);
                        objapp.chance = Convert.ToDouble(ds.Tables[0].Rows[i]["RATIO"]);
                        objapp.IsActive = ds.Tables[0].Rows[i]["IsActive"] == "1" ? true : false;
                        objapp.textFontWeight = "";
                        objapp.textFontSize = "";
                        objapp.textColor = "";
                        lstApplication.Add(objapp);                    
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstApplication;
        }

        public int GetSpinCount(int UserKey)
        {
            int GetSpinCount = 0;
            try
            {
                DataSet ds = new DataSet();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;


                _cmd.Parameters.Add(new SqlParameter("@UserKey", UserKey));


                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_SPIN_WHEEL_REMAINING_COUNT");

                if (ds.Tables[0].Rows.Count > 0)
                {
                    GetSpinCount = Convert.ToInt32(ds.Tables[0].Rows[0]["RemainingCount"]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return GetSpinCount;
        }




    }


}