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
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PickupAPi.Controllers
{
    public class SecondOpinionController : ApiController
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

        

      
    }
}


public class ClientInfo
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string MobileNo { get; set; }
    public string PinCode { get; set; }
    public string LoanType { get; set; }
    public int UserKey { get; set; }
    public string Version { get; set; }
}

public class LoanEligibility
{

    public class Header 
    {
        public string EH_NAME { get; set; }
        public decimal EH_NET_SALARY { get; set; }
        public decimal EH_OBLIGATION { get; set; }
        public decimal EH_NET_GROSS { get; set; }
        public int EH_COMPANY_NAME_KEY { get; set; }
        public decimal EH_ROI { get; set; }
        public int EH_TENURE { get; set; }
        public string EH_CATEGORY { get; set; }
        public Detail lstDtl { get; set; }
        public int Key { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool IsActive { get; set; }
    }


    public class Detail
    {
        public int ED_EG_KEY { get; set; }
        public decimal ED_AMT { get; set; }
        public int ED_LOAN_TYPE { get; set; }

        public int Key { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool IsActive { get; set; }
    }
}

public class Eligibility 
{

    public string Bank { get; set; }
    public string Category { get; set; }
    public string Amt { get; set; }
    public string Slab { get; set; }

    public int Key { get; set; }
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; }
    public DateTime CreatedOn { get; set; }
    public int ModifiedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public bool IsActive { get; set; }
}



