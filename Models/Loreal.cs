using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using DialerAPi.Models;
using System.Configuration;
using PickupAPi.Models;
using System.IO;

namespace PickupAPi.Models
{
    public class Loreal
    {
        public List<Employee> LoginCheck(string UserName, string UUID)
        {

            List<Employee> lstEmployee = new List<Employee>();

            DataSet ds = null;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@UserName", UserName);
                //com.Parameters.AddWithValue("@Password", Password);
                com.Parameters.AddWithValue("@UUID", UUID);
                //com.Parameters.AddWithValue("@Type", Type);

                ds = SqlHelper.ExecuteDS(ref com, "USP_VENDOR_EMPLOYEE_DTL_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Employee objEmployee = new Employee();
                    lstEmployee.Add(new Employee
                    {
                        Key = Convert.ToInt32(dt.Rows[i]["Key"]),
                        EmployeeNo = Convert.ToInt32(dt.Rows[i]["EmployeeNo"]),
                        EmployeeName = Convert.ToString(dt.Rows[i]["EmployeeName"]),
                        UserName = Convert.ToString(dt.Rows[i]["UserName"]),
                        Password = Convert.ToString(dt.Rows[i]["Password"]),
                        DesignationID = Convert.ToInt32(dt.Rows[i]["DesignationID"]),
                        DesignationDesc = Convert.ToString(dt.Rows[i]["DesignationDesc"]),
                        RoleID = Convert.ToInt64(dt.Rows[i]["RoleID"]),
                        RoleDesc = Convert.ToString(dt.Rows[i]["RoleDesc"]),
                        Salary = Convert.ToDecimal(dt.Rows[i]["Salary"]),
                        BossId = Convert.ToInt64(dt.Rows[i]["BossId"]),
                        ReportsToName = Convert.ToString(dt.Rows[i]["ReportsToName"]),
                        EmailId = Convert.ToString(dt.Rows[i]["EmailId"]),
                        ProgrammeManagerID = Convert.ToInt64(dt.Rows[i]["ProgrammeManagerID"]),
                        ProgrammeManagerName = Convert.ToString(dt.Rows[i]["ProgrammeManagerName"]),
                        ReportingManagerID = Convert.ToInt64(dt.Rows[i]["ReportingManagerID"]),
                        ReportingManagerName = Convert.ToString(dt.Rows[i]["ReportingManagerName"]),
                        BankAccountNo = Convert.ToString(dt.Rows[i]["BankAccountNo"]),
                        LocationDesc = Convert.ToString(dt.Rows[i]["LocationDesc"]),
                        ReportsTo = Convert.ToInt64(dt.Rows[i]["BossId"]),
                        DateOfJoining = Convert.ToString(dt.Rows[i]["DateOfJoining"]),
                        DateOfBirth = Convert.ToString(dt.Rows[i]["DateOfBirth"]),
                        Address = Convert.ToString(dt.Rows[i]["Address"]),
                        OfficeContactNo = Convert.ToInt64(dt.Rows[i]["OfficeContactNo"]),
                        MobileNo1 = Convert.ToInt64(dt.Rows[i]["MobileNo1"]),
                        MobileNo2 = Convert.ToInt64(dt.Rows[i]["MobileNo2"]),
                        AadharNo = Convert.ToString(dt.Rows[i]["AadharNo"]),
                        PanNo = Convert.ToString(dt.Rows[i]["PanNo"]),

                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstEmployee;
        }

        public List<LiveCount> FetchLiveCount(int UserKey)
        {
            List<LiveCount> lst = new List<LiveCount>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);

                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_VENDOR_DIALER_LIVE_COUNT_API]");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    LiveCount objDialer = new LiveCount();
                    objDialer.Lead = Convert.ToInt32(dt.Rows[i]["Lead"]);
                    objDialer.Ringing = Convert.ToInt32(dt.Rows[i]["Ringing"]);
                    objDialer.FollowUp = Convert.ToInt32(dt.Rows[i]["FollowUp"]);
                    objDialer.DND = Convert.ToInt32(dt.Rows[i]["DND"]);
                    objDialer.NI = Convert.ToInt32(dt.Rows[i]["NI"]);

                    objDialer.TotalCount = Convert.ToInt32(dt.Rows[i]["TotalCount"]);
                    objDialer.NotContactable = Convert.ToInt32(dt.Rows[i]["NotContactable"]);
                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public int InsertLeadInfo(DialerAPi.Models.Dialer objDialer, int UserKey)
        {
            int i = 0;
            try
            {
                string connectionString = ConfigurationManager.AppSettings["DBCS"].ToString();
                using (SqlConnection con = new SqlConnection())
                {

                    SqlCommand com = new SqlCommand();
                    com.Connection = con;
                    com.CommandType = CommandType.StoredProcedure;

                    com.Parameters.AddWithValue("@CD_CLIENT_NAME", objDialer.CD_CLIENT_NAME);
                    com.Parameters.AddWithValue("@CD_CONTACT", objDialer.CD_CONTACT);
                    com.Parameters.AddWithValue("@CD_PRODUCT", objDialer.CD_PRODUCT);
                    com.Parameters.AddWithValue("@CD_ADDRESS", objDialer.CD_ADDRESS);
                    com.Parameters.AddWithValue("@CD_PINCODE", objDialer.CD_PINCODE);
                    com.Parameters.AddWithValue("@CD_REMARKS", objDialer.CD_REMARKS);
                    com.Parameters.AddWithValue("@CD_STATUS", objDialer.CD_STATUS);
                    com.Parameters.AddWithValue("@CD_CALLBACK_DATE", objDialer.CD_CALLBACK_DATE);
                    com.Parameters.AddWithValue("@CD_CALLBACK_TIME", objDialer.CD_CALLBACK_TIME);
                    com.Parameters.AddWithValue("@CD_CALLER_DURATION", objDialer.CD_CALLER_DURATION);
                    com.Parameters.AddWithValue("@CD_CALL_DATE", objDialer.CD_CALL_DATE_TIME);
                    com.Parameters.AddWithValue("@UserKey", UserKey);
                    //SqlHelper.ExecuteNonQuery(ref com, "[USP_VENDOR_INSERT_LEAD_MANUAL_API]");

                    con.Open();
                    i = (int)com.ExecuteScalar();


                    if (con.State == System.Data.ConnectionState.Open) con.Close();
                    return i;



                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return i;
        }



        public int UpdateDialer(int Key, string Status, string Remarks, string CallBackTime, string CallBackDate, string CallDuration, string CallDate, int UserKey)
        {
            int i = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@Key", Key);
                com.Parameters.AddWithValue("@Status", Status);
                com.Parameters.AddWithValue("@Remarks", Remarks);
                com.Parameters.AddWithValue("@CallBackDate", CallBackDate);
                com.Parameters.AddWithValue("@CallBackTime", CallBackTime);
                com.Parameters.AddWithValue("@CallDuration", CallDuration);
                com.Parameters.AddWithValue("@CallDate", CallDate);
                com.Parameters.AddWithValue("@UserKey", UserKey);
                SqlHelper.ExecuteNonQuery(ref com, "[USP_VENDOR_UPDATE_DIALER_STATUS_API]");
                i = 1;
            }
            catch (Exception)
            {
                i = 0;
            }
            return i;
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
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_VENDOR_GET_LEAD_N_FOLLOWUP_DATA_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Dialer objDialer = new Dialer();
                    objDialer.CD_KEY = Convert.ToInt32(dt.Rows[i]["CD_KEY"]);
                    objDialer.CD_CLIENT_NAME = Convert.ToString(dt.Rows[i]["CD_CLIENT_NAME"]);
                    objDialer.CD_CONTACT = Convert.ToString(dt.Rows[i]["CD_CONTACT"]);
                    objDialer.CD_PRODUCT = Convert.ToString(dt.Rows[i]["CD_PRODUCT"]);
                    objDialer.CD_COMPANY = Convert.ToString(dt.Rows[i]["CD_COMPANY"]);
                    objDialer.CD_STATUS = Convert.ToString(dt.Rows[i]["CD_STATUS"]);
                    objDialer.CD_ASSIGNED_TO = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO"]);
                    objDialer.CD_ASSIGNED_TO_NAME = Convert.ToString(dt.Rows[i]["CD_ASSIGNED_TO_NAME"]);
                    objDialer.CD_CALLBACK_DATE = Convert.ToString(dt.Rows[i]["CD_CALLBACK_DATE"]);
                    objDialer.CD_CALLBACK_TIME = Convert.ToString(dt.Rows[i]["CD_CALLBACK_TIME"]);
                    objDialer.CD_CALLER_REMARKS = Convert.ToString(dt.Rows[i]["CD_CALLER_REMARKS"]);
                    objDialer.CD_PRODUCT_Desc = Convert.ToString(dt.Rows[i]["CD_PRODUCT_DESC"]);
                    lst.Add(objDialer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }


        

    }
}