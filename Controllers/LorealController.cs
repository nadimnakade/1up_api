using DialerAPi.Models;
using Newtonsoft.Json;
using PickupAPi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PickupAPi.Controllers
{
    [EnableCors(origins: "http://103.137.92.198:8080", headers: "*", methods: "*")]
    public class LorealController : ApiController
    {
        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage ValidateLogin(string UserName, string UUID)
        {

            List<Employee> lstEmployee = new List<Employee>();
            Models.vDialer obj = new Models.vDialer();
            lstEmployee = obj.LoginCheck(UserName, UUID);

            var jsonSerializer = JsonConvert.SerializeObject(lstEmployee);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

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
        public HttpResponseMessage FetchLiveCount(int UserKey)
        {
            vDialer obj = new vDialer();
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
        public String InsertLeadInfo(string CD_CLIENT_NAME, string CD_CONTACT, string CD_ADDRESS, string CD_PINCODE, string CD_REMARKS, string CD_STATUS,
            string CD_CALLBACK_DATE, string CD_CALLBACK_TIME, string CD_CALLER_DURATION, string CD_CALL_DATE_TIME, string CD_PRODUCT, int UserKey

            )
        {
            int i = 0;
            string result = "";
            vDialer objBO = new vDialer();
            Dialer objDialer = new Dialer();
            objDialer.CD_CLIENT_NAME = CD_CLIENT_NAME;
            objDialer.CD_CONTACT = CD_CONTACT;
            objDialer.CD_PINCODE = CD_PINCODE;
            objDialer.CD_ADDRESS = CD_ADDRESS;
            objDialer.CD_REMARKS = CD_REMARKS;
            objDialer.CD_STATUS = CD_STATUS;
            objDialer.CD_PRODUCT = CD_PRODUCT;
            objDialer.CD_CALLBACK_DATE = CD_CALLBACK_DATE;
            objDialer.CD_CALLER_DURATION = CD_CALLER_DURATION;
            objDialer.CD_CALL_DATE_TIME = CD_CALL_DATE_TIME;
            objDialer.CD_CALLBACK_TIME = CD_CALLBACK_TIME;

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


        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetLeadnFollowupData(int UserKey, string Type)
        {
            vDialer obj = new vDialer();
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

        //[AcceptVerbs("GET", "POST")]
        //public string UpdateDialingStatus(int Key)
        //{
        //    Models.Pickup objBO = new Models.Pickup();
        //    string strMsg = "";
        //    strMsg = objBO.UpdateDialingStatus(Key);

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
        //public string UpdateDialer(int Key, string Status, string Remarks, string CallBackTime, string CallBackDate, string CallDuration, string CallDate,

        //    int UserKey, int IsHLLAPBL,

        //    int IsCibilBad, int IsRealEstate)
        //{
        //    string strMsg = "";
        //    Models.Pickup objBO = new Models.Pickup();
        //    try
        //    {
        //        int intRetVal = objBO.UpdateDialer(Key, Status, Remarks, CallBackTime, CallBackDate, CallDuration, CallDate, UserKey, IsHLLAPBL,
        //            IsCibilBad, IsRealEstate);
        //        if (intRetVal > 0)
        //        {
        //            strMsg = "SuccessFul";
        //        }
        //        else
        //        {
        //            strMsg = "Error";
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        strMsg = "Error";
        //    }
        //    return strMsg;
        //}



        //[AcceptVerbs("GET", "POST")]
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


        


    }
}
