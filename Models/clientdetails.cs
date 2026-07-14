using PickupAPi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DialerAPi.Models
{
    public class clientdetails
    {
        public string customdate { get; set; }
        private string strStatusDesc;
        public string StatusDesc
        {
            get
            {
                return strStatusDesc;
            }
            set
            {
                strStatusDesc = value;
            }
        }

        private string strUpdatedOn;
        public string UpdatedOn
        {
            get
            {
                return strUpdatedOn;
            }
            set
            {
                strUpdatedOn = value;
            }
        }

        private string strremarks;
        public string remarks
        {
            get
            {
                return strremarks;
            }
            set
            {
                strremarks = value;
            }
        }

        private decimal NET_SALARY;

        public decimal net_salary
        {
            get
            {
                return NET_SALARY;
            }
            set
            {
                NET_SALARY = value;
            }
        }
        

        private string cpvstatus;

        public string cpv_status
        {
            get
            {
                return cpvstatus;
            }
            set
            {
                cpvstatus = value;
            }
        }

        private string DISBURSAL_DATE;

        public string disbursal_date
        {
            get
            {
                return DISBURSAL_DATE;
            }
            set
            {
                DISBURSAL_DATE = value;
            }
        }

        private string SENT_TO_BANK_DATE;

        public string sent_to_bank_date
        {
            get
            {
                return SENT_TO_BANK_DATE;
            }
            set
            {
                SENT_TO_BANK_DATE = value;
            }
        }

        private string APP_DATE;

        public string app_date
        {
            get
            {
                return APP_DATE;
            }
            set
            {
                APP_DATE = value;
            }
        }

        private string SESS_EMP_ID;
        public string sess_emp_id
        {
            get
            {
                return SESS_EMP_ID;
            }
            set
            {
                SESS_EMP_ID = value;
            }
        }

        private string credit_month;

        public string creditmonth
        {
            get
            {
                return credit_month;
            }
            set
            {
                credit_month = value;
            }
        }

        private string SESS_EMP_NAME;
        public string sess_emp_name
        {
            get
            {
                return SESS_EMP_NAME;
            }
            set
            {
                SESS_EMP_NAME = value;
            }
        }

        private string SESS_ROLE_ID;
        public string sess_role_id
        {
            get
            {
                return SESS_ROLE_ID;
            }
            set
            {
                SESS_ROLE_ID = value;
            }
        }

        private string LOS_NO;

        public string los_no
        {
            get
            {
                return LOS_NO;
            }
            set
            {
                LOS_NO = value;
            }
        }

        private string SESS_LOC_ID;
        public string sess_loc_id
        {
            get
            {
                return SESS_LOC_ID;
            }
            set
            {
                SESS_LOC_ID = value;
            }
        }


        private string SESS_DESIG;
        public string sess_desig
        {
            get
            {
                return SESS_DESIG;
            }
            set
            {
                SESS_DESIG = value;
            }
        }

        private string SESS_DOJ;
        public string sess_doj
        {
            get
            {
                return SESS_DOJ;
            }
            set
            {
                SESS_DOJ = value;
            }
        }



        private string UID;
        public string uid
        {
            get
            {
                return UID;
            }
            set
            {
                UID = value;
            }
        }


        private string APP_NO;
        public string app_no
        {
            get
            {
                return APP_NO;
            }
            set
            {
                APP_NO = value;
            }
        }


        private string BANK_NAME;
        public string bank_name
        {
            get
            {
                return BANK_NAME;
            }
            set
            {
                BANK_NAME = value;
            }
        }


        private string LOAN_NAME;
        public string loan_name
        {
            get
            {
                return LOAN_NAME;
            }
            set
            {
                LOAN_NAME = value;
            }
        }



        private string LOAN_SCHEME_NAME;
        public string loan_scheme_name
        {
            get
            {
                return LOAN_SCHEME_NAME;
            }
            set
            {
                LOAN_SCHEME_NAME = value;
            }
        }


        private string LOAN_AMNT;
        public string loan_amnt
        {
            get
            {
                return LOAN_AMNT;
            }
            set
            {
                LOAN_AMNT = value;
            }
        }

        private string strdisbursed_amnt;
        public string disbursed_amnt
        {
            get
            {
                return strdisbursed_amnt;
            }
            set
            {
                strdisbursed_amnt = value;
            }
        }


        private string PURPOSE_OF_LOAN;
        public string purpose_of_loan
        {
            get
            {
                return PURPOSE_OF_LOAN;
            }
            set
            {
                PURPOSE_OF_LOAN = value;
            }
        }


        private string LOGIN_CHANNEL_NAME;
        public string login_channel_name
        {
            get
            {
                return LOGIN_CHANNEL_NAME;
            }
            set
            {
                LOGIN_CHANNEL_NAME = value;
            }
        }


        private string LOGIN_REGION_NAME;
        public string login_region_name
        {
            get
            {
                return LOGIN_REGION_NAME;
            }
            set
            {
                LOGIN_REGION_NAME = value;
            }
        }

        private string LOC_NAME;
         public string loc_name
        {
            get
            {
                return LOC_NAME;
            }
            set
            {
                LOC_NAME = value;
            }
        }


        private string CLIENT_NAME;
        public string client_name
        {
            get
            {
                return CLIENT_NAME;
            }
            set
            {
                CLIENT_NAME = value;
            }
        }


        private Int64? CLIENT_MBL_NO;
        public Int64? client_mbl_no
        {
            get
            {
                return CLIENT_MBL_NO;
            }
            set
            {
                CLIENT_MBL_NO = value;
            }
        }


        private string CLIENT_DOB;
        public string client_DOB
        {
            get
            {
                return CLIENT_DOB;
            }
            set
            {
                CLIENT_DOB = value;
            }
        }


        private string CLIENT_PAN;
        public string client_pan
        {
            get
            {
                return CLIENT_PAN;
            }
            set
            {
                CLIENT_PAN = value;
            }
        }


        private string CLIENT_AADHAR_NO;
        public string client_aadhar_no
        {
            get
            {
                return CLIENT_AADHAR_NO;
            }
            set
            {
                CLIENT_AADHAR_NO = value;
            }
        }


        private string GENDER;
        public string gender
        {
            get
            {
                return GENDER;
            }
            set
            {
                GENDER = value;
            }
        }


        private string MARITATAL_STATUS;
        public string marital_status
        {
            get
            {
                return MARITATAL_STATUS;
            }
            set
            {
                MARITATAL_STATUS = value;
            }
        }


        private string DESIG_NAME;
        public string desig_name
        {
            get
            {
                return DESIG_NAME;
            }
            set
            {
                DESIG_NAME = value;
            }
        }

        private string TEL_CALLER_NAME;
        public string tel_caller_name
        {
            get
            {
                return TEL_CALLER_NAME;
            }
            set
            {
                TEL_CALLER_NAME = value;
            }
        }

        private string SUPERVISOR_NAME;
        public string supervisor_name
        {
            get
            {
                return SUPERVISOR_NAME;
            }
            set
            {
                SUPERVISOR_NAME = value;
            }
        }

        private string UNIQID;

        public string UniqId
        {
            get
            {
                return UNIQID;
            }
            set
            {
                UNIQID = value;
            }
        }

        private string COMMENTS;
        public string comments
        {
            get
            {
                return COMMENTS;
            }
            set
            {
                COMMENTS = value;
            }
        }

        private string TENURE;
        public string tenure
        {
            get
            {
                return TENURE;
            }
            set
            {
                TENURE = value;
            }
        }

        public string hold_status { get; set; }
        public string add_doc_req { get; set; }
        public string img_rel_remarks { get; set; }

        public int TotalCount { get; set; }

        public DataSet get_StatusWiseClientDetails(int intStatus, string strUSER_ID, int intRoleId)
        {
            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;

                _cmd.Parameters.Add(new SqlParameter("@STATUS", intStatus));
                _cmd.Parameters.Add(new SqlParameter("@ROLEID", intRoleId));
                _cmd.Parameters.Add(new SqlParameter("@USERID", strUSER_ID));

                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_CIP_GET_STATUSWISE_DETAILS]");

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public DataSet get_ClientDetails(string Action, string SearchText, int intUserKey)
        {
            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.Add(new SqlParameter("@Action", Action));
                _cmd.Parameters.Add(new SqlParameter("@SearchText", SearchText));
                _cmd.Parameters.Add(new SqlParameter("@UserKey", intUserKey));
                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_CIP_GET_CLIENT_DETAILS]");

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public DataSet get_ClientDetailsReport(string Action, string SearchText, int intUserKey)
        {
            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.Add(new SqlParameter("@SearchText", SearchText));
                _cmd.Parameters.Add(new SqlParameter("@UserKey", intUserKey));
                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_CIP_GET_CLIENT_DETAILS_REPORT]");

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public DataSet get_ClientDetailsLead(int intKey, int intUserKey)
        {
            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;                
                _cmd.Parameters.Add(new SqlParameter("@Key", intKey));
                _cmd.Parameters.Add(new SqlParameter("@UserKey", intUserKey)); 

                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_CIP_GET_CLIENT_DETAILS_LEAD]");

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public DataSet get_ClientDetailsCustomPaging(int displayStart, int displayLength, int sortCol, string sortDir, string SearchText, 
            int intUserKey,string strLoginRegion,string strBank,string strSupervisor,string strCaller,string strStatus, string strAppDate)
        {
            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                _cmd.Parameters.Add(new SqlParameter("@DisplayLength", displayLength));
                _cmd.Parameters.Add(new SqlParameter("@DisplayStart", displayStart));
                _cmd.Parameters.Add(new SqlParameter("@SortCol", sortCol));
                _cmd.Parameters.Add(new SqlParameter("@SortDir", sortDir));
                _cmd.Parameters.Add(new SqlParameter("@SearchText", SearchText));
                _cmd.Parameters.Add(new SqlParameter("@UserKey", intUserKey));


                // PARAMETER SAFETY FIX: Pass DBNull.Value if the string filter is null or empty.
                _cmd.Parameters.Add(new SqlParameter("@FLoginRegion", string.IsNullOrEmpty(strLoginRegion) ? (object)DBNull.Value : strLoginRegion));
                _cmd.Parameters.Add(new SqlParameter("@FBank", string.IsNullOrEmpty(strBank) ? (object)DBNull.Value : strBank));
                _cmd.Parameters.Add(new SqlParameter("@FSupervisor", string.IsNullOrEmpty(strSupervisor) ? (object)DBNull.Value : strSupervisor));
                _cmd.Parameters.Add(new SqlParameter("@FCaller", string.IsNullOrEmpty(strCaller) ? (object)DBNull.Value : strCaller));
                _cmd.Parameters.Add(new SqlParameter("@FStatus", string.IsNullOrEmpty(strStatus) ? (object)DBNull.Value : strStatus));
                _cmd.Parameters.Add(new SqlParameter("@FAppDate", string.IsNullOrEmpty(strAppDate) ? (object)DBNull.Value : strAppDate));


                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_CIP_GET_CLIENT_DETAILS_CUSTOM_PAGING]");

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public DataSet get_ClientDetailsExport(int intUserKey)
        {
            DataSet ds = null;
            try
            {
                SqlCommand _cmd = new SqlCommand();
                _cmd.CommandTimeout = 180;
                
                _cmd.Parameters.Add(new SqlParameter("@UserKey", intUserKey));
                ds = SqlHelper.ExecuteDS(ref _cmd, "[USP_CIP_GET_CLIENT_DETAILS_EXPORT]");

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}