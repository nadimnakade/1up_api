using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;

using Newtonsoft.Json;
using PickupAPi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web;
using TheArtOfDev.HtmlRenderer.WinForms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.css;
using System.Net.Http.Headers;
using DialerAPi.Models;
using System.Web.Script.Serialization;
using System.Web.Http.Cors;
using System.Threading.Tasks;

namespace PickupAPi.Controllers
{
    public class UpdateStatusController : ApiController
    {
        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        [AcceptVerbs("GET", "POST")]
        public string PostCreateUser(string MobileNo, string UDID, string Type)
        {
            //return MobileNo + "-" + UDID + "-" + Type;
            Models.Pickup obj = new Models.Pickup();

            //string MobileNo = "", UDID = "", Type = "";
            //MobileNo = parameter.MobileNo;
            //UDID = parameter.UDID;
            //Type = parameter.Type;
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



        public class UploadCustomerImageModel
        {
            public string Key { get; set; }

            public string ImageData { get; set; }

        }



        [AcceptVerbs("GET", "POST")]
        public string PostUpdateStatus(string Key, string CallerRemarks, string Status, string CallBackDate,
            string CallBackTime, string Lat, string Long)
        {
            string strMsg = "", base64 = "";
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
                    //response.Content = new StringContent("SuccessFul", Encoding.UTF8, "application/json");
                    strMsg = "SuccessFul";
                }
                else
                {
                    //response.Content = new StringContent("Error", Encoding.UTF8, "application/json");
                    strMsg = "Error";
                }
                //return response;
            }
            catch (Exception)
            {
                strMsg = "Error";
            }
            //throw new HttpResponseException(HttpStatusCode.NotFound);
            return strMsg;
        }

        [AcceptVerbs("GET", "POST")]
        public string UpdateDiallingTime(int Key)
        {
            Models.Pickup objBO = new Models.Pickup();
            string strMsg = "";
            strMsg = objBO.UpdateDiallingTime(Key);

            if (strMsg == "Success")
            {
                //response.Content = new StringContent("SuccessFul", Encoding.UTF8, "application/json");
                strMsg = "SuccessFul";
            }
            //else
            //{
            //    //response.Content = new StringContent("Error", Encoding.UTF8, "application/json");
            //    strMsg = "Error";
            //}
            return strMsg;
        }


        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage ValidateLogin(string UserName, string Password, string UUID, string Type)
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


        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetCaller(int UserKey)
        {
            //string strTest = "Testing";

            //var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(strTest, Encoding.UTF8, "application/json");
            //return response;

            List<Models.ListItem> lstEmployee = new List<Models.ListItem>();
            Models.Pickup obj = new Models.Pickup();
            lstEmployee = obj.GetCaller(UserKey);

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
        public string AssignPickup(int PickupKey, int AssignedTo, int UserKey)
        {
            string strMsg = "";
            Models.Pickup objBO = new Models.Pickup();
            try
            {


                int intRetVal = objBO.AssignPickup(PickupKey, AssignedTo, UserKey);
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
        public HttpResponseMessage GetPickup(int UserKey)
        {
            //string strTest = "Testing";

            //var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(strTest, Encoding.UTF8, "application/json");
            //return response;

            List<Models.ListItem> lstEmployee = new List<Models.ListItem>();
            Models.Pickup obj = new Models.Pickup();
            lstEmployee = obj.GetPickup(UserKey);

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
        public string Login(string UserName, string Password, string UUID)
        {
            string strMsg = "", base64 = "";
            Models.Pickup objBO = new Models.Pickup();
            try
            {
                Models.UpdateStatus obj = new Models.UpdateStatus();

                int intRetVal = objBO.UpdateStatus(obj);
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                if (intRetVal > 0)
                {
                    //response.Content = new StringContent("SuccessFul", Encoding.UTF8, "application/json");
                    strMsg = "SuccessFul";
                }
                else
                {
                    //response.Content = new StringContent("Error", Encoding.UTF8, "application/json");
                    strMsg = "Error";
                }
                //return response;
            }
            catch (Exception)
            {
                strMsg = "Error";
            }
            //throw new HttpResponseException(HttpStatusCode.NotFound);
            return strMsg;
        }



        [AcceptVerbs("GET", "POST")]
        public string PostDataEntry(int PickupKey, string Name, string MobileNo, string Banks, string PickupType, string Product, string CallerUserID, string PickupFrom,
                                    string PickupTo, string PickupDate, string PickupRegion, string Address, string Remarks, int UserKey)
        {
            string strMsg = "";
            Models.Pickup objBO = new Models.Pickup();
            try
            {
                Models.Pickup obj = new Models.Pickup();

                obj.PickupKey = PickupKey;
                obj.Name = Name;
                obj.MobileNo = MobileNo;
                obj.Banks = Banks;
                obj.PickupType = PickupType;
                obj.Product = Product;
                obj.PickupTimeFrom = PickupFrom;
                obj.PickupTimeTo = PickupTo;
                obj.CallerUserID = CallerUserID;
                obj.PickupDate = PickupDate;
                obj.LoginRegion = PickupRegion;
                obj.Address = Address;
                obj.Remarks = Remarks;

                int intRetVal = objBO.Manage(obj, UserKey);
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
        public string EMI(string LoanAmt, string Months, string ROI)
        {


            var bb = Convert.ToDouble(LoanAmt);
            var numberOfMonths = Convert.ToInt32(Months);
            var rateOfInterest = Convert.ToDouble(ROI);

            var loanAmount = Convert.ToDouble(LoanAmt);

            var monthlyInterestRatio = Convert.ToDouble((rateOfInterest / 100) / 12);

            var top = Math.Pow((1 + monthlyInterestRatio), numberOfMonths);
            var bottom = top - 1;
            var sp = top / bottom;
            var emi = Convert.ToDouble(((loanAmount * monthlyInterestRatio) * sp));
            var full = numberOfMonths * emi;
            var interest = full - loanAmount;
            var int_pge = (interest / full) * 100;

            //$("#tbl_loan_pge").html((100-int_pge.toFixed(2))+" %");

            //var emi_str = emi.toFixed(2).toString().replace(/,/ g, "").replace(/\B(?= (\d{ 3})+(? !\d))/ g, ",");
            //var loanAmount_str = loanAmount.toString().replace(/,/ g, "").replace(/\B(?= (\d{ 3})+(? !\d))/ g, ",");
            //var full_str = full.toFixed(2).toString().replace(/,/ g, "").replace(/\B(?= (\d{ 3})+(? !\d))/ g, ",");
            //var int_str = interest.toFixed(2).toString().replace(/,/ g, "").replace(/\B(?= (\d{ 3})+(? !\d))/ g, ",");

            string css = "<style>.table{width:100%;max-width:100%;margin-bottom:1rem}.tabletd,.tableth{padding:.75rem;vertical-align:top;border-top:1pxsolid#eceeef}.tabletheadth{vertical-align:bottom;border-bottom:2pxsolid#eceeef}.tabletbody+tbody{border-top:2pxsolid#eceeef}.table.table{background-color:#fff}.table-smtd,.table-smth{padding:.3rem}.table-bordered{border:1pxsolid#eceeef}.table-borderedtd,.table-borderedth{border:1pxsolid#eceeef}.table-borderedtheadtd,.table-borderedtheadth{border-bottom-width:2px}.table-stripedtbodytr:nth-of-type(odd){background-color:rgba(0,0,0,.05)}.table-hovertbodytr:hover{background-color:rgba(0,0,0,.075)}.table-active,.table-active>td,.table-active>th{background-color:rgba(0,0,0,.075)}.table-hover.table-active:hover{background-color:rgba(0,0,0,.075)}.table-hover.table-active:hover>td,.table-hover.table-active:hover>th{background-color:rgba(0,0,0,.075)}.table-success,.table-success>td,.table-success>th{background-color:#dff0d8}.table-hover.table-success:hover{background-color:#d0e9c6}.table-hover.table-success:hover>td,.table-hover.table-success:hover>th{background-color:#d0e9c6}.table-info,.table-info>td,.table-info>th{background-color:#d9edf7}.table-hover.table-info:hover{background-color:#c4e3f3}.table-hover.table-info:hover>td,.table-hover.table-info:hover>th{background-color:#c4e3f3}.table-warning,.table-warning>td,.table-warning>th{background-color:#fcf8e3}.table-hover.table-warning:hover{background-color:#faf2cc}.table-hover.table-warning:hover>td,.table-hover.table-warning:hover>th{background-color:#faf2cc}.table-danger,.table-danger>td,.table-danger>th{background-color:#f2dede}.table-hover.table-danger:hover{background-color:#ebcccc}.table-hover.table-danger:hover>td,.table-hover.table-danger:hover>th{background-color:#ebcccc}.thead-inverseth{color:#fff;background-color:#292b2c}.thead-defaultth{color:#464a4c;background-color:#eceeef}.table-inverse{color:#fff;background-color:#292b2c}.table-inversetd,.table-inverseth,.table-inversetheadth{border-color:#fff}.table-inverse.table-bordered{border:0}.table-responsive{display:block;width:100%;overflow-x:auto;-ms-overflow-style:-ms-autohiding-scrollbar}.table-responsive.table-bordered{border:0}</style>";



            string html = css + "<table style='border:1px solid #ddd;width: 100%;max-width: 100%;border-collapse: collapse '><thead style='text-align:center;background-color:#5b5ef4;padding: 8px;color:white;vertical-align: bottom;border-bottom: 2px solid #eceeef;'><tr><th>No.</th><th>Opening Bal.</th><th>EMI</th><th>Principal</th><th>Interest</th><th>Bal.</th></tr></thead><tbody>";

            CultureInfo hindi = new CultureInfo("hi-IN");

            var int_dd = 0.0; var pre_dd = 0.0; var end_dd = 0.0;
            for (var j = 1; j <= numberOfMonths; j++)
            {
                int_dd = bb * ((rateOfInterest / 100) / 12);
                pre_dd = emi - int_dd;
                end_dd = bb - pre_dd;


                var loanAmt = Math.Round(bb, 2);//  Math.Round( decimal.Parse(Convert.ToDecimal(bb).ToString(), CultureInfo.InvariantCulture) ,2).ToString();
                var strpre_dd = Math.Round(pre_dd, 2);  //Math.Round(decimal.Parse(Convert.ToDecimal(pre_dd).ToString(), CultureInfo.InvariantCulture),2).ToString();
                var strend_dd = Math.Round(end_dd, 2);//Math.Round(decimal.Parse(Convert.ToDecimal(end_dd).ToString(), CultureInfo.InvariantCulture),2).ToString();
                string bordercss = "padding: 8px;border: 1px solid #ddd; text-align:right";
                //<b>&#8377;</b><img style='width:12px;height:12px' src='https://cdn3.iconfinder.com/data/icons/indian-rupee-symbol/128/Indian_Rupee_symbol.png' />
                html += "<tr><td style='" + bordercss + "'>" + j + "</td><td style='" + bordercss + "'><b>&#8377;</b>" + string.Format(hindi, "{0:c}", bb) + "</td><td style='" + bordercss + "'>" + Math.Round(emi, 2).ToString() +
                    "</td><td style='" + bordercss + "'>" + string.Format(hindi, "{0:c}", strpre_dd) + "</td><td style='" + bordercss + "'>" + Math.Round(int_dd, 2).ToString() + "</td><td style='" + bordercss + "'><b>&#8377;</b>" + string.Format(hindi, "{0:c}", strend_dd) + "</td></tr>";
                bb = bb - pre_dd;
            }

            html += "</tbody></table>";


            string targetPath = "", filename = "";


            byte[] pdf; // result will be here

            var cssText = File.ReadAllText(HttpContext.Current.Server.MapPath("~/content/site.css"));


            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 60, 60);
                var writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();


                using (var cssMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cssText)))
                {
                    using (var htmlMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html)))
                    {
                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, htmlMemoryStream, cssMemoryStream);
                    }
                }
                string targetFolder = HttpContext.Current.Server.MapPath("~/uploads/");
                filename = "EMI" + DateTime.Now.ToString("ddMMyyyyHHss") + ".pdf";
                targetPath = Path.Combine(targetFolder, filename);

                document.Close();

                pdf = memoryStream.ToArray();

                System.IO.File.WriteAllBytes(targetPath, pdf);

            }
            return filename;
        }


        [AcceptVerbs("GET", "POST")]
        public string PostUpdateImage([FromBody] UploadCustomerImageModel model)
        {
            return "Successful" + model.Key + "-" + model.ImageData;
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetLead(int UserKey, string SearchText, int PageSize, int PageIndex)
        {
            //string strTest = "Testing";

            //var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(strTest, Encoding.UTF8, "application/json");
            //return response;

            List<LeadMaster> lstLeadMaster = new List<LeadMaster>();
            Models.Pickup obj = new Models.Pickup();
            lstLeadMaster = obj.GetLeadList(UserKey, SearchText, PageSize, PageIndex);

            var jsonSerializer = JsonConvert.SerializeObject(lstLeadMaster);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public string ManageLead(int Key, string Name, string EmailID, string MobileNo, string Address, string ProductKey, int UserKey)
        {
            string strMsg = "";
            Models.Pickup objBO = new Models.Pickup();

            LeadMaster LM = new LeadMaster();
            LM.Key = Convert.ToInt32(Key);
            LM.Name = Name;
            LM.EmailID = EmailID;
            LM.MobileNo = (MobileNo);
            LM.Address = Address;
            LM.Product = Convert.ToInt32(ProductKey);
            LM.CreatedBy = UserKey;

            try
            {
                int intRetVal = objBO.ManageLead(LM);
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

        [HttpGet]
        public HttpResponseMessage Generate()
        {
            var path = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/EMI.pdf"); ;
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = Path.GetFileName(path);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentLength = stream.Length;
            return result;
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetRanking(int UserKey)
        {
            //string strTest = "Testing";

            //var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(strTest, Encoding.UTF8, "application/json");
            //return response;

            List<Employee> lstEmployee = new List<Employee>();
            Dialer obj = new Dialer();
            lstEmployee = obj.GetRanking(UserKey);

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
        public HttpResponseMessage GetRankingOld(int UserKey)
        {
            List<Employee> lstEmployee = new List<Employee>();
            Dialer obj = new Dialer();
            lstEmployee = obj.GetRankingOld(UserKey);

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
        public HttpResponseMessage GetPerformance(int UserKey)
        {


            List<Performance> lstPerformance = new List<Performance>();
            Dialer obj = new Dialer();
            lstPerformance = obj.GetPerformance(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstPerformance);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public int IsAttendanceMarked(int UserKey, int Type)
        {
            Dialer obj = new Dialer();
            int intRetVal = obj.IsAttendanceMarked(UserKey, Type);
            return intRetVal;
        }

        public HttpResponseMessage GetClientsBirthday(int UserKey)
        {
            List<ClientDtl> lstClientDtl = new List<ClientDtl>();
            Dialer obj = new Dialer();
            lstClientDtl = obj.GetClientsBirthday(UserKey);

            var jsonSerializer = JsonConvert.SerializeObject(lstClientDtl);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

      

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage FnSaveCallLogs(List<CallLogList> lstCallLog)
        {
            string strMsg = "";
            Dialer obj = new Dialer();
            int intRetVal = obj.FnSaveCallLogs(lstCallLog);
            if (intRetVal > 0)
            {
                strMsg = "Ok";
            }
            else
            {
                strMsg = "Not Ok";
            }

            var jsonSerializer = JsonConvert.SerializeObject(strMsg);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetDisbursalDtl(int UserKey, string LoanId)
        {
            Dialer obj = new Dialer();
            List<Application> lstPickup = obj.GetDisbursalDtl(UserKey, LoanId);

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
        public HttpResponseMessage FnSaveCallLogsAll([FromBody] List<CallLogList> CallLog)
        {


            string strMsg = "";
            Dialer obj = new Dialer();

            int intRetVal = obj.FnSaveCallLogsAll(CallLog);
            if (intRetVal > 0)
            {
                strMsg = "Ok";
            }
            else
            {
                strMsg = "Not Ok";
            }

            var jsonSerializer = JsonConvert.SerializeObject(strMsg);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage FnIndvCallLog([FromBody] List<CallLogList> CallLog)
        {


            string strMsg = "";
            Dialer obj = new Dialer();

            int intRetVal = obj.FnIndvCallLog(CallLog);
            if (intRetVal > 0)
            {
                strMsg = "Ok";
            }
            else
            {
                strMsg = "Not Ok";
            }

            var jsonSerializer = JsonConvert.SerializeObject(strMsg);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage GetLoginDtls(int UserKey, int Type)
        {
            Dialer obj = new Dialer();
            List<DailyLogin> lstDailyLogin = obj.GetLoginDtls(UserKey, Type);

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
        public HttpResponseMessage FnSaveCallLogsIndv(string date, string type, string duration, string name, string number, int userkey)
        {
            string strMsg = "";
            Dialer obj = new Dialer();

            CallLogList objCallLogList = new CallLogList();
            objCallLogList.name = name;
            objCallLogList.duration = duration;
            objCallLogList.date = date;
            objCallLogList.number = number;
            objCallLogList.userkey = userkey;
            objCallLogList.type = type;
            int intRetVal = obj.FnSaveCallLogsIndv(objCallLogList);
            if (intRetVal > 0)
            {
                strMsg = "Ok";
            }
            else
            {
                strMsg = "Not Ok";
            }

            var jsonSerializer = JsonConvert.SerializeObject(strMsg);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage ValidateLoginEmployee(string UserName)
        {
            //string strTest = "Testing";

            //var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(strTest, Encoding.UTF8, "application/json");
            //return response;

            Employee lstEmployee = new Employee();
            Dialer obj = new Dialer();
            lstEmployee = obj.LoginCheck(UserName);

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
        public HttpResponseMessage fnSaveVersion(int UserKey, string Version)
        {
            string strMsg = "";
            Dialer obj = new Dialer();
            int intRetVal = obj.fnSaveVersion(UserKey,Version);
            
            if (intRetVal > 0)
            {
                strMsg = "Ok";
            }
            else
            {
                strMsg = "Not Ok";
            }

            var jsonSerializer = JsonConvert.SerializeObject(strMsg);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        public bool PostMakeBooking(Parameter parameter)
        {
            return true;
        }

        [AcceptVerbs("GET", "POST")]
        //public string SaveRecording()
        //{
        //    try
        //    {

        //        ErrorLogging("Recording Started");

        //        //// string Recording = Encoding.UTF8.GetString((byte[])HttpContext.Current.Request.Params["Recording"]);
        //        //byte[] data = Convert.FromBase64String(HttpContext.Current.Request.Params["BRecording"]);
        //        string assemblyFolder = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        //        //System.IO.File.WriteAllBytes(assemblyFolder + "/Recordings/" + HttpContext.Current.Request.Params["FileName"] + "_b64.mp3", data);
        //        // //HttpContext.Current.Response.ContentType = "audio/mpeg";
        //        // //HttpContext.Current.Response.AddHeader("content-type", HttpContext.Current.Response.ContentType);
        //        // //HttpContext.Current.Response.BinaryWrite(data);
        //        // //HttpContext.Current.Response.End();
        //        var httpRequest = HttpContext.Current.Request;

        //        foreach (string file in httpRequest.Files)
        //        {
        //            var postedFile = httpRequest.Files[file];
        //            assemblyFolder = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        //            DateTime d = DateTime.Now;
        //            string s = d.ToString("ddMMyyyy");
        //            assemblyFolder = assemblyFolder + "/Recordings/" + HttpContext.Current.Request.Params["Caller"] + "/" + s + "/";
        //            if (!Directory.Exists(assemblyFolder))
        //            {
        //                Directory.CreateDirectory(assemblyFolder);
        //            }
        //            postedFile.SaveAs(assemblyFolder + HttpContext.Current.Request.Params["FileName"] + ".mp3");


        //            int Key = Convert.ToInt32(HttpContext.Current.Request.Params["CD_KEY"]);
        //            string FileName = Convert.ToString(HttpContext.Current.Request.Params["FileName"]);
        //            string Caller = Convert.ToString(HttpContext.Current.Request.Params["Caller"]);
        //            string Contact = Convert.ToString(HttpContext.Current.Request.Params["CD_CONTACT"]);

        //            try
        //            {
        //                Models.Pickup objBO = new Models.Pickup();
        //                int intRetVal = objBO.fnSaveRecordingDetail(Key, Caller, FileName, s, Contact);
        //            }
        //            catch (Exception ex)
        //            {
        //                ErrorLogging(ex.Message);
        //                ErrorLogging(ex.StackTrace);
        //            }


        //        }
        //        return "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogging(ex.Message);
        //        ErrorLogging(ex.StackTrace);
        //        return ex.Message;

        //    }

        //}

        public string SaveRecording()
        {
            try
            {
                ErrorLogging("Recording Started");

                var httpRequest = HttpContext.Current.Request;

                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];

                    // Save recordings to H:\logs
                    string baseFolder = @"H:\IIS\Recordings";
                    DateTime d = DateTime.Now;
                    string dateFolder = d.ToString("ddMMyyyy");
                    string targetFolder = Path.Combine(baseFolder, HttpContext.Current.Request.Params["Caller"], dateFolder);

                    // Ensure the directory exists
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    // Save the uploaded file to the target location
                    string targetFilePath = Path.Combine(targetFolder, HttpContext.Current.Request.Params["FileName"] + ".mp3");
                    postedFile.SaveAs(targetFilePath);

                    // Log details in the database
                    int key = Convert.ToInt32(HttpContext.Current.Request.Params["CD_KEY"]);
                    string fileName = Convert.ToString(HttpContext.Current.Request.Params["FileName"]);
                    string caller = Convert.ToString(HttpContext.Current.Request.Params["Caller"]);
                    string contact = Convert.ToString(HttpContext.Current.Request.Params["CD_CONTACT"]);

                    try
                    {
                        Models.Pickup objBO = new Models.Pickup();
                        int intRetVal = objBO.fnSaveRecordingDetail(key, caller, fileName, dateFolder, contact);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogging("Database Error: " + ex.Message);
                        ErrorLogging(ex.StackTrace);
                    }
                }

                return "Success";
            }
            catch (Exception ex)
            {
                ErrorLogging("General Error: " + ex.Message);
                ErrorLogging(ex.StackTrace);
                return ex.Message;
            }
        }


        //public HttpResponseMessage fnSaveClientInfo(ClientInfo clientInfo)
        //{
        //    string strMsg = "";
        //    Dialer obj = new Dialer();
        //    int intRetVal = obj.fnSaveClientInfo(clientInfo.FullName, clientInfo.Email, clientInfo.MobileNo,
        //                                      clientInfo.PinCode, clientInfo.LoanType);

        //    if (intRetVal > 0)
        //    {
        //        strMsg = "Ok";
        //    }
        //    else
        //    {
        //        strMsg = "Not Ok";
        //    }

        //    var jsonSerializer = JsonConvert.SerializeObject(strMsg);
        //    if (!string.IsNullOrEmpty(jsonSerializer))
        //    {
        //        var response = this.Request.CreateResponse(HttpStatusCode.OK);
        //        response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
        //        return response;
        //    }
        //    throw new HttpResponseException(HttpStatusCode.NotFound);
        //}
        [EnableCors(origins: "http://secondopinion.co.in:8090", headers: "*", methods: "*")]
        public HttpResponseMessage fnSaveClientInfo(ClientInfo clientInfo)
        {
            string strMsg = "";
            Dialer obj = new Dialer();
            int intRetVal = obj.fnSaveClientInfo(clientInfo.FullName, clientInfo.Email, clientInfo.MobileNo,
                                              clientInfo.PinCode, clientInfo.LoanType);

            if (intRetVal > 0)
            {
                strMsg = "Ok";
            }
            else
            {
                strMsg = "Not Ok";
            }

            var jsonSerializer = JsonConvert.SerializeObject(strMsg);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [EnableCors(origins: "http://secondopinion.co.in:8090", headers: "*", methods: "*")]
        public HttpResponseMessage LoanEligibility(LoanEligibility.Header objHeader)
        {

            Dialer obj = new Dialer();
            List<Eligibility> lst = obj.LoanEligibility(objHeader);
            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };

            var jsonSerializer = JsonConvert.SerializeObject(lst);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [EnableCors(origins: "http://secondopinion.co.in:8090", headers: "*", methods: "*")]
        public HttpResponseMessage FetchCategory(string Key)
        {
            Dialer obj = new Dialer();
            List<PickupAPi.Models.ListItem> lst = obj.ListCompanyCategory(Key);

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
        public HttpResponseMessage GetSpinConfig(int UserKey)
        {
            Dialer obj = new Dialer();
            List<SpinWheelConfig> lstDailyLogin = obj.GetSpinConfig(UserKey);

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
        public async Task<HttpResponseMessage> fnSaveCallLogsUtilities(List<CallLogUtilitiesList> lstCallLog)
        {
            if (lstCallLog == null || lstCallLog.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No call logs provided.");
            }

            for (int i = 0; i < lstCallLog.Count; i++)
            {
                using (var file = new StreamWriter(@"H:\logs\" + "log.txt", true))
                {
                    file.WriteLine("Testing");
                    file.WriteLine(lstCallLog[i].name.ToString());                    
                    file.WriteLine(lstCallLog[i].phoneNumber.ToString());
                    file.WriteLine(lstCallLog[i].duration.ToString());
                    file.Close();
                }
            }
            string strMsg = "";
            Pickup obj = new Pickup();
            int intRetVal = await obj.fnSaveCallLogsUtilities(lstCallLog);
            if (intRetVal > 0)
            {
                strMsg = "Ok";
            }
            else
            {
                strMsg = "Not Ok";
            }

            var jsonSerializer = JsonConvert.SerializeObject(strMsg);
            if (!string.IsNullOrEmpty(jsonSerializer))
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(jsonSerializer, Encoding.UTF8, "application/json");
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }



        public static void ErrorLogging(string msg)
        {
            string strPath = @"C:\Error\Log.txt";
            if (!File.Exists(strPath))
            {
                File.Create(strPath).Dispose();
            }
            using (StreamWriter sw = File.AppendText(strPath))
            {
                sw.WriteLine("=============Error Logging ===========");
                sw.WriteLine("===========Start============= " + DateTime.Now);
                sw.WriteLine("Error Message: " + msg);
                sw.WriteLine("Stack Trace: " + msg);
                sw.WriteLine("===========End============= " + DateTime.Now);

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
    }
}


public class SpinWheelConfig
{
    public int id { get; set; }
    public double amount { get; set; }
    public double chance { get; set; }
    public string fillStyle { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string description { get; set; }
    public string IconName { get; set; }
    public string textFontSize { get; set; }
    public string textColor { get; set; }
    public string textFontWeight { get; set; }
}

public class SpinWheelHistory
{
    public int Id { get; set; }
    public int UserKey { get; set; }
    public int PrizeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime SpinDate { get; set; }
    public int CustomerMobileNo { get; set; }
}
