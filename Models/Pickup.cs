using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using PickupAPi.Models;
using System.Configuration;
using System.IO;
using System.Web.Http.Results;
using System.Threading.Tasks;
using DialerAPi.Models;
using System.Reflection;

namespace PickupAPi.Models
{

    public class Incentive
    {
        public string PointsPL { get; set; }
        public string ReferralsPL { get; set; }
        public string ReferralAmt { get; set; }
        public string PLMP { get; set; }
        public string PLNMP { get; set; }
        public string HL { get; set; }
        public string BL { get; set; }
        public string CC { get; set; }
        public string APLMP { get; set; }
        public string APLNMP { get; set; }
        public string HLAmt { get; set; }
        public string BLAmt { get; set; }
        public string CCAmt { get; set; }

        public string Incentives { get; set; }

    }

    public class Application
    {
        public int Id { get; set; }
        public string AppDate { get; set; }
        public string ClientName { get; set; }
        public string Bank { get; set; }
        public string CPVStatus { get; set; }
        public string DisbAmt { get; set; }
        public string DisbDate { get; set; }
        public string LoginAmt { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string Caller { get; set; }
        public string Product { get; set; }
        public string AppNo { get; set; }
        public string LosNo { get; set; }
        public string UpdatedOn { get; set; }
    }

    public class DailyLogin
    {
        public int Id { get; set; }
        public string Client { get; set; }
        public string Banks { get; set; }
        public string LoginAmt { get; set; }
        public string Remarks { get; set; }
        public string Caller { get; set; }
        public string Product { get; set; }
        public string LosNo { get; set; }
        public string CreatedOn { get; set; }
        public string Region { get; set; }

    }

    public class ListItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class Attachment
    {
        public int PickupKey { get; set; }
        public String Image { get; set; }
    }


    public class UpdateStatus
    {
        public int Key { get; set; }
        public string Status { get; set; }
        public string CallerRemarks { get; set; }
        public string CallBackDate { get; set; }
        public string CallBackTime { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public String Image { get; set; }
    }

    public class Pickup
    {
        public int PickupKey { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public string PickupType { get; set; }
        public string ProductDesc { get; set; }
        public string Remarks { get; set; }
        public string CallerRemarks { get; set; }
        public string CallerUserName { get; set; }
        public string PickupTimeFrom { get; set; }
        public string PickupTimeTo { get; set; }
        public string CallerLocation { get; set; }
        public string TLName { get; set; }
        public string CallerTLNo { get; set; }
        public string BanksDesc { get; set; }
        public string AssignedOn { get; set; }
        public string PickupDate { get; set; }
        public string Product { get; set; }
        public string CallerUserID { get; set; }
        public string Banks { get; set; }
        public string LoginRegion { get; set; }
        public string PickupMobileNO { get; set; }

        public string PushID { get; set; }

        public string PickupExecutive { get; set; }
        public string PickupExecutiveNo { get; set; }
        public string Status { get; set; }
        public int UpdateStatus(UpdateStatus LM)
        {

            int i;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                //    //obj.CallerRemarks = CallerRemarks;
                //    //obj.Status = Status;
                //    //obj.Key = Convert.ToInt32(Key);
                //    //obj.CallBackDate = CallBackDate;
                //    //obj.CallBackTime = CallBackTime;
                //    //obj.Latitude = Lat;
                //    //obj.Longitude = Long;
                //    //obj.Image = base64;


                com.Parameters.AddWithValue("@Key", LM.Key);
                com.Parameters.AddWithValue("@Status", LM.Status);
                com.Parameters.AddWithValue("@CallerRemarks", LM.CallerRemarks);
                com.Parameters.AddWithValue("@CallBackDate", LM.CallBackDate);
                com.Parameters.AddWithValue("@CallBackTime", LM.CallBackTime);
                com.Parameters.AddWithValue("@LAT", LM.Latitude);
                com.Parameters.AddWithValue("@LONG", LM.Longitude);
                com.Parameters.AddWithValue("@IMAGE", LM.Image);

                i = SqlHelper.ExecuteNonQuery(ref com, "USP_CIP_UPDATE_PICKUP_REMARKS_API");
            }
            catch (Exception)
            {

                throw;
            }
            return i;
        }

        public int Manage(Pickup LM, int UserKey)
        {
            int i;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@LM_KEY", LM.PickupKey);
                com.Parameters.AddWithValue("@LM_NAME", LM.Name);
                com.Parameters.AddWithValue("@LM_PICKUP_DATE", LM.PickupDate);
                com.Parameters.AddWithValue("@LM_PICKUP_TYPE", LM.PickupType);
                com.Parameters.AddWithValue("@LM_CONTACT_NO", LM.MobileNo);
                com.Parameters.AddWithValue("@LM_ADDRESS", LM.Address);
                com.Parameters.AddWithValue("@LM_PRODUCT", LM.Product);
                com.Parameters.AddWithValue("@LM_CALLER_USERID", LM.CallerUserID);
                com.Parameters.AddWithValue("@LM_REMARKS", LM.Remarks);
                com.Parameters.AddWithValue("@LM_PICKUP_TIME_FROM", LM.PickupTimeFrom);
                com.Parameters.AddWithValue("@LM_PICKUP_TIME_TO", LM.PickupTimeTo);
                com.Parameters.AddWithValue("@LM_BRANCH_KEY", LM.Banks);
                com.Parameters.AddWithValue("@LM_LOGIN_REGION", LM.LoginRegion);
                com.Parameters.AddWithValue("@LM_CREATED_BY", UserKey);

                i = SqlHelper.ExecuteNonQuery(ref com, "USP_MANAGE_Pickup_MASTER_API");
            }
            catch (Exception)
            {
                throw;
            }
            return i;
        }

        public string GetPushID(int UserKey)
        {
            string PushID = "";
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@UserKey", UserKey);

                DataSet ds = SqlHelper.ExecuteDS(ref com, "USP_FETCH_PUSH_ID_API");

                DataTable dt = ds.Tables[0];

                if (dt != null)
                {
                    PushID = Convert.ToString(dt.Rows[0][0]);
                }


            }
            catch (Exception)
            {

                throw;
            }
            return PushID;
        }



        public List<Pickup> FetchPendingPickup(string MobileNo)
        {
            List<Pickup> lst = new List<Pickup>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("MobileNo", MobileNo);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_FETCH_PICKUP_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Pickup objPickup = new Pickup();

                    objPickup.PickupKey = Convert.ToInt32(dt.Rows[i]["PickupKey"]);
                    objPickup.Name = Convert.ToString(dt.Rows[i]["Name"]);
                    objPickup.Address = Convert.ToString(dt.Rows[i]["Address"]);
                    objPickup.MobileNo = Convert.ToString(dt.Rows[i]["MobileNo"]);
                    objPickup.Remarks = Convert.ToString(dt.Rows[i]["Remarks"]);
                    objPickup.ProductDesc = Convert.ToString(dt.Rows[i]["ProductDesc"]);
                    objPickup.CallerUserName = Convert.ToString(dt.Rows[i]["CallerUserName"]);
                    objPickup.AssignedOn = Convert.ToString(dt.Rows[i]["AssignedOn"]);
                    objPickup.PickupTimeFrom = Convert.ToString(dt.Rows[i]["PickupTimeFrom"]);
                    objPickup.PickupTimeTo = Convert.ToString(dt.Rows[i]["PickupTimeTo"]);
                    objPickup.BanksDesc = Convert.ToString(dt.Rows[i]["BanksDesc"]);
                    objPickup.CallerLocation = Convert.ToString(dt.Rows[i]["CallerLocation"]);
                    objPickup.TLName = Convert.ToString(dt.Rows[i]["TLName"]);
                    objPickup.PickupType = Convert.ToString(dt.Rows[i]["PickupType"]);
                    objPickup.CallerTLNo = Convert.ToString(dt.Rows[i]["CallerTLNo"]);
                    objPickup.PickupDate = Convert.ToString(dt.Rows[i]["PickupDate"]);
                    objPickup.PushID = Convert.ToString(dt.Rows[i]["PushID"]);
                    lst.Add(objPickup);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Pickup> FetchPickupCompleted()
        {
            List<Pickup> lst = new List<Pickup>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();

                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_FETCH_PICKUP_COMPLETED_DATA");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Pickup objPickup = new Pickup();

                    objPickup.PickupKey = Convert.ToInt32(dt.Rows[i]["PickupKey"]);
                    objPickup.Name = Convert.ToString(dt.Rows[i]["Name"]);
                    objPickup.Address = Convert.ToString(dt.Rows[i]["Address"]);
                    objPickup.MobileNo = Convert.ToString(dt.Rows[i]["MobileNo"]);
                    objPickup.Remarks = Convert.ToString(dt.Rows[i]["Remarks"]);
                    objPickup.ProductDesc = Convert.ToString(dt.Rows[i]["ProductDesc"]);
                    objPickup.CallerUserName = Convert.ToString(dt.Rows[i]["CallerUserName"]);
                    objPickup.AssignedOn = Convert.ToString(dt.Rows[i]["AssignedOn"]);
                    objPickup.PickupTimeFrom = Convert.ToString(dt.Rows[i]["PickupTimeFrom"]);
                    objPickup.PickupTimeTo = Convert.ToString(dt.Rows[i]["PickupTimeTo"]);
                    objPickup.BanksDesc = Convert.ToString(dt.Rows[i]["BanksDesc"]);
                    objPickup.CallerLocation = Convert.ToString(dt.Rows[i]["CallerLocation"]);
                    objPickup.TLName = Convert.ToString(dt.Rows[i]["TLName"]);
                    objPickup.PickupType = Convert.ToString(dt.Rows[i]["PickupType"]);
                    objPickup.CallerTLNo = Convert.ToString(dt.Rows[i]["CallerTLNo"]);
                    objPickup.PickupDate = Convert.ToString(dt.Rows[i]["PickupDate"]);
                    objPickup.CallerRemarks = Convert.ToString(dt.Rows[i]["CallerRemarks"]);
                    lst.Add(objPickup);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Pickup> FetchPickupDataNew(int UserKey, int Type)
        {
            List<Pickup> lst = new List<Pickup>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.Parameters.AddWithValue("@TYPE", Type);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_FETCH_PICKUP_DATA_ENTRY_API_NEW");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Pickup objPickup = new Pickup();

                    objPickup.PickupKey = Convert.ToInt32(dt.Rows[i]["PickupKey"]);
                    objPickup.Name = Convert.ToString(dt.Rows[i]["Name"]);
                    objPickup.Address = Convert.ToString(dt.Rows[i]["Address"]);
                    objPickup.MobileNo = Convert.ToString(dt.Rows[i]["MobileNo"]);
                    objPickup.Remarks = Convert.ToString(dt.Rows[i]["Remarks"]);
                    objPickup.ProductDesc = Convert.ToString(dt.Rows[i]["ProductDesc"]);
                    objPickup.CallerUserName = Convert.ToString(dt.Rows[i]["CallerUserName"]);
                    objPickup.AssignedOn = Convert.ToString(dt.Rows[i]["AssignedOn"]);
                    objPickup.PickupTimeFrom = Convert.ToString(dt.Rows[i]["PickupTimeFrom"]);
                    objPickup.PickupTimeTo = Convert.ToString(dt.Rows[i]["PickupTimeTo"]);
                    objPickup.BanksDesc = Convert.ToString(dt.Rows[i]["BanksDesc"]);
                    objPickup.CallerLocation = Convert.ToString(dt.Rows[i]["CallerLocation"]);
                    objPickup.TLName = Convert.ToString(dt.Rows[i]["TLName"]);
                    objPickup.PickupType = Convert.ToString(dt.Rows[i]["PickupType"]);
                    objPickup.CallerTLNo = Convert.ToString(dt.Rows[i]["CallerTLNo"]);
                    objPickup.PickupDate = Convert.ToString(dt.Rows[i]["PickupDate"]);

                    objPickup.PickupExecutive = Convert.ToString(dt.Rows[i]["PickupName"]);
                    objPickup.PickupExecutiveNo = Convert.ToString(dt.Rows[i]["PickupContact"]);
                    objPickup.Status = Convert.ToString(dt.Rows[i]["Status"]);


                    lst.Add(objPickup);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<Pickup> FetchPickupData(int UserKey)
        {
            List<Pickup> lst = new List<Pickup>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_FETCH_PICKUP_DATA_ENTRY_API");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Pickup objPickup = new Pickup();

                    objPickup.PickupKey = Convert.ToInt32(dt.Rows[i]["PickupKey"]);
                    objPickup.Name = Convert.ToString(dt.Rows[i]["Name"]);
                    objPickup.Address = Convert.ToString(dt.Rows[i]["Address"]);
                    objPickup.MobileNo = Convert.ToString(dt.Rows[i]["MobileNo"]);
                    objPickup.Remarks = Convert.ToString(dt.Rows[i]["Remarks"]);
                    objPickup.ProductDesc = Convert.ToString(dt.Rows[i]["ProductDesc"]);
                    objPickup.CallerUserName = Convert.ToString(dt.Rows[i]["CallerUserName"]);
                    objPickup.AssignedOn = Convert.ToString(dt.Rows[i]["AssignedOn"]);
                    objPickup.PickupTimeFrom = Convert.ToString(dt.Rows[i]["PickupTimeFrom"]);
                    objPickup.PickupTimeTo = Convert.ToString(dt.Rows[i]["PickupTimeTo"]);
                    objPickup.BanksDesc = Convert.ToString(dt.Rows[i]["BanksDesc"]);
                    objPickup.CallerLocation = Convert.ToString(dt.Rows[i]["CallerLocation"]);
                    objPickup.TLName = Convert.ToString(dt.Rows[i]["TLName"]);
                    objPickup.PickupType = Convert.ToString(dt.Rows[i]["PickupType"]);
                    objPickup.CallerTLNo = Convert.ToString(dt.Rows[i]["CallerTLNo"]);
                    objPickup.PickupDate = Convert.ToString(dt.Rows[i]["PickupDate"]);

                    objPickup.PickupExecutive = Convert.ToString(dt.Rows[i]["PickupName"]);
                    objPickup.PickupExecutiveNo = Convert.ToString(dt.Rows[i]["PickupContact"]);
                    objPickup.Status = Convert.ToString(dt.Rows[i]["Status"]);


                    lst.Add(objPickup);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public async Task<string> UpdateDialingStatusAsync(int Key)
        {
            string strMsg = "";
            string connectionString = ConfigurationManager.AppSettings["DBCS"]; // Replace with your actual connection string

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(); // Open the connection asynchronously

                    using (SqlCommand command = new SqlCommand("USP_UPDATE_DIALER_STATUS_API", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Key", Key);

                        await command.ExecuteNonQueryAsync(); // Execute the command asynchronously
                        strMsg = "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
            }

            return strMsg;
        }




        //public string UpdateDialingStatus(int Key)
        //{
        //    int i = 0;
        //    string strMsg = "";
        //    try
        //    {
        //        SqlCommand com = new SqlCommand();
        //        com.CommandType = CommandType.StoredProcedure;

        //        com.Parameters.AddWithValue("@Key", Key);

        //        SqlHelper.ExecuteNonQuery(ref com, "USP_UPDATE_DIALER_STATUS_API");
        //        strMsg = "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        strMsg = ex.Message;
        //    }
        //    return strMsg;
        //}

        public string UpdateDiallingTime(int Key)
        {
            int i = 0;
            string strMsg = "";
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@Key", Key);

                SqlHelper.ExecuteNonQuery(ref com, "USP_UPDATE_DIALLING_TIME_API");
                strMsg = "Success";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
            }
            return strMsg;



        }


        public List<ListItem> FetchExecutives(int UserKey)
        {
            List<ListItem> lst = new List<ListItem>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("@USER_KEY", UserKey);
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_CIP_GET_pickup_exec_api");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    ListItem objPickup = new ListItem();


                    objPickup.Text = Convert.ToString(dt.Rows[i]["TEXT"]);
                    objPickup.Value = Convert.ToInt32(dt.Rows[i]["VALUE"]);


                    lst.Add(objPickup);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public List<ListItem> FetchOffer()
        {
            List<ListItem> lst = new List<ListItem>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_CIP_VIEW_OFFER");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    ListItem objPickup = new ListItem();


                    objPickup.Text = Convert.ToString(dt.Rows[i][0]);


                    lst.Add(objPickup);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }



        public string ValidateLogin(string MobileNo, string UDID, string Type)
        {
            string strReturn = "";
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("MobileNo", MobileNo);
                _cmd.Parameters.AddWithValue("UDID", UDID);
                _cmd.Parameters.AddWithValue("Type", Type);
                _cmd.CommandTimeout = 180;
                DataSet ds = SqlHelper.ExecuteDS(ref _cmd, "USP_VALIDATE_LOGIN_API");

                DataTable dt = ds.Tables[0];

                if (dt != null && dt.Rows.Count > 0)
                {
                    strReturn = Convert.ToString(dt.Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                strReturn = "";
            }
            return strReturn;
        }

        public string UpdateStatus1(Int64 Key, String Image)
        {
            string strReturn = "";
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.Parameters.AddWithValue("Key", Key);
                _cmd.Parameters.AddWithValue("Image", Image);
                _cmd.CommandTimeout = 180;
                DataSet ds = SqlHelper.ExecuteDS(ref _cmd, "USP_UPDATE_IMAGE_API");

                DataTable dt = ds.Tables[0];

                if (dt != null && dt.Rows.Count > 0)
                {
                    strReturn = Convert.ToString(dt.Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                strReturn = "";
            }
            return strReturn;
        }

        public int AssignPickup(int PickupKey, int AssignedTo, int AssignedBy)
        {
            int i;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@Key", PickupKey);
                com.Parameters.AddWithValue("@AssignedTo", AssignedTo);
                com.Parameters.AddWithValue("@AssignedBy", AssignedBy);
                i = SqlHelper.ExecuteNonQuery(ref com, "[USP_CIP_UPDATE_Pickup_ASSIGNMENT]");
            }
            catch (Exception)
            {

                throw;
            }
            return i;
        }


        public List<ListItem> GetPickup(int UserKey)
        {
            List<ListItem> lst = new List<ListItem>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);

                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_CIP_GET_PICKUP_EMPLOYEE_LIST");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    lst.Add(new ListItem
                    {
                        Value = Convert.ToInt32(dt.Rows[i]["KEY"]),
                        Text = Convert.ToString(dt.Rows[i]["VALUE"]),
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }



        public List<ListItem> GetCaller(int UserKey)
        {
            List<ListItem> lst = new List<ListItem>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);

                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_CIP_GET_EMPLOYEE_LIST");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    lst.Add(new ListItem
                    {
                        Value = Convert.ToInt32(dt.Rows[i]["KEY"]),
                        Text = Convert.ToString(dt.Rows[i]["VALUE"]),
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }


        public List<ListItem> GetProduct(int UserKey)
        {
            List<ListItem> lst = new List<ListItem>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.AddWithValue("@UserKey", UserKey);

                ds = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_PRODUCT");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    lst.Add(new ListItem
                    {
                        Value = Convert.ToInt32(dt.Rows[i]["KEY"]),
                        Text = Convert.ToString(dt.Rows[i]["VALUE"]),
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }

        public string InsertAttachment(List<Attachment> lstAttachment)
        {
            string strMsg = "";
            try
            {
                for (int i = 0; i < lstAttachment.Count; i++)
                {
                    SqlCommand com = new SqlCommand();
                    com.CommandType = CommandType.StoredProcedure;
                    com.CommandText = "USP_INSERT_PICKUP_ATTACHMENTS";
                    com.Parameters.AddWithValue("@PA_PICKUP_KEY", lstAttachment[i].PickupKey);
                    com.Parameters.AddWithValue("@PA_IMAGE_STRING", lstAttachment[i].Image);
                    SqlHelper.ExecuteNonQuery(ref com, "[USP_INSERT_PICKUP_ATTACHMENTS]");
                }

                strMsg = "SuccessFul";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
            }
            return strMsg;

            //SqlCommand com = new SqlCommand();
            //int iRetVal = 0;
            //using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.AppSettings["DBCS"].ToString()))
            //{
            //    sqlConnection.Open();
            //    SqlTransaction transaction = sqlConnection.BeginTransaction();                
            //    try
            //    {
            //        for (int i = 0; i < lstAttachment.Count; i++)
            //        {
            //            com = new SqlCommand();
            //            com.CommandType = CommandType.StoredProcedure;
            //            com.CommandText = "USP_INSERT_PICKUP_ATTACHMENTS";
            //            com.Parameters.AddWithValue("@PA_PICKUP_KEY", lstAttachment[i].PickupKey);
            //            com.Parameters.AddWithValue("@PA_IMAGE_STRING", lstAttachment[i].Image);
            //            com.ExecuteNonQuery();
            //        }
            //        transaction.Commit();
            //        iRetVal = 1;
            //        //i = SqlHelper.ExecuteNonQuery(ref com, "[USP_INSERT_PICKUP_ATTACHMENTS]");
            //    }
            //    catch (Exception)
            //    {
            //        transaction.Rollback();
            //        iRetVal = -1;
            //    }
            //}
            //return iRetVal;
        }


        public List<Employee> LoginCheck(string UserName, string Password, string UUID, string Type)
        {

            List<Employee> lstEmployee = new List<Employee>();

            DataSet ds = null;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@UserName", UserName);
                com.Parameters.AddWithValue("@Password", Password);
                com.Parameters.AddWithValue("@UUID", UUID);
                com.Parameters.AddWithValue("@Type", Type);

                ds = SqlHelper.ExecuteDS(ref com, "USP_CIP_GET_EMPLOYEE_LOGIN_DETAIL_API");

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
            catch (Exception)
            {
                throw;
            }
            return lstEmployee;
        }


        #region Dialer

        public async Task<int> UpdateDialerAsync(int Key, string Status, string Remarks, string CallBackTime,
    string CallBackDate, string CallDuration, string CallDate, int UserKey,
    int IsHLLAPBL, int IsCibilBad, int IsRealEstate)
        {
            int retryCount = 3;
            int delayMs = 200;

            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandTimeout = 30;
                        cmd.Parameters.AddWithValue("@Key", Key);
                        cmd.Parameters.AddWithValue("@Status", Status);
                        cmd.Parameters.AddWithValue("@Remarks", (object)Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CallBackTime", (object)CallBackTime ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CallBackDate", (object)CallBackDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CallDuration", (object)CallDuration ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CallDate", (object)CallDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@UserKey", UserKey);
                        cmd.Parameters.AddWithValue("@IsHLLAPBL", IsHLLAPBL);
                        cmd.Parameters.AddWithValue("@IsCibilBad", IsCibilBad);
                        cmd.Parameters.AddWithValue("@IsRealEstate", IsRealEstate);

                        
                        return await SqlHelper.NewExecuteNonQueryAsync(cmd, "USP_CIP_UPDATE_DIALER_STATUS_API");
                    }
                }
                catch (SqlException ex) when (ex.Number == 1205) // 1205 = deadlock victim
                {
                    if (attempt == retryCount) throw;
                    await Task.Delay(delayMs * attempt);
                }
                catch (Exception ex)
                {
                    string logPath = @"H:\logs\log.txt";
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                                      $"Message: {ex.Message}\r\n" +
                                      $"Stack Trace: {ex.StackTrace}\r\n" +
                                      "----------------------------------------\r\n";

                    string directory = Path.GetDirectoryName(logPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    return 0;
                }
            }
            return 0;
        }

        // public async Task<int> UpdateDialerAsync(int Key, string Status, string Remarks, string CallBackTime,
        //string CallBackDate, string CallDuration, string CallDate, int UserKey,
        //int IsHLLAPBL, int IsCibilBad, int IsRealEstate)
        // {



        //     try
        //     {
        //         using (SqlCommand cmd = new SqlCommand())
        //         {
        //             cmd.Parameters.AddWithValue("@Key", Key);
        //             cmd.Parameters.AddWithValue("@Status", Status);
        //             cmd.Parameters.AddWithValue("@Remarks", (object)Remarks ?? DBNull.Value);
        //             cmd.Parameters.AddWithValue("@CallBackTime", (object)CallBackTime ?? DBNull.Value);
        //             cmd.Parameters.AddWithValue("@CallBackDate", (object)CallBackDate ?? DBNull.Value);
        //             cmd.Parameters.AddWithValue("@CallDuration", (object)CallDuration ?? DBNull.Value);
        //             cmd.Parameters.AddWithValue("@CallDate", (object)CallDate ?? DBNull.Value);
        //             cmd.Parameters.AddWithValue("@UserKey", UserKey);
        //             cmd.Parameters.AddWithValue("@IsHLLAPBL", IsHLLAPBL);
        //             cmd.Parameters.AddWithValue("@IsCibilBad", IsCibilBad);
        //             cmd.Parameters.AddWithValue("@IsRealEstate", IsRealEstate);

        //             SqlHelper sqlHelper = new SqlHelper(); // Create an instance of SqlHelper
        //             return await sqlHelper.NewExecuteNonQueryAsync(cmd, "USP_CIP_UPDATE_DIALER_STATUS_API");
        //         }

        //     }
        //     catch (Exception ex)
        //     {
        //         string logPath = @"H:\logs\log.txt";
        //         string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
        //                           $"Message: {ex.Message}\r\n" +
        //                           $"Stack Trace: {ex.StackTrace}\r\n" +
        //                           "----------------------------------------\r\n";

        //         string directory = Path.GetDirectoryName(logPath);
        //         if (!Directory.Exists(directory))
        //         {
        //             Directory.CreateDirectory(directory);
        //         }
        //         return 0;
        //     }



        // }

        public int UpdateDialer(int Key, string Status, string Remarks, string CallBackTime, string CallBackDate, string CallDuration, string CallDate, int UserKey,
            int IsHLLAPBL, int IsCibilBad, int IsRealEstate
            )
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
                com.Parameters.AddWithValue("@IsHLLAPBL", IsHLLAPBL);
                com.Parameters.AddWithValue("@IsCibilBad", IsCibilBad);
                com.Parameters.AddWithValue("@IsRealEstate", IsRealEstate);
                com.CommandTimeout = 120;


                SqlHelper.ExecuteNonQuery(ref com, "[USP_CIP_UPDATE_DIALER_STATUS_API]");
                i = 1;
            }
            catch (Exception ex)
            {

                using (var file = new StreamWriter(@"H:\logs\" + "log.txt", true))
                {
                    file.WriteLine("Testing");
                    file.WriteLine(ex.Message);
                    file.WriteLine(ex.StackTrace);
                    file.Close();
                }
                i = 0;
            }
            return i;
        }

        public List<LeadMaster> GetLeadList(int intUserKey, string SearchText, int PageSize, int PageIndex)
        {
            List<LeadMaster> lst = new List<LeadMaster>();

            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.AddWithValue("@UserKey", intUserKey);
                _cmd.Parameters.AddWithValue("@SearchText", SearchText);
                _cmd.Parameters.AddWithValue("@PageSize", PageSize);
                _cmd.Parameters.AddWithValue("@PageIndex", PageIndex);

                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_FETCH_LEAD_MASTER_RECORD_API]");

                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    LeadMaster objLeadMaster = new LeadMaster();
                    lst.Add(new LeadMaster
                    {
                        Key = Convert.ToInt32(dt.Rows[i]["LeadKey"]),
                        Name = Convert.ToString(dt.Rows[i]["Name"]),
                        Address = Convert.ToString(dt.Rows[i]["Address"]),
                        MobileNo = Convert.ToString(dt.Rows[i]["MobileNo"]),
                        EmailID = Convert.ToString(dt.Rows[i]["EmailID"]),
                        Remarks = Convert.ToString(dt.Rows[i]["Remarks"]),
                        Product = Convert.ToInt32(dt.Rows[i]["Product"]),
                        ProductDesc = Convert.ToString(dt.Rows[i]["ProductDesc"]),
                        CreatedByName = Convert.ToString(dt.Rows[i]["CreatedByName"]),
                        AssignedBy = Convert.ToInt32(dt.Rows[i]["AssignedBy"]),
                        AssignedByName = Convert.ToString(dt.Rows[i]["AssignedByName"]),
                        CallerUserID = Convert.ToInt32(dt.Rows[i]["CallerUserID"]),
                        CallerUserName = Convert.ToString(dt.Rows[i]["CallerUserName"]),
                        AssignedTo = Convert.ToInt32(dt.Rows[i]["AssignedTo"]),
                        AssignedToName = Convert.ToString(dt.Rows[i]["AssignedToName"]),
                        Status = Convert.ToString(dt.Rows[i]["Status"]),
                        CallerRemarks = Convert.ToString(dt.Rows[i]["CallerRemarks"]),
                        AssignedOn = Convert.ToString(dt.Rows[i]["AssignedOn"]),
                        CreatedDate = Convert.ToString(dt.Rows[i]["CreatedDate"]),
                        LastUpdatedOn = Convert.ToString(dt.Rows[i]["ModifiedOn"]),
                        CreatedBy = Convert.ToInt32(dt.Rows[i]["CreatedBy"]),
                        IsAssignable = Convert.ToInt32(dt.Rows[i]["IsAssignable"]),
                        IsEditable = Convert.ToInt32(dt.Rows[i]["IsEditable"]),
                        CallBackDate = Convert.ToString(dt.Rows[i]["CallBackDate"]),
                        CallBackTime = Convert.ToString(dt.Rows[i]["CallBackTime"])
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }


        public int ManageLead(LeadMaster LM)
        {
            int i;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@LM_KEY", LM.Key);
                com.Parameters.AddWithValue("@LM_NAME", LM.Name);
                com.Parameters.AddWithValue("@LM_EMAIL_ID", LM.EmailID);
                com.Parameters.AddWithValue("@LM_CONTACT_NO", LM.MobileNo);
                com.Parameters.AddWithValue("@LM_ADDRESS", LM.Address);
                com.Parameters.AddWithValue("@LM_PRODUCT", LM.Product);
                com.Parameters.AddWithValue("@LM_CALLER_USERID", LM.CreatedBy);
                com.Parameters.AddWithValue("@LM_CREATED_BY", LM.CreatedBy);
                i = SqlHelper.ExecuteNonQuery(ref com, "USP_MANAGE_LEAD_MASTER");
            }
            catch (Exception)
            {
                throw;
            }
            return i;
        }

        public int InsertLeadInfo(DialerAPi.Models.Dialer objDialer, int UserKey)
        {
            int i = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@CD_CLIENT_NAME", objDialer.CD_CLIENT_NAME);
                com.Parameters.AddWithValue("@CD_CONTACT", objDialer.CD_CONTACT);
                com.Parameters.AddWithValue("@CD_SALARY", objDialer.CD_SALARY);
                com.Parameters.AddWithValue("@CD_COMPANY", objDialer.CD_COMPANY);
                com.Parameters.AddWithValue("@CD_REMARKS", objDialer.CD_REMARKS);
                com.Parameters.AddWithValue("@CD_STATUS", objDialer.CD_STATUS);
                com.Parameters.AddWithValue("@CD_CALLBACK_DATE", objDialer.CD_CALLBACK_DATE);
                com.Parameters.AddWithValue("@CD_CALLBACK_TIME", objDialer.CD_CALLBACK_TIME);
                com.Parameters.AddWithValue("@CD_CALLER_DURATION", objDialer.CD_CALLER_DURATION);
                com.Parameters.AddWithValue("@CD_CALL_DATE", objDialer.CD_CALL_DATE_TIME);
                com.Parameters.AddWithValue("@UserKey", UserKey);
                com.Parameters.AddWithValue("@CD_IS_HLLAPBL", objDialer.CD_IS_HLLAPBL);
                com.Parameters.AddWithValue("@CD_IS_CIBIL_BAD", objDialer.CD_IS_CIBIL_BAD);
                com.Parameters.AddWithValue("@CD_IS_REALESTATE", objDialer.CD_IS_REALESTATE);

                SqlHelper.ExecuteNonQuery(ref com, "[USP_CIP_INSERT_LEAD_MANUAL_API]");
                i = 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return i;
        }

        public async Task<int> InsertLeadInfo_NewAsync(DialerAPi.Models.Dialer objDialer, int UserKey)
        {
            int i = 0;
            try
            {
                string connectionString = ConfigurationManager.AppSettings["DBCS"];
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand com = new SqlCommand("USP_CIP_INSERT_LEAD_MANUAL_API_NEW", conn))
                    {
                        com.CommandType = CommandType.StoredProcedure;

                        com.Parameters.AddWithValue("@CD_CLIENT_NAME", objDialer.CD_CLIENT_NAME);
                        com.Parameters.AddWithValue("@CD_CONTACT", objDialer.CD_CONTACT);
                        com.Parameters.AddWithValue("@CD_SALARY", objDialer.CD_SALARY);
                        com.Parameters.AddWithValue("@CD_COMPANY", objDialer.CD_COMPANY);
                        com.Parameters.AddWithValue("@CD_REMARKS", objDialer.CD_REMARKS);
                        com.Parameters.AddWithValue("@CD_STATUS", objDialer.CD_STATUS);
                        com.Parameters.AddWithValue("@CD_CALLBACK_DATE", objDialer.CD_CALLBACK_DATE);
                        com.Parameters.AddWithValue("@CD_CALLBACK_TIME", objDialer.CD_CALLBACK_TIME);
                        com.Parameters.AddWithValue("@CD_CALLER_DURATION", objDialer.CD_CALLER_DURATION);
                        com.Parameters.AddWithValue("@CD_CALL_DATE", objDialer.CD_CALL_DATE_TIME);
                        com.Parameters.AddWithValue("@UserKey", UserKey);
                        com.Parameters.AddWithValue("@CD_IS_HLLAPBL", objDialer.CD_IS_HLLAPBL);
                        com.Parameters.AddWithValue("@CD_IS_CIBIL_BAD", objDialer.CD_IS_CIBIL_BAD);
                        com.Parameters.AddWithValue("@CD_IS_REALESTATE", objDialer.CD_IS_REALESTATE);

                        var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.Output;
                        com.Parameters.Add(returnParameter);

                        await conn.OpenAsync();
                        await com.ExecuteNonQueryAsync();
                        i = (int)returnParameter.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                // SqlHelper.ErrorLogging(ex.Message); // Ensure proper logging
                throw;
            }

            return i;
        }


        public int InsertLeadInfo_New(DialerAPi.Models.Dialer objDialer, int UserKey)
        {
            int i = 0;
            try
            {
                string connectionString = ConfigurationManager.AppSettings["DBCS"].ToString();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand com = new SqlCommand("USP_CIP_INSERT_LEAD_MANUAL_API_NEW", conn);
                    com.CommandType = CommandType.StoredProcedure;

                    com.Parameters.AddWithValue("@CD_CLIENT_NAME", objDialer.CD_CLIENT_NAME);
                    com.Parameters.AddWithValue("@CD_CONTACT", objDialer.CD_CONTACT);
                    com.Parameters.AddWithValue("@CD_SALARY", objDialer.CD_SALARY);
                    com.Parameters.AddWithValue("@CD_COMPANY", objDialer.CD_COMPANY);
                    com.Parameters.AddWithValue("@CD_REMARKS", objDialer.CD_REMARKS);
                    com.Parameters.AddWithValue("@CD_STATUS", objDialer.CD_STATUS);
                    com.Parameters.AddWithValue("@CD_CALLBACK_DATE", objDialer.CD_CALLBACK_DATE);
                    com.Parameters.AddWithValue("@CD_CALLBACK_TIME", objDialer.CD_CALLBACK_TIME);
                    com.Parameters.AddWithValue("@CD_CALLER_DURATION", objDialer.CD_CALLER_DURATION);
                    com.Parameters.AddWithValue("@CD_CALL_DATE", objDialer.CD_CALL_DATE_TIME);
                    com.Parameters.AddWithValue("@UserKey", UserKey);
                    com.Parameters.AddWithValue("@CD_IS_HLLAPBL", objDialer.CD_IS_HLLAPBL);
                    com.Parameters.AddWithValue("@CD_IS_CIBIL_BAD", objDialer.CD_IS_CIBIL_BAD);
                    com.Parameters.AddWithValue("@CD_IS_REALESTATE", objDialer.CD_IS_REALESTATE);

                    var returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.Output;
                    com.Parameters.Add(returnParameter);

                    conn.Open();
                    com.ExecuteNonQuery();
                    i = (int)returnParameter.Value;
                }
            }
            catch (Exception ex)
            {
                //SqlHelper.ErrorLogging(ex.Message);
                throw ex;
            }

            return i;
        }

        public int AddLead(DialerAPi.Models.Dialer objDialer, int UserKey)
        {
            int i = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@Name", objDialer.CD_CLIENT_NAME);
                com.Parameters.AddWithValue("@Contact", objDialer.CD_CONTACT);
                com.Parameters.AddWithValue("@Product", objDialer.CD_PRODUCT);
                com.Parameters.AddWithValue("@CallerID", objDialer.CD_CALLER_ID);
                com.Parameters.AddWithValue("@Remarks", objDialer.CD_REMARKS);
                com.Parameters.AddWithValue("@UserKey", UserKey);
                SqlHelper.ExecuteNonQuery(ref com, "[USP_CIP_ADD_LEAD_MASTER_API]");
                i = 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return i;
        }

        public List<Application> GetApplications(int CallerKey, string SearchText)
        {
            List<Application> lstApplication = new List<Application>();
            try
            {
                DataSet dsClientData = new DataSet();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;

                //if (Convert.IsDBNull(SearchText)) {
                //    SearchText = "";
                //}
                if (SearchText == "")
                {
                    _cmd.Parameters.Add(new SqlParameter("@SearchText", DBNull.Value));
                }
                else
                {
                    _cmd.Parameters.Add(new SqlParameter("@SearchText", SearchText));
                }

                _cmd.Parameters.Add(new SqlParameter("@Caller", CallerKey));

                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_SEARCH_APPLICATION_API");

                if (dsClientData.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsClientData.Tables[0].Rows.Count; i++)
                    {
                        Application objapp = new Application();
                        objapp.Id = Convert.ToInt32(dsClientData.Tables[0].Rows[i]["Id"]);
                        objapp.Bank = Convert.ToString(dsClientData.Tables[0].Rows[i]["BANK_NAME"]);
                        objapp.AppNo = Convert.ToString(dsClientData.Tables[0].Rows[i]["APP_NO"]);
                        objapp.AppDate = Convert.ToString(dsClientData.Tables[0].Rows[i]["APP_DATE"]);
                        objapp.CPVStatus = Convert.ToString(dsClientData.Tables[0].Rows[i]["cpv_status"]);
                        objapp.Comments = Convert.ToString(dsClientData.Tables[0].Rows[i]["Remarks"]);
                        objapp.Status = Convert.ToString(dsClientData.Tables[0].Rows[i]["StatusDesc"]);
                        objapp.Caller = Convert.ToString(dsClientData.Tables[0].Rows[i]["Caller"]);
                        objapp.LoginAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["loan_amnt"]);
                        objapp.Product = Convert.ToString(dsClientData.Tables[0].Rows[i]["Product"]);
                        objapp.ClientName = Convert.ToString(dsClientData.Tables[0].Rows[i]["client_name"]);
                        objapp.UpdatedOn = Convert.ToString(dsClientData.Tables[0].Rows[i]["UpdatedOn"]);
                        objapp.DisbAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["DisbAmt"]);
                        objapp.DisbDate = Convert.ToString(dsClientData.Tables[0].Rows[i]["DisbDate"]);
                        objapp.LosNo = Convert.ToString(dsClientData.Tables[0].Rows[i]["LosNo"]);
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

        public List<DailyLogin> GetDailyLoginList(int CallerKey, string SearchText)
        {
            List<DailyLogin> lstApplication = new List<DailyLogin>();
            try
            {
                DataSet dsClientData = new DataSet();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;

                //if (Convert.IsDBNull(SearchText)) {
                //    SearchText = "";
                //}
                if (SearchText == "")
                {
                    _cmd.Parameters.Add(new SqlParameter("@SearchText", DBNull.Value));
                }
                else
                {
                    _cmd.Parameters.Add(new SqlParameter("@SearchText", SearchText));
                }

                _cmd.Parameters.Add(new SqlParameter("@Caller", CallerKey));

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

        public List<Application> GetDisbursedList(int CallerKey, string SearchText)
        {
            List<Application> lstApplication = new List<Application>();
            try
            {
                DataSet dsClientData = new DataSet();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;

                //if (Convert.IsDBNull(SearchText)) {
                //    SearchText = "";
                //}
                if (SearchText == "")
                {
                    _cmd.Parameters.Add(new SqlParameter("@SearchText", DBNull.Value));
                }
                else
                {
                    _cmd.Parameters.Add(new SqlParameter("@SearchText", SearchText));
                }

                _cmd.Parameters.Add(new SqlParameter("@Caller", CallerKey));

                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_SEARCH_DISBURSED_APPLICATION_API");

                if (dsClientData.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsClientData.Tables[0].Rows.Count; i++)
                    {
                        Application objapp = new Application();
                        objapp.Id = Convert.ToInt32(dsClientData.Tables[0].Rows[i]["Id"]);
                        objapp.Bank = Convert.ToString(dsClientData.Tables[0].Rows[i]["BANK_NAME"]);
                        objapp.AppNo = Convert.ToString(dsClientData.Tables[0].Rows[i]["APP_NO"]);
                        objapp.AppDate = Convert.ToString(dsClientData.Tables[0].Rows[i]["APP_DATE"]);
                        objapp.CPVStatus = Convert.ToString(dsClientData.Tables[0].Rows[i]["cpv_status"]);
                        objapp.Comments = Convert.ToString(dsClientData.Tables[0].Rows[i]["Remarks"]);
                        objapp.Status = Convert.ToString(dsClientData.Tables[0].Rows[i]["StatusDesc"]);
                        objapp.Caller = Convert.ToString(dsClientData.Tables[0].Rows[i]["Caller"]);
                        objapp.LoginAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["loan_amnt"]);
                        objapp.Product = Convert.ToString(dsClientData.Tables[0].Rows[i]["Product"]);
                        objapp.ClientName = Convert.ToString(dsClientData.Tables[0].Rows[i]["client_name"]);
                        objapp.UpdatedOn = Convert.ToString(dsClientData.Tables[0].Rows[i]["UpdatedOn"]);
                        objapp.DisbAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["DisbAmt"]);
                        objapp.DisbDate = Convert.ToString(dsClientData.Tables[0].Rows[i]["DisbDate"]);
                        objapp.LosNo = Convert.ToString(dsClientData.Tables[0].Rows[i]["LosNo"]);
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


        public List<Application> SearchApp(string CallerKey, string SearchText)
        {
            List<Application> lstApplication = new List<Application>();
            try
            {
                DataSet dsClientData = new DataSet();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;

                //if (Convert.IsDBNull(SearchText)) {
                //    SearchText = "";
                //}
                _cmd.Parameters.Add(new SqlParameter("@SearchText", SearchText));
                _cmd.Parameters.Add(new SqlParameter("@Caller", CallerKey));
                library.WriteErroLog("Before execute dataset");
                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_SEARCH_APPLICATION_API");
                library.WriteErroLog("After execute dataset");
                if (dsClientData.Tables[0].Rows.Count > 0)
                {
                    library.WriteErroLog(dsClientData.Tables[0].Rows.Count.ToString());
                    for (int i = 0; i < dsClientData.Tables[0].Rows.Count; i++)
                    {
                        Application objapp = new Application();
                        objapp.Id = Convert.ToInt32(dsClientData.Tables[0].Rows[i]["Id"]);
                        objapp.Bank = Convert.ToString(dsClientData.Tables[0].Rows[i]["BANK_NAME"]);
                        objapp.AppNo = Convert.ToString(dsClientData.Tables[0].Rows[i]["APP_NO"]);
                        objapp.AppDate = Convert.ToString(dsClientData.Tables[0].Rows[i]["APP_DATE"]);
                        objapp.CPVStatus = Convert.ToString(dsClientData.Tables[0].Rows[i]["cpv_status"]);
                        objapp.Comments = Convert.ToString(dsClientData.Tables[0].Rows[i]["Remarks"]);
                        objapp.Status = Convert.ToString(dsClientData.Tables[0].Rows[i]["StatusDesc"]);
                        //objapp.StatusDate = Convert.ToString(dsClientData.Tables[0].Rows[i]["UpdatedOn"]);
                        objapp.LoginAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["loan_amnt"]);
                        objapp.Product = Convert.ToString(dsClientData.Tables[0].Rows[i]["Product"]);
                        objapp.ClientName = Convert.ToString(dsClientData.Tables[0].Rows[i]["client_name"]);
                        objapp.UpdatedOn = Convert.ToString(dsClientData.Tables[0].Rows[i]["UpdatedOn"]);
                        objapp.DisbAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["DisbAmt"]);
                        objapp.DisbDate = Convert.ToString(dsClientData.Tables[0].Rows[i]["DisbDate"]);
                        lstApplication.Add(objapp);
                    }
                }
            }
            catch (Exception ex)
            {
                library.WriteErroLog(SearchText + ":SearchText, CallerKey" + CallerKey + "-----" + ex.StackTrace);
                throw ex;
            }
            return lstApplication;
        }

        public List<Incentive> GetIncentive(int UserKey)
        {
            List<Incentive> lstApplication = new List<Incentive>();
            try
            {
                DataSet dsClientData = new DataSet();
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;

                //if (Convert.IsDBNull(SearchText)) {
                //    SearchText = "";
                //}

                _cmd.Parameters.Add(new SqlParameter("@UserKey", UserKey));

                dsClientData = SqlHelper.ExecuteDS(ref _cmd, "USP_GET_INCENTIVE_API");
                if (dsClientData.Tables[0].Rows.Count > 0)
                {
                    // library.WriteErroLog(dsClientData.Tables[0].Rows.Count.ToString());
                    for (int i = 0; i < dsClientData.Tables[0].Rows.Count; i++)
                    {
                        Incentive objapp = new Incentive();
                        objapp.APLMP = Convert.ToString(dsClientData.Tables[0].Rows[i]["APLMP"]);
                        objapp.APLNMP = Convert.ToString(dsClientData.Tables[0].Rows[i]["APLNMP"]);
                        objapp.BL = Convert.ToString(dsClientData.Tables[0].Rows[i]["BL"]);
                        objapp.BLAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["BLAmt"]);
                        objapp.CC = Convert.ToString(dsClientData.Tables[0].Rows[i]["CC"]);
                        objapp.CCAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["CCAmt"]);
                        objapp.HL = Convert.ToString(dsClientData.Tables[0].Rows[i]["HL"]);
                        objapp.HLAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["HLAmt"]);
                        objapp.Incentives = Convert.ToString(dsClientData.Tables[0].Rows[i]["Incentives"]);
                        objapp.PLMP = Convert.ToString(dsClientData.Tables[0].Rows[i]["PLMP"]);
                        objapp.PLNMP = Convert.ToString(dsClientData.Tables[0].Rows[i]["PLNMP"]);
                        objapp.PointsPL = Convert.ToString(dsClientData.Tables[0].Rows[i]["Points"]);
                        objapp.ReferralAmt = Convert.ToString(dsClientData.Tables[0].Rows[i]["PL Referral Amt"]);
                        objapp.ReferralsPL = Convert.ToString(dsClientData.Tables[0].Rows[i]["PL Referrals"]);

                        lstApplication.Add(objapp);
                    }
                }
            }
            catch (Exception ex)
            {
                //library.WriteErroLog(SearchText + ":SearchText, CallerKey" + CallerKey + "-----" + ex.StackTrace);
                throw ex;
            }
            return lstApplication;
        }
        #endregion

        #region COMPANYCATEGORY
        public List<CompCat> CompanyCatList(int displayStart, int displayLength, int sortCol, string sortDir, string SearchText, int ProductKey, int BankKey)
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.CommandTimeout = 180;
                cmd.Parameters.AddWithValue("@DisplayLength", displayLength);
                cmd.Parameters.AddWithValue("@DisplayStart", displayStart);
                cmd.Parameters.AddWithValue("@SortCol", sortCol);
                cmd.Parameters.AddWithValue("@SortDir", sortDir);
                cmd.Parameters.AddWithValue("@SearchText", SearchText);
                cmd.Parameters.AddWithValue("@ProductKey", ProductKey);
                cmd.Parameters.AddWithValue("@BankKey", BankKey);

                var ds = SqlHelper.ExecuteDS(ref cmd, "[USP_GET_COMPANY_CATEGORY]");
                return ds.Tables[0].AsEnumerable()
                    .Select(row => new CompCat
                    {
                        NAME = row["NAME"].ToString(),
                        CATEGORY = row["CATEGORY"].ToString(),
                        BANK = row["BANK"].ToString(),
                        PRODUCT = row["PRODUCT"].ToString(),
                        TOTALCOUNT = Convert.ToInt32(row["TotalCount"])
                    }).ToList();
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
        }

        //    public async Task<List<CompCat>> CompanyCatListAsync(
        //int displayStart, int displayLength, int sortCol, string sortDir,
        //string SearchText, int ProductKey, int BankKey)
        //    {
        //        using (var connection = new SqlConnection(SqlHelper.connectionString))
        //        using (var cmd = new SqlCommand("[USP_GET_COMPANY_CATEGORY]", connection))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.CommandTimeout = 180;
        //            cmd.Parameters.AddWithValue("@DisplayLength", displayLength);
        //            cmd.Parameters.AddWithValue("@DisplayStart", displayStart);
        //            cmd.Parameters.AddWithValue("@SortCol", sortCol);
        //            cmd.Parameters.AddWithValue("@SortDir", sortDir);
        //            cmd.Parameters.AddWithValue("@SearchText", SearchText ?? string.Empty);
        //            cmd.Parameters.AddWithValue("@ProductKey", ProductKey);
        //            cmd.Parameters.AddWithValue("@BankKey", BankKey);

        //            var ds = new DataSet();
        //            using (var adapter = new SqlDataAdapter(cmd))
        //            {
        //                await connection.OpenAsync();
        //                await Task.Run(() => adapter.Fill(ds));
        //            }

        //            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //            {
        //                // Return an empty list if nothing is returned
        //                return new List<CompCat>();
        //            }

        //            return ds.Tables[0].AsEnumerable()
        //                .Select(row => new CompCat
        //                {
        //                    NAME = row.Field<string>("NAME"),
        //                    CATEGORY = row.Field<string>("CATEGORY"),
        //                    BANK = row.Field<string>("BANK"),
        //                    PRODUCT = row.Field<string>("PRODUCT"),
        //                    TOTALCOUNT = row.Field<int>("TotalCount")
        //                }).ToList();
        //        }
        //    }


        public async Task<List<CompCat>> CompanyCatListAsync(
    int displayStart,
    int displayLength,
    int sortCol,
    string sortDir,
    string searchText,
    int productKey,
    int bankKey)
        {
            var result = new List<CompCat>();

            using (var connection = new SqlConnection(SqlHelper.connectionString))
            using (var cmd = new SqlCommand("[USP_GET_COMPANY_CATEGORY]", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 180;

                // Use strongly typed parameters instead of AddWithValue
                cmd.Parameters.Add("@DisplayLength", SqlDbType.Int).Value = displayLength;
                cmd.Parameters.Add("@DisplayStart", SqlDbType.Int).Value = displayStart;
                cmd.Parameters.Add("@SortCol", SqlDbType.Int).Value = sortCol;
                cmd.Parameters.Add("@SortDir", SqlDbType.VarChar, 10).Value = sortDir;
                cmd.Parameters.Add("@SearchText", SqlDbType.NVarChar, 200).Value = searchText ?? string.Empty;
                cmd.Parameters.Add("@ProductKey", SqlDbType.Int).Value = productKey;
                cmd.Parameters.Add("@BankKey", SqlDbType.Int).Value = bankKey;

                try
                {
                    await connection.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var compCat = new CompCat
                            {
                                NAME = reader["NAME"] != DBNull.Value ? reader["NAME"].ToString() : string.Empty,
                                CATEGORY = reader["CATEGORY"] != DBNull.Value ? reader["CATEGORY"].ToString() : string.Empty,
                                BANK = reader["BANK"] != DBNull.Value ? reader["BANK"].ToString() : string.Empty,
                                PRODUCT = reader["PRODUCT"] != DBNull.Value ? reader["PRODUCT"].ToString() : string.Empty,
                                TOTALCOUNT = reader["TotalCount"] != DBNull.Value ? Convert.ToInt32(reader["TotalCount"]) : 0
                            };
                            result.Add(compCat);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Optional: log exception details here
                    // Logger.LogError(ex, "Error fetching company category list");
                    throw;
                }
            }

            return result;
        }

        #endregion

        public async Task<List<BankSearch>> BankListAsync(
            int displayStart,
            int displayLength,
            int sortCol,
            string sortDir,
            string searchText
    )
        {
            var result = new List<BankSearch>();

            using (var connection = new SqlConnection(SqlHelper.connectionString))
            using (var cmd = new SqlCommand("USP_GET_BANK_BY_PINCODE", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 180;

                var normalizedSortDir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
                var safeDisplayLength = displayLength <= 0 ? 10 : Math.Min(displayLength, 100);
                var safeDisplayStart = displayStart < 0 ? 0 : displayStart;
                var safeSortCol = sortCol < 0 ? 0 : sortCol;
                var safeSearchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim();

                cmd.Parameters.Add("@DisplayLength", SqlDbType.Int).Value = safeDisplayLength;
                cmd.Parameters.Add("@DisplayStart", SqlDbType.Int).Value = safeDisplayStart;
                cmd.Parameters.Add("@SortCol", SqlDbType.Int).Value = safeSortCol;
                cmd.Parameters.Add("@SortDir", SqlDbType.VarChar, 10).Value = normalizedSortDir;
                cmd.Parameters.Add("@SearchText", SqlDbType.VarChar, 200).Value = safeSearchText ?? string.Empty;                

                try
                {
                    await connection.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var banksearch = new BankSearch
                            {
                                BankName = reader["BankName"] != DBNull.Value ? reader["BankName"].ToString() : string.Empty,
                                PinCode = reader["PinCode"] != DBNull.Value ? reader["PinCode"].ToString() : string.Empty,
                                Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : string.Empty,
                                Serviceable = reader["Serviceable"] != DBNull.Value ? reader["Serviceable"].ToString() : string.Empty,
                                RMName = reader["RMName"] != DBNull.Value ? reader["RMName"].ToString() : string.Empty,
                                RMEmail = reader["RMEmail"] != DBNull.Value ? reader["RMEmail"].ToString() : string.Empty,
                                RMContact = reader["RMContact"] != DBNull.Value ? Convert.ToInt32(reader["RMContact"]) : 0,
                                TotalCount = reader["TotalCount"] != DBNull.Value ? Convert.ToInt32(reader["TotalCount"]) : 0
                            };
                            result.Add(banksearch);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Optional: log exception details here
                    // Logger.LogError(ex, "Error fetching company category list");
                    throw;
                }
            }

            return result;
        }

        public string checkDND(string MobileNo)
        {
            DataSet ds = null;
            string strMsg = "";
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@MOBILE_NO", MobileNo);
                ds = SqlHelper.ExecuteDS(ref com, "USP_VALIDATE_DND_MOBILENO");
                DataTable dt = ds.Tables[0];

                if (dt != null && dt.Rows.Count > 0)
                {
                    strMsg = Convert.ToString(dt.Rows[0][0]);
                }
            }
            catch (Exception)
            {
                strMsg = "";
            }
            return strMsg;
        }

        public int InsertDailyLogins(int Product, string Banks, string LOSNo, string AppNo, int Region, string ClientName, string Contact, decimal? LoginAmt, string Remarks, int UserKey)
        {
            int i = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@PRODUCT_KEY", Product);
                com.Parameters.AddWithValue("@BANKS", Banks);
                com.Parameters.AddWithValue("@LOSNO", LOSNo);
                //com.Parameters.AddWithValue("@APPNO", AppNo); 
                com.Parameters.AddWithValue("@REGION", Region);
                com.Parameters.AddWithValue("@CLIENTNAME", ClientName);
                com.Parameters.AddWithValue("@CONTACT", Contact);
                com.Parameters.AddWithValue("@LOGINAMT", LoginAmt);
                com.Parameters.AddWithValue("@REMARKS", Remarks);
                com.Parameters.AddWithValue("@CREATED_BY", UserKey);
                SqlHelper.ExecuteNonQuery(ref com, "USP_INSERT_DAILY_LOGINS_API");
                i = 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return i;
        }

        public int fnSaveRecordingDetail(int Key, string Caller, string FileName, string Date, string Contact
            )
        {
            int i = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@Key", Key);
                com.Parameters.AddWithValue("@Caller", Caller);
                com.Parameters.AddWithValue("@FileName", FileName);
                com.Parameters.AddWithValue("@Date", Date);
                com.Parameters.AddWithValue("@Contact", Contact);

                SqlHelper.ExecuteNonQuery(ref com, "[USP_CIP_INSERT_RECORDING_API]");
                i = 1;
            }
            catch (Exception)
            {
                i = 0;
            }
            return i;
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

        public int InsertSpinWheelHistory(int userKey, int prizeId)
        {

            int i = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.Add(new SqlParameter("@UserKey", SqlDbType.Int) { Value = userKey });
                com.Parameters.Add(new SqlParameter("@PrizeId", SqlDbType.Int) { Value = prizeId });
                //com.Parameters.Add(new SqlParameter("@CustMobileNo", SqlDbType.Int) { Value = custMobileNo });

                SqlHelper.ExecuteNonQuery(ref com, "[USP_INSERT_INTO_SPIN_WHEEL_HISTORY]");
                i = 1;
            }
            catch (Exception)
            {
                i = 0;
            }
            return i;
        }

        public async Task<int> fnSaveCallLogsUtilities(List<CallLogUtilitiesList> CallLog)
        {
            int intRetVal = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["DBCS"].ToString()))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {
                        await con.OpenAsync(); // Use async open
                        sqlBulkCopy.DestinationTableName = "TBL_CALL_LOGS_UTILITIES";
                        sqlBulkCopy.ColumnMappings.Add("name", "name");
                        sqlBulkCopy.ColumnMappings.Add("dateTime", "date");
                        sqlBulkCopy.ColumnMappings.Add("duration", "duration");
                        sqlBulkCopy.ColumnMappings.Add("type", "type");
                        sqlBulkCopy.ColumnMappings.Add("userkey", "userkey");
                        sqlBulkCopy.ColumnMappings.Add("phoneNumber", "number");

                        DataTable dt = ToDataTable(CallLog);
                        await sqlBulkCopy.WriteToServerAsync(dt); // Use async write

                    }

                    intRetVal = 1;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                throw; // Consider logging the exception instead of rethrowing
            }
            return intRetVal;
        }
        //public int fnSaveCallLogsUtilities(List<CallLogUtilitiesList> CallLog)
        //{
        //    //List<CallLogList> lstCallLog = objCallLogInput.RequestData.lstCallLog;            
        //    int intRetVal = 0;
        //    //    try
        //    //    {
        //    //        //if (lstCallLog[0].userkey == 24510 || lstCallLog[0].userkey == 1447)
        //    //        //{
        //    //        using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["DBCS"].ToString()))
        //    //        //{
        //    //        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
        //    //        //{
        //    //        //    con.Open();
        //    //        //    sqlBulkCopy.DestinationTableName = "TBL_CALL_LOGS_UTILITIES";
        //    //        //    sqlBulkCopy.ColumnMappings.Add("name", "name");
        //    //        //    sqlBulkCopy.ColumnMappings.Add("dateTime", "date");
        //    //        //    sqlBulkCopy.ColumnMappings.Add("duration", "duration");
        //    //        //    sqlBulkCopy.ColumnMappings.Add("type", "type");
        //    //        //    sqlBulkCopy.ColumnMappings.Add("userkey", "userkey");
        //    //        //    sqlBulkCopy.ColumnMappings.Add("phoneNumber", "number");
        //    //        //    DataTable dt = ToDataTable(CallLog);
        //    //        //    sqlBulkCopy.WriteToServer(dt);

        //    //        //    con.Close();
        //    //        //}

        //    //        intRetVal = 1;
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        throw ex;
        //    //    }
        //    //    return intRetVal;
        //    //}

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
    }



    public static class library
    {
        public static void WriteErroLog(string message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
                sw.WriteLine(System.DateTime.Now + ":" + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

public class LeadMaster
{
    public int Key { get; set; }
    public int CreatedBy { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string MobileNo { get; set; }
    public string EmailID { get; set; }
    public string CreatedDate { get; set; }
    public int AssignedTo { get; set; }
    public int CallerUserID { get; set; }
    public string CallerUserName { get; set; }
    public string AssignedToName { get; set; }
    public int AssignedBy { get; set; }
    public string AssignedByName { get; set; }
    public int Product { get; set; }
    public string ProductDesc { get; set; }
    public string Status { get; set; }
    public string CallerRemarks { get; set; }
    public string Remarks { get; set; }
    public string AssignedOn { get; set; }
    public int IsEditable { get; set; }
    public int IsAssignable { get; set; }
    public string LkpDesc { get; set; }
    public string CallBackDate { get; set; }
    public string CallBackTime { get; set; }
    public string LastUpdatedOn { get; set; }
    public string CreatedByName { get; set; }
    public string Keys { get; set; }
    public int TotalCount { get; set; }

}

public class CompCat
{
    public string NAME { get; set; }
    public string CATEGORY { get; set; }
    public string BANK { get; set; }
    public string PRODUCT { get; set; }
    public int TOTALCOUNT { get; set; }
}

public class BankSearch
{
    public string BankName { get; set; }
    public string PinCode { get; set; }
    public string Address { get; set; }
    public string RMName { get; set; }
    public string RMEmail { get; set; }
    public int RMContact { get; set; }
    public int TotalCount { get;  set; }
    public string Serviceable { get; set; }
}
