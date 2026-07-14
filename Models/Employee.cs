using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PickupAPi.Models
{
    public class Employee
    {
        public int Key { get; set; }
        public int EmployeeNo { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Int64 DesignationID { get; set; }
        public string DesignationDesc { get; set; }
        public Int64 RoleID { get; set; }
        public string RoleDesc { get; set; }
        public decimal Salary { get; set; }
        public Int64 BossId { get; set; }
        public string BossName { get; set; }
        public Int64 LocationId { get; set; }
        public string LocationDesc { get; set; }
        public Int64 LocationHeadId { get; set; }
        public Int64 ReportsTo { get; set; }
        public string ReportsToName { get; set; }

        public Int64 ReportingManagerID { get; set; }
        public string ReportingManagerName { get; set; }

        public Int64 ProgrammeManagerID { get; set; }
        public string ProgrammeManagerName { get; set; }

        public string DateOfJoining { get; set; }
        public string DateOfBirth { get; set; }
        public string Address { get; set; }
        public Int64 OfficeContactNo { get; set; }
        public Int64 MobileNo1 { get; set; }
        public Int64 MobileNo2 { get; set; }
        public Int64 ProductKey { get; set; }
        public string ProductName { get; set; }
        public string BankAccountNo { get; set; }
        public string ReferredBy { get; set; }
        public string LastWorkingDate { get; set; }
        public string EmailId { get; set; }
        public string PanNo { get; set; }
        public string AadharNo { get; set; }
        public string EmployeeType { get; set; }

        public string TLDesignationDesc { get; set; }
        public string BHDesignationDesc { get; set; }
        public string BHLocationDesc { get; set; }


        public string Absentdays { get; set; }
        public string Late { get; set; }
        public string Halfday { get; set; }
        public decimal Salarytobegiven { get; set; }




        public decimal Points { get; set; }
        public decimal PLMP { get; set; }
        public decimal PLNMP { get; set; }

        public decimal PLRefferal { get; set; }
        public decimal APLRefferal { get; set; }

        public decimal HL { get; set; }
        public decimal BL { get; set; }
        public decimal CC { get; set; }
        public decimal APLMP { get; set; }
        public decimal APLNMP { get; set; }
        public decimal HLAmt { get; set; }
        public decimal BLAmt { get; set; }
        public decimal CCAmt { get; set; }
        public decimal Incentives { get; set; }
        public string Rank { get; internal set; }
        public string PhotoURL { get; internal set; }
    }
}