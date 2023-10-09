using MVC_SYSTEM.LabourModels;
using MVC_SYSTEM.MasterModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MVC_SYSTEM.Class
{
    public class GeneralFunc
    {
        private GetNSWL GetNSWL = new GetNSWL();
        private MVC_SYSTEM_MasterModels db = new MVC_SYSTEM_MasterModels();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();

        public string BatchNoFunc(int? NeragaID, int? SyarikatID, int? WilayahID, int? LadangID, string BatchWord, string BatchFlag, MVC_SYSTEM_Models dbr)
        {
            var GetNSWLDetail = GetNSWL.GetLadangDetail(LadangID.Value);
            var getbatchno = db.tbl_BatchRunNo.Where(x => x.fld_BatchFlag == BatchFlag && x.fld_NegaraID == GetNSWLDetail.fld_NegaraID && x.fld_SyarikatID == GetNSWLDetail.fld_SyarikatID && x.fld_WilayahID == GetNSWLDetail.fld_WilayahID && x.fld_LadangID == GetNSWLDetail.fld_LadangID).FirstOrDefault();
            int? convertint = 0;
            string genbatchno = "";

            if (getbatchno == null)
            {
                tbl_BatchRunNo tbl_BatchRunNo = new tbl_BatchRunNo();
                tbl_BatchRunNo.fld_BatchRunNo = 1;
                tbl_BatchRunNo.fld_BatchFlag = BatchFlag;
                tbl_BatchRunNo.fld_NegaraID = GetNSWLDetail.fld_NegaraID;
                tbl_BatchRunNo.fld_SyarikatID = GetNSWLDetail.fld_SyarikatID;
                tbl_BatchRunNo.fld_WilayahID = GetNSWLDetail.fld_WilayahID;
                tbl_BatchRunNo.fld_LadangID = GetNSWLDetail.fld_LadangID;
                db.tbl_BatchRunNo.Add(tbl_BatchRunNo);
                db.SaveChanges();
                convertint = 1;
                genbatchno = GetNSWLDetail.fld_RequestCode.ToUpper() + "_" + BatchWord + "_" + GetNSWLDetail.fld_LdgCode.ToUpper() + "_" + convertint;
            }
            else
            {
                convertint = getbatchno.fld_BatchRunNo;
                genbatchno = GetNSWLDetail.fld_RequestCode.ToUpper() + "_" + BatchWord + "_" + GetNSWLDetail.fld_LdgCode.ToUpper() + "_" + convertint;
                convertint = convertint + 1;
                getbatchno.fld_BatchRunNo = convertint;
                db.Entry(getbatchno).State = EntityState.Modified;
                db.SaveChanges();
            }

            return genbatchno;
        }

        public long CalAge(DateTime Startdate)
        {
            long age = 0;
            DateTime EndDate = ChangeTimeZone.gettimezone();
            TimeSpan ts = new TimeSpan(EndDate.Ticks - Startdate.Ticks);
            age = (long)(ts.Days / 365);
            return age;
        }

        public long CalEndDate(DateTime Startdate)
        {
            long age = 0;
            DateTime EndDate = ChangeTimeZone.gettimezone();
            TimeSpan ts = new TimeSpan(Startdate.Ticks - EndDate.Ticks);
            age = (long)(ts.Days / 365);
            return age;
        }

        public string TimeSpanToDate(DateTime d1)
        {
            // compute & return the difference of two dates,
            // returning years, months & days
            // d1 should be the larger (newest) of the two dates
            // we want d1 to be the larger (newest) date
            // flip if we need to
            int years = 0;
            int months = 0;
            int days = 0;
            string Return = "";
            DateTime d2 = ChangeTimeZone.gettimezone();
            if (d1 < d2)
            {
                DateTime d3 = d2;
                d2 = d1;
                d1 = d3;
            }

            // compute difference in total months
            months = 12 * (d1.Year - d2.Year) + (d1.Month - d2.Month);

            // based upon the 'days',
            // adjust months & compute actual days difference
            if (d1.Day < d2.Day)
            {
                months--;
                days = DateTime.DaysInMonth(d2.Year, d2.Month) - d2.Day + d1.Day;
            }
            else
            {
                days = d1.Day - d2.Day;
            }
            // compute years & actual months
            years = months / 12;
            months -= years * 12;

            Return = years + "";  //modified by wani 4.2.2020  (1)

            return Return;
        }


        public string TimeSpanToMonth(DateTime d1)
        {
            // compute & return the difference of two dates,
            // returning years, months & days
            // d1 should be the larger (newest) of the two dates
            // we want d1 to be the larger (newest) date
            // flip if we need to
            int years = 0;
            int months = 0;
            int days = 0;
            string Return = "";
            DateTime d2 = ChangeTimeZone.gettimezone();
            if (d1 < d2)
            {
                DateTime d3 = d2;
                d2 = d1;
                d1 = d3;
            }

            // compute difference in total months
            months = 12 * (d1.Year - d2.Year) + (d1.Month - d2.Month);

            // based upon the 'days',
            // adjust months & compute actual days difference
            if (d1.Day < d2.Day)
            {
                months--;
                days = DateTime.DaysInMonth(d2.Year, d2.Month) - d2.Day + d1.Day;
            }
            else
            {
                days = d1.Day - d2.Day;
            }
            // compute years & actual months
            years = months / 12;
            months -= years * 12;

            Return =  months + "";   //modified by wani 4.2.2020  (2)

            return Return;
        }

        public string TimeSpanToDay(DateTime d1)
        {
            // compute & return the difference of two dates,
            // returning years, months & days
            // d1 should be the larger (newest) of the two dates
            // we want d1 to be the larger (newest) date
            // flip if we need to
            int years = 0;
            int months = 0;
            int days = 0;
            string Return = "";
            DateTime d2 = ChangeTimeZone.gettimezone();
            if (d1 < d2)
            {
                DateTime d3 = d2;
                d2 = d1;
                d1 = d3;
            }

            // compute difference in total months
            months = 12 * (d1.Year - d2.Year) + (d1.Month - d2.Month);

            // based upon the 'days',
            // adjust months & compute actual days difference
            if (d1.Day < d2.Day)
            {
                months--;
                days = DateTime.DaysInMonth(d2.Year, d2.Month) - d2.Day + d1.Day;
            }
            else
            {
                days = d1.Day - d2.Day;
            }
            // compute years & actual months
            years = months / 12;
            months -= years * 12;

            Return =  days + "";    //modified by wani 4.2.2020  (3)

            return Return;
        }

        public decimal CostingPrmtPsprt(int? NeragaID, int? SyarikatID, short? Purpose, string Nationality)
        {
            var GetCosting = db.tbl_RenewCost.Where(x => x.fld_NegaraID == NeragaID && x.fld_SyarikatID == SyarikatID && x.fld_PurposeIndicator == Purpose && x.fld_NationalityCode == Nationality).FirstOrDefault();

            return GetCosting.fld_CostPrice.Value;
        }
    }
}