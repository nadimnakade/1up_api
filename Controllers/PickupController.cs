using DialerAPi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PickupAPi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Data.SqlClient;


namespace PickupAPi.Controllers
{

    public class Parameter
    {
        public string MobileNo { get; set; }
        public string UDID { get; set; }
        public string Type { get; set; }
    }




    public class PickupController : ApiController
    {



        [HttpPost]
        [ActionName("UpdateStatus")]
        public HttpResponseMessage UpdateStatus(string Key, string CallerRemarks, string Status, string CallBackDate, string CallBackTime, string Lat, string Long, string base64)
        {


            string strMsg = "";
            Models.Pickup objBO = new Models.Pickup();
            try
            {
                Models.UpdateStatus obj = new Models.UpdateStatus();

                obj.CallerRemarks = CallerRemarks;
                obj.Status = Status;
                obj.Key = Convert.ToInt32(Key);
                obj.CallBackDate = CallBackDate;
                obj.CallBackTime = CallBackTime;
                obj.Latitude = Lat;
                obj.Longitude = Long;
                obj.Image = base64;

                int intRetVal = objBO.UpdateStatus(obj);
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                if (intRetVal > 0)
                {
                    response.Content = new StringContent("SuccessFul", Encoding.UTF8, "application/json");

                }
                else
                {
                    response.Content = new StringContent("Error", Encoding.UTF8, "application/json");
                }
                return response;
            }
            catch (Exception)
            {
                strMsg = "Error";
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [HttpPost]
        [ActionName("TestMethod")]
        public string TestMethod([FromBody] UpdateStatus UpdateStatus)
        {
            return "Hello from http post web api controller: " + UpdateStatus.Key;
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetPickupCompleted()
        {
            Models.Pickup obj = new Models.Pickup();
            List<Pickup> lstPickup = obj.FetchPickupCompleted();

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [HttpGet]
        public HttpResponseMessage GetPickupData(string MobileNo)
        {
            Models.Pickup obj = new Models.Pickup();
            List<Pickup> lstPickup = obj.FetchPendingPickup(Convert.ToString(MobileNo));

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetExecutive(int UserKey)
        {
            Models.Pickup obj = new Models.Pickup();
            List<ListItem> lstPickup = obj.FetchExecutives(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetOffer()
        {
            Models.Pickup obj = new Models.Pickup();
            List<ListItem> lstPickup = obj.FetchOffer();

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetPickupEntryData(int UserKey)
        {
            Models.Pickup obj = new Models.Pickup();
            List<Pickup> lstPickup = obj.FetchPickupData(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetPickupEntryDataNew(int UserKey, int Type)
        {
            Models.Pickup obj = new Models.Pickup();
            List<Pickup> lstPickup = obj.FetchPickupDataNew(UserKey, Type);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public string GetPushID(int UserKey)
        {
            string result = string.Empty;
            try
            {
                Pickup objPickup = new Pickup();
                result = objPickup.GetPushID(UserKey);
            }
            catch (Exception exe)
            {
                result = exe.Message.ToString();
            }
            return result;
        }

        [AcceptVerbs("GET", "POST")]
        public string UpdateImageVideo()
        {

            string result = string.Empty;
            List<PickupAPi.Models.Attachment> lstAttachment = new List<PickupAPi.Models.Attachment>();
            PickupAPi.Models.Attachment objAttachment = new PickupAPi.Models.Attachment();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var PickupKey = HttpContext.Current.Request.Params.Get("PickupKey");



                for (int j = 0; j < httpRequest.Form.AllKeys.Length; j++)
                {
                    if (j > 0)
                    {
                        objAttachment = new PickupAPi.Models.Attachment();
                        objAttachment.PickupKey = Convert.ToInt32(PickupKey);
                        objAttachment.Image = httpRequest.Params.Get("files_" + (j - 1));
                        lstAttachment.Add(objAttachment);
                    }
                }
                Models.Pickup obj = new Models.Pickup();
                string strMsg = obj.InsertAttachment(lstAttachment);
                result = strMsg;
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("AuthenticateEmployee")]
        public HttpResponseMessage AuthenticateEmployee(string UserName, string Password, string UUID, string Type)
        {
            //string strTest = "Testing";

            //var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(strTest, Encoding.UTF8, "application/json");
            //return response;

            List<Employee> lstEmployee = new List<Employee>();
            Models.Pickup obj = new Models.Pickup();
            lstEmployee = obj.LoginCheck(UserName, Password, UUID, Type);

            var jsonSerializer = JsonConvert.SerializeObject(lstEmployee);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        [HttpPost]
        [ActionName("ValidateLogin")]
        public string ValidateLogin(Parameter parameter)
        {
            Models.Pickup obj = new Models.Pickup();
            string MobileNo = "", UDID = "", Type = "";
            MobileNo = parameter.MobileNo;
            UDID = parameter.UDID;
            Type = parameter.Type;
            string strReturn = "";
            try
            {
                Message objMsg = new Message();
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                strReturn = obj.ValidateLogin(MobileNo, UDID, Type);
                //objMsg.Description = strReturn;
                //var jsonSerializer = JsonConvert.SerializeObject(objMsg);
                //response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");                
            }
            catch (Exception)
            {
                //throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            //throw new HttpResponseException(HttpStatusCode.NotFound);
            return strReturn;
        }

        #region Dialer
        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetDialerData(int UserKey)
        {
            Dialer obj = new Dialer();
            List<Calling> lstPickup = obj.FetchDialerData(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetCallerDataIndividual(int UserKey, int Key)
        {
            Dialer obj = new Dialer();
            List<Dialer> lstPickup = obj.FetchCallingDataIndividual(UserKey, Key);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetDialerNotUpdatedData(int UserKey)
        {
            Dialer obj = new Dialer();
            List<Dialer> lstPickup = obj.FetchDialerNotUpdateData(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        [AcceptVerbs("GET", "POST")]
        public async Task<string> UpdateDialingStatus(int Key)
        {
            try
            {

                Models.Pickup objBO = new Models.Pickup();
                string strMsg = "";
                strMsg = await objBO.UpdateDialingStatusAsync(Key);
                if (strMsg == "Success")
                {
                    //response.Content = new StringContent("SuccessFul", Encoding.UTF8, "application/json");
                    strMsg = "SuccessFul";
                }
                return strMsg;
                //using (var cmd = new SqlCommand())
                //{
                //    cmd.Parameters.AddWithValue("@Key", Key);
                //    var result = await SqlHelper.ExecuteDSAsync(cmd, "USP_UPDATE_DIALING_STATUS");
                //    return result.Tables[0].Rows.Count > 0 ? "Success" : "Error";
                //}
            }
            catch (Exception ex)
            {
                // Log the error
                await LogErrorAsync(ex);
                return "Error";
            }
        }


        //[AcceptVerbs("GET", "POST")]
        //public string UpdateDialingStatus(int Key)
        //{
        //    Models.Pickup objBO = new Models.Pickup();
        //    string strMsg = "";
        //    strMsg = objBO.UpdateDialingStatusAsync(Key);

        //    if (strMsg == "Success")
        //    {
        //        //response.Content = new StringContent("SuccessFul", Encoding.UTF8, "application/json");
        //        strMsg = "SuccessFul";
        //    }
        //    //else
        //    //{
        //    //    //response.Content = new StringContent("Error", Encoding.UTF8, "application/json");
        //    //    strMsg = "Error";
        //    //}
        //    return strMsg;
        //}

        [AcceptVerbs("GET", "POST")]
        public string GeneratePaySlip(int UserKey)
        {
            return "https://images.sampletemplates.com/wp-content/uploads/2016/07/21162516/Full-Payslip-Template.jpeg";
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<string> UpdateDialer(int Key, string Status, string Remarks, string CallBackTime,
    string CallBackDate, string CallDuration, string CallDate, int UserKey,
    int IsHLLAPBL, int IsCibilBad, int IsRealEstate)
        {
            try
            {               

                Models.Pickup objBO = new Models.Pickup();
                int intRetVal = await objBO.UpdateDialerAsync(Key, Status, Remarks, CallBackTime,
                    CallBackDate, CallDuration, CallDate, UserKey, IsHLLAPBL, IsCibilBad, IsRealEstate);


                //string logPath = @"H:\logs\log.txt";
                //string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                //                  $"Key: {Key}\r\n" +
                //                  $"Status: {Status}\r\n" +
                //                  $"Duration: {CallDuration}\r\n" +
                //                  $"RetVal: {intRetVal}\r\n" +
                //                  "----------------------------------------\r\n";
                //// Use FileStream for async writing since File.AppendAllTextAsync isn't available in older frameworks
                //using (var fileStream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                //using (var streamWriter = new StreamWriter(fileStream))
                //{
                //    await streamWriter.WriteAsync(logMessage);
                //}

                return intRetVal > 0 ? "Successful" : "Error";
            }
            catch (Exception ex)
            {
                //await LogErrorAsync(ex);
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

                // Use FileStream for async writing since File.AppendAllTextAsync isn't available in older frameworks
                using (var fileStream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    await streamWriter.WriteAsync(logMessage);
                }

                return "Error";
            }
        }



        //[AcceptVerbs("GET", "POST")]
        //public string UpdateDialer(int Key, string Status, string Remarks, string CallBackTime, string CallBackDate, string CallDuration, string CallDate, int UserKey,
        //    int IsHLLAPBL, int IsCibilBad, int IsRealEstate

        //    )
        //{
        //    string strMsg = "";
        //    Models.Pickup objBO = new Models.Pickup();
        //    try
        //    {
        //        int intRetVal = objBO.UpdateDialer(Key, Status, Remarks, CallBackTime, CallBackDate, CallDuration, CallDate, UserKey, IsHLLAPBL, IsCibilBad, IsRealEstate);
        //        if (intRetVal > 0)
        //        {
        //            strMsg = "SuccessFul";
        //        }
        //        else
        //        {
        //            strMsg = "Error";
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        using (var file = new StreamWriter(@"H:\logs\" + "log.txt", true))
        //        {
        //            file.WriteLine("Testing");
        //            file.WriteLine(ex.Message);
        //            file.WriteLine(ex.StackTrace);
        //            file.Close();
        //        }
        //        strMsg = "Error";
        //    }
        //    return strMsg;
        //}

        [AcceptVerbs("GET", "POST")]
        public String AddLead(string Name, string MobileNo, string Product, string CallerID, string Remarks, int UserKey)
        {
            int i = 0;
            string result = "";
            Models.Pickup objBO = new Models.Pickup();
            DialerAPi.Models.Dialer objDialer = new Dialer();
            objDialer.CD_CLIENT_NAME = Name;
            objDialer.CD_CONTACT = MobileNo;
            objDialer.CD_PRODUCT = Product;
            objDialer.CD_CALLER_ID = CallerID;
            objDialer.CD_REMARKS = Remarks;

            try
            {
                int intRetVal = objBO.AddLead(objDialer, UserKey);
                if (intRetVal > 0)
                {
                    result = "SuccessFul";
                }
                else if (intRetVal == -1)
                {
                    result = "Lead already exists";
                }
                else
                {
                    result = "Error";
                }
            }
            catch (Exception exe)
            {
                result = exe.Message.ToString();
            }
            return result;
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetLead(int UserKey, int PageIndex)
        {
            Dialer obj = new Dialer();
            List<Dialer> lstPickup = obj.GetLead(UserKey, PageIndex);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public String InsertLeadInfo(string CD_CLIENT_NAME, string CD_CONTACT, string CD_SALARY, string CD_COMPANY, string CD_REMARKS, string CD_STATUS,
            string CD_CALLBACK_DATE, string CD_CALLBACK_TIME, string CD_CALLER_DURATION, string CD_CALL_DATE_TIME, int UserKey
            , int IsHLLAPBL, int IsCibilBad, int IsRealEstate
            )
        {
            int i = 0;
            string result = "";
            Models.Pickup objBO = new Models.Pickup();
            DialerAPi.Models.Dialer objDialer = new Dialer();
            objDialer.CD_CLIENT_NAME = CD_CLIENT_NAME;
            objDialer.CD_CONTACT = CD_CONTACT;
            objDialer.CD_SALARY = CD_CLIENT_NAME;
            objDialer.CD_COMPANY = CD_COMPANY;
            objDialer.CD_REMARKS = CD_REMARKS;
            objDialer.CD_STATUS = CD_STATUS;
            objDialer.CD_CALLBACK_DATE = CD_CALLBACK_DATE;
            objDialer.CD_CALLER_DURATION = CD_CALLER_DURATION;
            objDialer.CD_CALL_DATE_TIME = CD_CALL_DATE_TIME;
            objDialer.CD_CALLBACK_TIME = CD_CALLBACK_TIME;

            objDialer.CD_IS_HLLAPBL = IsHLLAPBL;
            objDialer.CD_IS_CIBIL_BAD = IsCibilBad;
            objDialer.CD_IS_REALESTATE = IsRealEstate;


            try
            {
                int intRetVal = objBO.InsertLeadInfo(objDialer, UserKey);
                if (intRetVal > 0)
                {
                    result = "SuccessFul";
                }
                else
                {
                    result = "Error";
                }
            }
            catch (Exception exe)
            {
                result = exe.Message.ToString();
            }
            return result;
        }



        //[AcceptVerbs("GET", "POST")]
        //public string InsertLeadInfo_New(string CD_CLIENT_NAME, string CD_CONTACT, string CD_SALARY, string CD_COMPANY, string CD_REMARKS, string CD_STATUS,
        //   string CD_CALLBACK_DATE, string CD_CALLBACK_TIME, string CD_CALLER_DURATION, string CD_CALL_DATE_TIME, int UserKey
        //   , int IsHLLAPBL, int IsCibilBad, int IsRealEstate
        //   )
        //{
        //    int intRetVal = 0;
        //    string result = "";
        //    Models.Pickup objBO = new Models.Pickup();
        //    DialerAPi.Models.Dialer objDialer = new Dialer();
        //    objDialer.CD_CLIENT_NAME = CD_CLIENT_NAME;
        //    objDialer.CD_CONTACT = CD_CONTACT;
        //    objDialer.CD_SALARY = CD_CLIENT_NAME;
        //    objDialer.CD_COMPANY = CD_COMPANY;
        //    objDialer.CD_REMARKS = CD_REMARKS;
        //    objDialer.CD_STATUS = CD_STATUS;
        //    objDialer.CD_CALLBACK_DATE = CD_CALLBACK_DATE;
        //    objDialer.CD_CALLER_DURATION = CD_CALLER_DURATION;
        //    objDialer.CD_CALL_DATE_TIME = CD_CALL_DATE_TIME;
        //    objDialer.CD_CALLBACK_TIME = CD_CALLBACK_TIME;

        //    objDialer.CD_IS_HLLAPBL = IsHLLAPBL;
        //    objDialer.CD_IS_CIBIL_BAD = IsCibilBad;
        //    objDialer.CD_IS_REALESTATE = IsRealEstate;


        //    try
        //    {
        //        intRetVal = objBO.InsertLeadInfo_New(objDialer, UserKey);
        //        //if (intRetVal > 0)
        //        //{
        //        //    result = "SuccessFul";
        //        //}
        //        //else
        //        //{
        //        //    result = "Error";
        //        //}
        //    }
        //    catch (Exception exe)
        //    {
        //        result = exe.Message.ToString();
        //        //SqlHelper.ErrorLogging(result);
        //    }
        //    return intRetVal.ToString();
        //}

        [AcceptVerbs("GET", "POST")]
        public async Task<string> InsertLeadInfo_New(
     string CD_CLIENT_NAME,
     string CD_CONTACT,
     string CD_SALARY,
     string CD_COMPANY,
     string CD_REMARKS,
     string CD_STATUS,
     string CD_CALLBACK_DATE,
     string CD_CALLBACK_TIME,
     string CD_CALLER_DURATION,
     string CD_CALL_DATE_TIME,
     int UserKey,
     int IsHLLAPBL,
     int IsCibilBad,
     int IsRealEstate)
        {
            int intRetVal = 0;
            string result = "";
            Models.Pickup objBO = new Models.Pickup();
            DialerAPi.Models.Dialer objDialer = new Dialer
            {
                CD_CLIENT_NAME = CD_CLIENT_NAME,
                CD_CONTACT = CD_CONTACT,
                CD_SALARY = CD_SALARY,  // Corrected from CD_CLIENT_NAME
                CD_COMPANY = CD_COMPANY,
                CD_REMARKS = CD_REMARKS,
                CD_STATUS = CD_STATUS,
                CD_CALLBACK_DATE = CD_CALLBACK_DATE,
                CD_CALLER_DURATION = CD_CALLER_DURATION,
                CD_CALL_DATE_TIME = CD_CALL_DATE_TIME,
                CD_CALLBACK_TIME = CD_CALLBACK_TIME,
                CD_IS_HLLAPBL = IsHLLAPBL,
                CD_IS_CIBIL_BAD = IsCibilBad,
                CD_IS_REALESTATE = IsRealEstate
            };

            try
            {
                intRetVal = await objBO.InsertLeadInfo_NewAsync(objDialer, UserKey);
                //result = intRetVal > 0 ? "Successful" : "Error";
            }
            catch (Exception ex)
            {
                result = ex.Message;
                intRetVal = 0;
                // SqlHelper.ErrorLogging(result); // Ensure you have proper logging
            }
            return intRetVal.ToString();
        }




        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetLeadnFollowupData(int UserKey, string Type)
        {
            Dialer obj = new Dialer();
            List<Dialer> lstPickup = obj.FetchLeadnFollowupData(UserKey, Type);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetCallerData(int UserKey, string Type, int Pageindex)
        {
            Dialer obj = new Dialer();
            List<Dialer> lstPickup = obj.FetchCallerData(UserKey, Type, Pageindex);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetProduct(int UserKey)
        {
            Pickup obj = new Pickup();
            List<ListItem> lst = obj.GetProduct(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lst);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetApplications(int UserKey, string SearchText)
        {


            Pickup obj = new Pickup();
            List<Application> lstPickup = obj.GetApplications(UserKey, SearchText);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetDisbursedList(int UserKey, string SearchText)
        {


            Pickup obj = new Pickup();
            List<Application> lstPickup = obj.GetDisbursedList(UserKey, SearchText);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }




        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage SearchApp(string UserKey, string SearchText)
        {
            //if (SearchText == "") {
            //    SearchText = DBNull.Value;
            //}

            Pickup obj = new Pickup();
            List<Application> lstPickup = obj.SearchApp(UserKey, SearchText);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetIncentive(int UserKey)
        {
            //if (SearchText == "") {
            //    SearchText = DBNull.Value;
            //}

            Pickup obj = new Pickup();
            List<Incentive> lstPickup = obj.GetIncentive(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
        #endregion

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetTeamCallLog(int UserKey)
        {
            //if (SearchText == "") {
            //    SearchText = DBNull.Value;
            //}

            Dialer obj = new Dialer();
            List<DialerCounter> lstDialerCounter = obj.CallLogLive(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstDialerCounter);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }



        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage DialerLive(int UserKey)
        {
            //if (SearchText == "") {
            //    SearchText = DBNull.Value;
            //}

            Dialer obj = new Dialer();
            List<DialerCounter> lstDialerCounter = obj.DialerLive(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstDialerCounter);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage DialerLiveIndv(int UserKey)
        {
            //if (SearchText == "") {
            //    SearchText = DBNull.Value;
            //}

            Dialer obj = new Dialer();
            List<DialerCounter> lstDialerCounter = obj.DialerLiveIndv(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstDialerCounter);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage DashboardCounter(int UserKey)
        {
            //if (SearchText == "") {
            //    SearchText = DBNull.Value;
            //}

            Dialer obj = new Dialer();
            List<DashboardCounter> lstDashboardCounter = obj.DashboardCounter(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstDashboardCounter);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage SendEmail(string Body, string User)
        {
            try
            {

                MailMessage mail = new MailMessage();
                var addresses = "tejasdhrona@gmail.com;sarfaraz.ali1@gmail.com;bhushanajani@gmail.com;manishkukreja2901@gmail.com;kishan1upfinpro@gmail.com";
                foreach (var address in addresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Add(address);
                }
                //mail.To.Add("tejasdhrona@gmail.com;sarfaraz.ali1@gmail.com;bhushanajani@gmail.com;manishkukreja2901@gmail.com;kishan1upfinpro@gmail.com");
                mail.From = new MailAddress("oneup.helpdesk1up@gmail.com");
                mail.Subject = "Whistle Blower " + User;

                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential
                ("oneup.helpdesk1up@gmail.com", "cip@12345");// Enter seders User name and password
                smtp.EnableSsl = true;
                smtp.Send(mail);


                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent("Email Sent", Encoding.UTF8, "application/json");
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);

        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetAttendance(int UserKey)
        {
            Dialer obj = new Dialer();
            List<Attendance> lstPickup = obj.GetAttendance(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);

        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage FetchLiveCount(int UserKey)
        {
            Dialer obj = new Dialer();
            List<LiveCount> lstPickup = obj.FetchLiveCount(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public string ValidateLoginFinans(string UserName, string Password)
        {
            //string strTest = "Testing";

            //var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(strTest, Encoding.UTF8, "application/json");
            //return response;

            //UserMaster objUserMaster = new UserMaster();
            //Finans obj = new Finans();
            //objUserMaster = obj.ValidateLoginFinans(UserName, Password);

            //var jsonSerializer = JsonConvert.SerializeObject(objUserMaster);
            //if (!string.IsNullOrEmpty(jsonSerializer))
            //{
            //    var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //    response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
            //    return response;
            //}
            //throw new HttpResponseException(HttpStatusCode.NotFound);
            return UserName + "-" + Password;
        }

        [AcceptVerbs("GET", "POST")]
        public string ManageUserMaster(UserMaster objUserMaster)
        {
            string strMsg = "";
            Finans objBO = new Finans();
            try
            {
                int intRetVal = objBO.ManageUserMaster(objUserMaster);
                if (intRetVal > 0)
                {
                    strMsg = "SuccessFul";
                }
                else
                {
                    strMsg = "Error";
                }
            }
            catch (Exception)
            {
                strMsg = "Error";
            }
            return strMsg;
        }

        [AcceptVerbs("GET", "POST")]
        public string checkDND(string MobileNo)
        {
            string strMsg = "";
            Models.Pickup objBO = new Models.Pickup();
            try
            {
                strMsg = objBO.checkDND(MobileNo);
            }
            catch (Exception)
            {
                strMsg = "Error";
            }
            return strMsg;
        }


        [AcceptVerbs("GET", "POST")]
        public string InsertDailyLogins(int Product, string Banks, string LOSNo, string AppNo, int Region, string ClientName, string Contact,
            string LoginAmt, string Remarks, int UserKey)
        {

            decimal? Login = 0;
            if (LoginAmt == "" || LoginAmt == "undefined" || LoginAmt == null)
            {
                Login = 0;
            }
            else
            {
                Login = Convert.ToDecimal(LoginAmt);
            }
            string strMsg = "";
            Models.Pickup objBO = new Models.Pickup();
            try
            {
                int intRetVal = objBO.InsertDailyLogins(Product, Banks, LOSNo, AppNo, Region, ClientName, Contact, Login, Remarks, UserKey);
                if (intRetVal > 0)
                {
                    strMsg = "Data Inserted SuccessFully";
                }
                else
                {
                    strMsg = "Error while inserting data";
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
            }
            return strMsg;
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetDailyLoginList(int UserKey, string SearchText)
        {


            Pickup obj = new Pickup();
            List<DailyLogin> lstDailyLogin = obj.GetDailyLoginList(UserKey, SearchText);

            var jsonSerializer = JsonConvert.SerializeObject(lstDailyLogin);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetLeadCount(int UserKey, int Type)
        {
            Dialer obj = new Dialer();
            List<Dialer> lstPickup = obj.GetLeadCount(UserKey, Type);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public string SaveRecording()
        {
            try
            {

                //SqlHelper.ErrorLogging("Recording Started");

                //// string Recording = Encoding.UTF8.GetString((byte[])HttpContext.Current.Request.Params["Recording"]);
                //byte[] data = Convert.FromBase64String(HttpContext.Current.Request.Params["BRecording"]);
                string assemblyFolder = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                //System.IO.File.WriteAllBytes(assemblyFolder + "/Recordings/" + HttpContext.Current.Request.Params["FileName"] + "_b64.mp3", data);
                // //HttpContext.Current.Response.ContentType = "audio/mpeg";
                // //HttpContext.Current.Response.AddHeader("content-type", HttpContext.Current.Response.ContentType);
                // //HttpContext.Current.Response.BinaryWrite(data);
                // //HttpContext.Current.Response.End();
                var httpRequest = HttpContext.Current.Request;

                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    assemblyFolder = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                    DateTime d = DateTime.Now;
                    string s = d.ToString("ddMMyyyy");
                    assemblyFolder = assemblyFolder + "/Recordings/" + HttpContext.Current.Request.Params["Caller"] + "/" + s + "/";
                    if (!Directory.Exists(assemblyFolder))
                    {
                        Directory.CreateDirectory(assemblyFolder);
                    }
                    postedFile.SaveAs(assemblyFolder + HttpContext.Current.Request.Params["FileName"] + ".mp3");








                    int Key = Convert.ToInt32(HttpContext.Current.Request.Params["CD_KEY"]);
                    string FileName = Convert.ToString(HttpContext.Current.Request.Params["FileName"]);
                    string Caller = Convert.ToString(HttpContext.Current.Request.Params["Caller"]);
                    string Contact = Convert.ToString(HttpContext.Current.Request.Params["CD_CONTACT"]);




                    string logPath = @"H:\logs\log.txt";
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                                        $"Message: {Key}\r\n" +
                                        $"FileName: {FileName}\r\n" +
                                        $"Caller: {Caller}\r\n" +
                                        $"Contact: {Contact}\r\n" +
                                        "----------------------------------------\r\n";

                    string directory = Path.GetDirectoryName(logPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(logPath, logMessage);


                    try
                    {
                        Models.Pickup objBO = new Models.Pickup();
                        int intRetVal = objBO.fnSaveRecordingDetail(Key, Caller, FileName, s, Contact);
                    }
                    catch (Exception ex)
                    {
                        //SqlHelper.ErrorLogging(ex.Message);
                        //SqlHelper.ErrorLogging(ex.StackTrace);
                    }


                }
                return "Success";
            }
            catch (Exception ex)
            {
                //SqlHelper.ErrorLogging(ex.Message);
                //SqlHelper.ErrorLogging(ex.StackTrace);
                return ex.Message;

            }

        }


        [AcceptVerbs("POST")]
        public string SaveDocuments()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                // Check if files are uploaded
                if (httpRequest.Files.Count > 0)
                {
                    HttpPostedFile postedFile = httpRequest.Files[0]; // Assuming only one file is sent
                    string FileName = httpRequest.Params["FileName"];
                    string Caller = httpRequest.Params["Caller"];
                    string MobileNo = httpRequest.Params["MobileNo"];
                    string FileType = httpRequest.Params["FileType"];
                    string Name = httpRequest.Params["Name"];
                    // Convert posted file to byte array
                    byte[] File = new byte[postedFile.ContentLength];
                    postedFile.InputStream.Read(File, 0, postedFile.ContentLength);

                    // Process the file data
                    try
                    {
                        Dialer obj = new Dialer();
                        int intRetVal = obj.fnSaveDocuments(File, Caller, FileName, MobileNo, FileType, Name);
                    }
                    catch (Exception ex)
                    {
                        // Log error or handle exception
                        return ex.Message;
                    }

                    return "Success";
                }
                else
                {
                    return "No file uploaded";
                }
            }
            catch (Exception ex)
            {
                // Log error or handle exception
                return ex.Message;
            }

        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetClientDocuments(int UserKey, string MobileNo)
        {
            Dialer obj = new Dialer();
            List<ClientDocument> lstClientDocument = obj.GetClientDocuments(UserKey, MobileNo);

            var jsonSerializer = JsonConvert.SerializeObject(lstClientDocument);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetCallerClients(int UserKey, string MobileNo)
        {
            Dialer obj = new Dialer();
            List<ClientDocument> lstClientDocument = obj.GetCallerClients(UserKey, MobileNo);

            var jsonSerializer = JsonConvert.SerializeObject(lstClientDocument);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetSpinConfig(int UserKey)
        {
            Models.Pickup obj = new Models.Pickup();
            List<SpinWheelConfig> lstPickup = obj.GetSpinConfig(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstPickup);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetSpinCount(int UserKey)
        {
            using (var file = new StreamWriter(@"H:\logs\" + "log.txt", true))
            {
                file.WriteLine("Testing");
                file.WriteLine(UserKey.ToString());
                file.Close();
            }
            Models.Pickup obj = new Models.Pickup();
            int getSpinCount = obj.GetSpinCount(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(getSpinCount);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage InsertSpinWheelHistory(int userkey, int prizeid)
        {
            Models.Pickup obj = new Models.Pickup();
            int getSpinCount = obj.InsertSpinWheelHistory(userkey, prizeid);


            var jsonSerializer = JsonConvert.SerializeObject(getSpinCount);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GeneratePdf([FromBody] PdfRequest request)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4);
                    iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    doc.Add(new iTextSharp.text.Paragraph(request.Title) { Font = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 18) });
                    doc.Add(new iTextSharp.text.Paragraph(request.Content) { Font = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12) });

                    doc.Close();
                    writer.Close();

                    byte[] pdfBytes = ms.ToArray();

                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(pdfBytes)
                    };
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "GeneratedDocument.pdf"
                    };

                    return response;
                }
            }
            catch (System.Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error generating PDF: " + ex.Message);
            }
        }


        public static void ReadError()
        {
            string strPath = @"D:\Rekha\Log.txt";
            using (StreamReader sr = new StreamReader(strPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }

        private async Task LogErrorAsync(Exception ex)
        {
            string logPath = @"H:\logs\errors.txt";
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Error: {ex.Message}\r\n" +
                               $"Stack Trace: {ex.StackTrace}\r\n" +
                               "----------------------------------------\r\n";

            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Use FileStream for async writing since File.AppendAllTextAsync isn't available in older frameworks
            using (var fileStream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                await streamWriter.WriteAsync(logMessage);
            }
        }

    }

    public class Message
    {
        public string Description { get; set; }
    }

    public class DialerUpdateRequest
    {
        public int Key { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string CallBackTime { get; set; }
        public string CallBackDate { get; set; }
        public string CallDuration { get; set; }
        public string CallDate { get; set; }
        public int UserKey { get; set; }
        public bool IsHLLAPBL { get; set; }
        public bool IsCibilBad { get; set; }
        public bool IsRealEstate { get; set; }
    }

    public class PdfRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
