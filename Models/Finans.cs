using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using DialerAPi.Models;
using System.Configuration;
using PickupAPi.Models;

namespace PickupAPi.Models
{

    public class UserMaster
    {
        public int ID { get; set; }
        public string NAME { get; set; }
        public string MOBILE_NO { get; set; }
        public string PASSWORD { get; set; }
        public string ADDRESS { get; set; }
        public string AADHAR_NO { get; set; }
        public string PAN_NO { get; set; }
        public string BANK_ACC_NO { get; set; }
        public string BANK_IFSC_CODE { get; set; }
        public string BANK_ACC_NAME { get; set; }
        public string BANK_NAME { get; set; }
        public string DEVICE_ID { get; set; }
        public string IS_ACTIVE { get; set; }
        public string KYC_VERIFIED { get; set; }

    }


    public class Finans
    {
        public UserMaster ValidateLoginFinans(string UserName, string Password)
        {

            UserMaster obj = new UserMaster();

            DataSet ds = null;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@UserName", UserName);
                com.Parameters.AddWithValue("@Password", Password);

                ds = SqlHelper.ExecuteDS(ref com, "USP_FINANS_VALIDATE_LOGIN");

                DataTable dt = ds.Tables[0];

                if (dt != null && dt.Rows.Count > 0)
                {

                    obj.ID = Convert.ToInt32(dt.Rows[0]["ID"]);
                    obj.NAME = Convert.ToString(dt.Rows[0]["NAME"]);
                    obj.MOBILE_NO = Convert.ToString(dt.Rows[0]["MOBILE_NO"]);
                    obj.AADHAR_NO = Convert.ToString(dt.Rows[0]["AADHAR_NO"]);

                    obj.PAN_NO = Convert.ToString(dt.Rows[0]["PAN_NO"]);
                    obj.ADDRESS = Convert.ToString(dt.Rows[0]["ADDRESS"]);
                    obj.BANK_ACC_NO = Convert.ToString(dt.Rows[0]["BANK_ACC_NO"]);

                    obj.BANK_NAME = Convert.ToString(dt.Rows[0]["BANK_NAME"]);
                    obj.BANK_IFSC_CODE = Convert.ToString(dt.Rows[0]["BANK_IFSC_CODE"]);
                    obj.BANK_ACC_NO = Convert.ToString(dt.Rows[0]["BANK_ACC_NO"]);

                    obj.BANK_ACC_NO = Convert.ToString(dt.Rows[0]["BANK_ACC_NO"]);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return obj;
        }
        public int ManageUserMaster(UserMaster objUserMaster)
        {
            int i = 0;
            try
            {
                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;

                com.Parameters.AddWithValue("@ID",              objUserMaster.ID);
                com.Parameters.AddWithValue("@MOBILE_NO",       objUserMaster.MOBILE_NO);
                com.Parameters.AddWithValue("@NAME",            objUserMaster.NAME);
                com.Parameters.AddWithValue("@PASSWORD",        objUserMaster.PASSWORD);
                com.Parameters.AddWithValue("@ADDRESS",         objUserMaster.ADDRESS);
                com.Parameters.AddWithValue("@AADHAR_NO",       objUserMaster.AADHAR_NO);
                com.Parameters.AddWithValue("@PAN_NO",          objUserMaster.PAN_NO);
                com.Parameters.AddWithValue("@BANK_ACC_NO",     objUserMaster.BANK_ACC_NO);
                com.Parameters.AddWithValue("@BANK_IFSC_CODE",  objUserMaster.BANK_IFSC_CODE);
                com.Parameters.AddWithValue("@BANK_ACC_NAME",   objUserMaster.BANK_ACC_NAME);
                com.Parameters.AddWithValue("@BANK_NAME",       objUserMaster.BANK_NAME);
                com.Parameters.AddWithValue("@DEVICE_ID",       objUserMaster.DEVICE_ID);

                SqlHelper.ExecuteNonQuery(ref com, "USP_FINANS_MANAGE_USER_MASTER");
                i = 1;
            }
            catch (Exception)
            {
                i = 0;
            }
            return i;
        }

        
        

    }
}