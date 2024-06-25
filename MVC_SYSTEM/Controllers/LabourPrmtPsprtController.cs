using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using iTextSharp.text;
using MVC_SYSTEM.LabourModels;
using MVC_SYSTEM.Class;
using MVC_SYSTEM.MasterModels;
using MVC_SYSTEM.App_LocalResources;
using MVC_SYSTEM.Attributes;
using MVC_SYSTEM.EstateModels;
using System.IO;
using MVC_SYSTEM.CustomModels;
using Rotativa;
//using Rotativa;
using System.Web.Security;


//Added by Shazana on 10/8
using Rotativa;
using System.Web.Security;
using MVC_SYSTEM.log;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    public class LabourPrmtPsprtController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private GetIdentity getidentity = new GetIdentity();
        GetWilayah getwilyah = new GetWilayah();
        ConvertToPdf ConvertToPdf = new ConvertToPdf();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private GetConfig GetConfig = new GetConfig();
        private List<tbl_LbrDataInfo> LbrDataInfo = new List<tbl_LbrDataInfo>();
        private GeneralFunc GeneralFunc = new GeneralFunc();
        string Purpose = "LABOUR";
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;

        errorlog geterror = new errorlog(); //Added by nana on 19/8

        // GET: LabourPrmtPsprt
        //public async Task<ActionResult> Index(int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, int? MonthList, int? MonthList2, int? YearList, tbl_LbrAbsconded tbl_LbrAbsconded, string sortOrder)  //Shazana 17/9

        //Commented by SHazana on 28/10
        //public async Task<ActionResult> Index(int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, string StartDate,string EndDate, tbl_LbrAbsconded tbl_LbrAbsconded, string sortOrder) //Shazana 17/9

        //Addded by Shazana on 28/10
        public ActionResult Index(int? RadioGroup, int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, string StartDate, string EndDate, tbl_LbrAbsconded tbl_LbrAbsconded, string sortOrder)
        {

            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int? GetWilayahID = 0;
            int? roleid = GetIdentity.RoleID(GetUserID);
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1;
            int RangeYear = DT.Year + 1;

            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

            var YearList1 = new List<SelectListItem>();
            for (var i = Year; i <= RangeYear; i++)
            {
                if (i == DT.Year)
                {
                    YearList1.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = true });
                }
                else
                {
                    YearList1.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                }
            }

            var MonthList1 = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "monthlist" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID), "fldOptConfValue", "fldOptConfDesc");
            var MonthList_2 = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "monthlist" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID), "fldOptConfValue", "fldOptConfDesc");

            ViewBag.YearList = YearList1;
            ViewBag.MonthList = MonthList1;
            ViewBag.MonthList2 = MonthList_2;


            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();
                if (WilayahIDList == 0)
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                    fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                    fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

                }
                else if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                    fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == WilayahIDList && x.fld_Deleted == false).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                    fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                    fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                    fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

                }

            }
            else
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_ID == WilayahID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();

                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();
                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == WilayahID && x.fld_ID == LadangID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                //fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));


            }

            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;




            DateTime? StartDate1 = Convert.ToDateTime(StartDate);
            DateTime? EndDate1 = Convert.ToDateTime(EndDate);

            //    Commented by Shazana on 28/10
            //if (WilayahID == 0 && LadangID == 0)
            //{

            //    if (string.IsNullOrEmpty(FreeText))
            //    {
            //        if (WilayahIDList == 0 && LadangIDList == 0)
            //        {

            //            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToListAsync();


            //        }
            //        else if (WilayahIDList != 0 && LadangIDList == 0)
            //        {
            //            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && x.fld_Nationality != "MA").OrderBy(x => x.fld_WorkerNo).ToListAsync();
            //        }
            //        else
            //        {
            //            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToListAsync();
            //        }
            //    }
            //    else
            //    {
            //        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToListAsync();
            //    }


            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(FreeText))
            //    {
            //        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToListAsync();
            //    }
            //    else
            //    {
            //        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToListAsync();
            //    }

            //}

            //   Close Commented by Shazana on 28/10






            //Added by Shazana on 28/10
            if (WilayahID == 0 && LadangID == 0 && RadioGroup == 0)
            {

                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {

                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();


                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && x.fld_Nationality != "MA").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                    else
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                }
                else
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
                }


            }

            else if (WilayahID == 0 && LadangID == 0 && RadioGroup == 1)  //Filter by passport
            {

                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {

                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();


                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && x.fld_Nationality != "MA").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                    else
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                }
                else
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
                }


            }

            else if (WilayahID == 0 && LadangID == 0 && RadioGroup == 2)  //Filter by passport
            {

                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {

                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();


                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && x.fld_Nationality != "MA").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                    else
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                }
                else
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
                }


            }


            //else
            //{
            //    if (string.IsNullOrEmpty(FreeText))
            //    {
            //        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToListAsync();
            //    }
            //    else
            //    {
            //        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToListAsync();
            //    }

            //}





            //Added by Shazana on 28/10



            ViewBag.workerNo = string.IsNullOrEmpty(sortOrder) ? "workerNo" : "";
            ViewBag.workerID = sortOrder == "WorkerID" ? "workerID desc" : "workerID";
            ViewBag.workerName = sortOrder == "workerName" ? "workerName desc" : "workerName";
            ViewBag.region = sortOrder == "Region" ? "Region desc" : "Region";
            ViewBag.Estate = sortOrder == "Estate" ? "Estate desc" : "Estate";

            //* added by nana: 17.07.2020
            var CustMod_LabourPrmtPsprt = new List<CustMod_LabourPrmtPsprt>();
            foreach (var labour in LbrDataInfo)
            {
                //  var app = Masterdb.tblPkjmastApp.Where(x => x.fldID == id).FirstOrDefault();
                string Host1, Catalog1, UserID1, Pass1 = "";
                string purpose2 = "CHECKROLL";
                MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
                if (labour.fld_WilayahID == 1)
                {
                    Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, 1, labour.fld_SyarikatID, labour.fld_NegaraID, purpose2);
                    dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
                }
                else if (labour.fld_WilayahID == 2)
                {
                    Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, 2, labour.fld_SyarikatID, labour.fld_NegaraID, purpose2);
                    dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
                }

                var CC = dbEstate.tbl_Pkjmast.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == labour.fld_WilayahID && x.fld_LadangID == labour.fld_LadangID && x.fld_Nopkj == labour.fld_WorkerNo).Select(s => s.fld_KodSAPPekerja).FirstOrDefault();

                CustMod_LabourPrmtPsprt.Add(new CustMod_LabourPrmtPsprt
                //  db.tbl_LbrDataInfo.Add(new tbl_LbrDataInfo
                {
                    fld_ID = labour.fld_ID,
                    fld_WorkerIDNo = labour.fld_WorkerIDNo,
                    fld_PermitNo = labour.fld_PermitNo,
                    fld_WorkerNo = labour.fld_WorkerNo,
                    fld_WorkerName = labour.fld_WorkerName,
                    fld_WorkerAddress = labour.fld_WorkerAddress,
                    fld_Postcode = labour.fld_Postcode,
                    fld_State = labour.fld_State,
                    fld_Country = labour.fld_Country,
                    fld_PhoneNo = labour.fld_PhoneNo,
                    fld_SexType = labour.fld_SexType,
                    fld_Race = labour.fld_Race,
                    fld_Religion = labour.fld_Religion,
                    fld_BOD = labour.fld_BOD,
                    fld_Age = labour.fld_Age,
                    fld_Nationality = labour.fld_Nationality,
                    fld_MarriedStatus = labour.fld_MarriedStatus,
                    fld_FeldaRelated = labour.fld_FeldaRelated,
                    fld_ActiveStatus = labour.fld_ActiveStatus,
                    fld_InactiveReason = labour.fld_InactiveReason,
                    fld_InactiveDT = labour.fld_InactiveDT,
                    fld_OnLeaveStatus = labour.fld_OnLeaveStatus,
                    fld_OnLeaveLastDT = labour.fld_OnLeaveLastDT,
                    fld_Notes = labour.fld_Notes,
                    fld_WorkingStartDT = labour.fld_WorkingStartDT,
                    fld_ConfirmationDT = labour.fld_ConfirmationDT,
                    fld_PassportEndDT = labour.fld_PassportEndDT,
                    fld_PermitEndDT = labour.fld_PermitEndDT,
                    fld_ArrivedDT = labour.fld_ArrivedDT,
                    fld_WorkerType = labour.fld_WorkerType,
                    fld_WorkCtgry = labour.fld_WorkCtgry,
                    fld_TmptLhr = labour.fld_TmptLhr,
                    fld_Roc = labour.fld_Roc,
                    fld_NegaraID = labour.fld_NegaraID,
                    fld_SyarikatID = labour.fld_SyarikatID,
                    fld_WilayahID = labour.fld_WilayahID,
                    fld_LadangID = labour.fld_LadangID,
                    // fld_DivisionID = labour.fld_DivisionID,
                    // fld_LbrProcessID = labour.fld_LbrProcessID,
                    fld_TransferToChckrollStatus = labour.fld_TransferToChckrollStatus,
                    fld_TransferToChckrollWorkerTransferStatus = labour.fld_TransferToChckrollWorkerTransferStatus,
                    fld_WorkerTransferCode = labour.fld_WorkerTransferCode,
                    fld_SupplierCode = labour.fld_SupplierCode,
                    fld_BankCode = labour.fld_BankCode,
                    fld_BankAcc = labour.fld_BankAcc,
                    fld_PerkesoNo = labour.fld_PerkesoNo,
                    //fld_CreatedBy = labour.fld_CreatedBy,
                    fld_CreatedDT = labour.fld_CreatedDT,
                    fld_ModifiedBy = labour.fld_ModifiedBy,
                    fld_ModifiedDT = labour.fld_ModifiedDT,
                    fld_Absconded = labour.fld_Absconded,
                    fld_EndContract = labour.fld_EndContract,

                    fld_Onleave = labour.fld_Onleave,
                    fld_SickDeath = labour.fld_SickDeath,
                    fld_fomema = labour.fld_fomema,
                    costcenter = CC

                }); ;
            }//*



            return View(CustMod_LabourPrmtPsprt);
        }

        //    Commented by Shazana on 28/10
        ////Added by Shazana on 17/8 (new action)
        //public ActionResult _LabourPrmtPsprt(int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, string StartDate, string EndDate, tbl_LbrAbsconded tbl_LbrAbsconded, string sortOrder, string print)

        //    Added by Shazana on 28/10
        public ActionResult _LabourPrmtPsprt(int? RadioGroup, int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, string StartDate, string EndDate, tbl_LbrAbsconded tbl_LbrAbsconded, string sortOrder, string print)

        {

            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int? GetWilayahID = 0;
            int? roleid = GetIdentity.RoleID(GetUserID);
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1;
            int RangeYear = DT.Year + 1;
            ViewBag.Print = print;
            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

            var YearList1 = new List<SelectListItem>();
            for (var i = Year; i <= RangeYear; i++)
            {
                if (i == DT.Year)
                {
                    YearList1.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = true });
                }
                else
                {
                    YearList1.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                }
            }

            var MonthList1 = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "monthlist" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID), "fldOptConfValue", "fldOptConfDesc");
            var MonthList_2 = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "monthlist" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID), "fldOptConfValue", "fldOptConfDesc");

            ViewBag.YearList = YearList1;
            ViewBag.MonthList = MonthList1;
            ViewBag.MonthList2 = MonthList_2;

            if (WilayahIDList == null && LadangIDList == null)
            {
                ViewBag.Message = GlobalResEstate.msgChooseRegionEstateMonthYear;

            }
            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();
                if (WilayahIDList == 0)
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                    fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                    fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

                }
                else if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                    fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == WilayahIDList && x.fld_Deleted == false).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                    fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                    fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                    fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

                }

            }
            else
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_ID == WilayahID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();

                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();
                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == WilayahID && x.fld_ID == LadangID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                //fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            }

            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;

            DT = ChangeTimeZone.gettimezone();
            DateTime StartDate1 = DT.Date;
            string Start = StartDate;
            if (Start != null)
            {
                StartDate1 = DateTime.ParseExact(Start, @"dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }

            DateTime EndDate1 = DT.Date;
            string End = EndDate;
            if (End != null)
            {
                EndDate1 = DateTime.ParseExact(End, @"dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }




            //Commented by Shazana on 28/10
            //if (WilayahID == 0 && LadangID == 0)
            //{

            //    if (string.IsNullOrEmpty(FreeText))
            //    {
            //        if (WilayahIDList == 0 && LadangIDList == 0)
            //        {

            //            LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();


            //        }
            //        else if (WilayahIDList != 0 && LadangIDList == 0)
            //        {
            //            LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && ( x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no") && x.fld_Nationality != "MA").OrderBy(x => x.fld_WorkerNo).ToList();
            //        }
            //        else
            //        {
            //            LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();
            //        }
            //    }
            //    else
            //    {
            //        LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
            //    }


            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(FreeText))
            //    {
            //        LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();
            //    }
            //    else
            //    {
            //        LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
            //    }

            //}

            ////Close commented by Shazana on 28/10



            //Added by Shazana on 28/10
            if (WilayahID == 0 && LadangID == 0 && RadioGroup == 0)
            {

                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {

                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();


                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && x.fld_Nationality != "MA").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                    else //fashqadmin
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                }
                else
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
                }


            }

            else if (WilayahID == 0 && LadangID == 0 && RadioGroup == 1)  //Filter by passport
            {

                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {

                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();


                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && x.fld_Nationality != "MA").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                    else
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                }
                else
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
                }


            }

            else if (WilayahID == 0 && LadangID == 0 && RadioGroup == 2)  //Filter by passport
            {

                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {

                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();


                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && x.fld_Nationality != "MA").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                    else
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                }
                else
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
                }


            }


            //Added by Shazana 21/6/2024

            //Added by Shazana on 28/10
            else if (WilayahID != 0 && LadangID != 0 && RadioGroup == 0 && roleid == 7) //permit
            {

                if (string.IsNullOrEmpty(FreeText) && LadangIDList !=0 && WilayahIDList != 0)
                {
                    if (WilayahIDList != 0 && LadangIDList != 0)
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID== LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && x.fld_Nationality != "MA").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                  
                }
                else if (FreeText != "" && LadangIDList != 0 && WilayahIDList != 0)
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText)) && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).OrderBy(x => x.fld_WorkerNo).ToList();
                }


            }

            else if (WilayahID != 0 && LadangID != 0 && RadioGroup == 1 && roleid == 7)  //Filter by passport
            {

                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList != 0 && LadangIDList != 0)
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();
                    }
                  
                }
                else if (FreeText != "" && WilayahID != 0  && LadangID != 0)
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
                }


            }

            else if (WilayahID != 0 && LadangID != 0 && RadioGroup == 2 && roleid == 7)  //Filter by passport and permit
            {

                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList != 0 && LadangIDList != 0)
                    {

                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToList();


                    }
                }
                else if(FreeText != "" && WilayahIDList != 0 && LadangIDList != 0)
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && (x.fld_PassportEndDT >= StartDate1 && x.fld_PassportEndDT <= EndDate1) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToList();
                }

            }
            //else
            //{
            //    if (string.IsNullOrEmpty(FreeText))
            //    {
            //        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT >= StartDate1 && x.fld_PermitEndDT <= EndDate1) && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no").OrderBy(x => x.fld_WorkerNo).ToListAsync();
            //    }
            //    else
            //    {
            //        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == "no" && x.fld_EndContract == "no" && x.fld_SickDeath == "no" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(x => x.fld_WorkerNo).ToListAsync();
            //    }

            //}

            //Added by Shazana on 28/10

            ViewBag.workerNo = string.IsNullOrEmpty(sortOrder) ? "workerNo" : "";
            ViewBag.workerID = sortOrder == "WorkerID" ? "workerID desc" : "workerID";
            ViewBag.workerName = sortOrder == "workerName" ? "workerName desc" : "workerName";
            ViewBag.region = sortOrder == "Region" ? "Region desc" : "Region";
            ViewBag.Estate = sortOrder == "Estate" ? "Estate desc" : "Estate";

            //* added by nana: 17.07.2020
            var CustMod_LabourPrmtPsprt = new List<CustMod_LabourPrmtPsprt>();
            foreach (var labour in LbrDataInfo)
            {
                //  var app = Masterdb.tblPkjmastApp.Where(x => x.fldID == id).FirstOrDefault();
                string Host1, Catalog1, UserID1, Pass1 = "";
                string purpose2 = "CHECKROLL";
                MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
                if (labour.fld_WilayahID == 1)
                {
                    Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, 1, labour.fld_SyarikatID, labour.fld_NegaraID, purpose2);
                    dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
                }
                else if (labour.fld_WilayahID == 2)
                {
                    Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, 2, labour.fld_SyarikatID, labour.fld_NegaraID, purpose2);
                    dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
                }

                var CC = dbEstate.tbl_Pkjmast.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == labour.fld_WilayahID && x.fld_LadangID == labour.fld_LadangID && x.fld_Nopkj == labour.fld_WorkerNo).Select(s => s.fld_KodSAPPekerja).FirstOrDefault();

                CustMod_LabourPrmtPsprt.Add(new CustMod_LabourPrmtPsprt
                //  db.tbl_LbrDataInfo.Add(new tbl_LbrDataInfo
                {
                    fld_ID = labour.fld_ID,
                    fld_WorkerIDNo = labour.fld_WorkerIDNo,
                    fld_PermitNo = labour.fld_PermitNo,
                    fld_WorkerNo = labour.fld_WorkerNo,
                    fld_WorkerName = labour.fld_WorkerName,
                    fld_WorkerAddress = labour.fld_WorkerAddress,
                    fld_Postcode = labour.fld_Postcode,
                    fld_State = labour.fld_State,
                    fld_Country = labour.fld_Country,
                    fld_PhoneNo = labour.fld_PhoneNo,
                    fld_SexType = labour.fld_SexType,
                    fld_Race = labour.fld_Race,
                    fld_Religion = labour.fld_Religion,
                    fld_BOD = labour.fld_BOD,
                    fld_Age = labour.fld_Age,
                    fld_Nationality = labour.fld_Nationality,
                    fld_MarriedStatus = labour.fld_MarriedStatus,
                    fld_FeldaRelated = labour.fld_FeldaRelated,
                    fld_ActiveStatus = labour.fld_ActiveStatus,
                    fld_InactiveReason = labour.fld_InactiveReason,
                    fld_InactiveDT = labour.fld_InactiveDT,
                    fld_OnLeaveStatus = labour.fld_OnLeaveStatus,
                    fld_OnLeaveLastDT = labour.fld_OnLeaveLastDT,
                    fld_Notes = labour.fld_Notes,
                    fld_WorkingStartDT = labour.fld_WorkingStartDT,
                    fld_ConfirmationDT = labour.fld_ConfirmationDT,
                    fld_PassportEndDT = labour.fld_PassportEndDT,
                    fld_PermitEndDT = labour.fld_PermitEndDT,
                    fld_ArrivedDT = labour.fld_ArrivedDT,
                    fld_WorkerType = labour.fld_WorkerType,
                    fld_WorkCtgry = labour.fld_WorkCtgry,
                    fld_TmptLhr = labour.fld_TmptLhr,
                    fld_Roc = labour.fld_Roc,
                    fld_NegaraID = labour.fld_NegaraID,
                    fld_SyarikatID = labour.fld_SyarikatID,
                    fld_WilayahID = labour.fld_WilayahID,
                    fld_LadangID = labour.fld_LadangID,
                    // fld_DivisionID = labour.fld_DivisionID,
                    // fld_LbrProcessID = labour.fld_LbrProcessID,
                    fld_TransferToChckrollStatus = labour.fld_TransferToChckrollStatus,
                    fld_TransferToChckrollWorkerTransferStatus = labour.fld_TransferToChckrollWorkerTransferStatus,
                    fld_WorkerTransferCode = labour.fld_WorkerTransferCode,
                    fld_SupplierCode = labour.fld_SupplierCode,
                    fld_BankCode = labour.fld_BankCode,
                    fld_BankAcc = labour.fld_BankAcc,
                    fld_PerkesoNo = labour.fld_PerkesoNo,
                    //fld_CreatedBy = labour.fld_CreatedBy,
                    fld_CreatedDT = labour.fld_CreatedDT,
                    fld_ModifiedBy = labour.fld_ModifiedBy,
                    fld_ModifiedDT = labour.fld_ModifiedDT,
                    fld_Absconded = labour.fld_Absconded,
                    fld_EndContract = labour.fld_EndContract,

                    fld_Onleave = labour.fld_Onleave,
                    fld_SickDeath = labour.fld_SickDeath,
                    fld_fomema = labour.fld_fomema,
                    costcenter = CC

                }); ;
            }//*

            return View(CustMod_LabourPrmtPsprt);
        }


        //Commented by Shazana on 28/10
        //public ActionResult PrintWorkerPdf(string WilayahIDList, string LadangIDList, string StatusList, string StartDate, string EndDate, string FreeText,  int id, string genid)
        //Added by Shazana on 28/10
        public ActionResult PrintWorkerPdf(int? RadioGroup, string WilayahIDList, string LadangIDList, string StatusList, string StartDate, string EndDate, string FreeText, int id, string genid)
        {

            int? getuserid = 0;
            string getusername = "";
            string getcookiesval = "";
            bool checkidentity = false;
            //geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
            var getuser = db1.tblUsers.Where(u => u.fldUserID == id && u.fldDeleted == false).SingleOrDefault();
            if (getuser != null)
            {
                getuserid = GetIdentity.ID(getuser.fldUserName);
                getusername = getuser.fldUserName;
            }
            ViewBag.Message =
            checkidentity = CheckGenIdentity(id, genid, getuserid, getusername, out getcookiesval);

            ActionAsPdf report = new ActionAsPdf("");

            if (checkidentity)
            {
                getBackAuth(getcookiesval);
                var cookies = Request.Cookies.AllKeys.ToDictionary(k => k, k => Request.Cookies[k].Value);
                //geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
                string print = "Yes";


                //DT = ChangeTimeZone.gettimezone();
                //DateTime StartDate1 = DT.Date;
                //string Start = StartDate;
                //if (Start != null)
                //{
                //    StartDate1 = DateTime.ParseExact(Start, @"dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                //}

                //DateTime EndDate1 = DT.Date;
                //string End = EndDate;
                //if (End != null)
                //{
                //    EndDate1 = DateTime.ParseExact(End, @"dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                //}

                //Commented by Shazana on 28/10
                //report = new ActionAsPdf("_LabourPrmtPsprt", new { WilayahIDList, LadangIDList, StartDate, EndDate,  FreeText, print })
                //Close Commented by Shazana on 28/10
                //    Added by Shazana on 28/10
                report = new ActionAsPdf("_LabourPrmtPsprt", new { RadioGroup, WilayahIDList, LadangIDList, StartDate, EndDate, FreeText, print })
                {
                    FormsAuthenticationCookieName = FormsAuthentication.FormsCookieName,
                    Cookies = cookies
                };
            }
            else
            {
                report = new ActionAsPdf("PDFInvalid");
            }

            //Added by Shazana on 28/10
            Response.AppendHeader("Content-Disposition", "inline; filename=" + "LabourPermitPassport" + ".pdf");
            //Close Added by Shazana on 28/10
            return report;
        }

        private MVC_SYSTEM_MasterModels db1 = new MVC_SYSTEM_MasterModels();
        int? getuserid = 0;
        string getusername = "";
        string getcookiesval = "";
        bool checkidentity = false;

        public void getBackAuth(string getcookiesval)
        {
            string CookiesValue = "";
            try
            {
                CookiesValue = Request.Cookies[FormsAuthentication.FormsCookieName].Value;
                if (CookiesValue != getcookiesval)
                {
                    HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    cookie.Value = getcookiesval;
                    Response.Cookies.Add(cookie);
                   // geterror.testlog("Try if : " + User.Identity.Name, "Try if : " + CookiesValue, "Try if : " + getcookiesval);
                }
               // geterror.testlog("Try no if : " + User.Identity.Name, "Try no if : " + CookiesValue, "Try no if : " + getcookiesval);
            }
            catch
            {
                HttpCookie authoCookies = new HttpCookie(FormsAuthentication.FormsCookieName, getcookiesval);
                Response.SetCookie(authoCookies);
              //  geterror.testlog("Catch : " + User.Identity.Name, "Catch : " + CookiesValue, "Catch : " + getcookiesval);
            }
           // geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
        }


        public bool CheckGenIdentity(int id, string genid, int? userid, string username, out string CookiesValue)
        {
            bool result = false;
            int? NegaraID, SyarikatID, WilayahID, LadangID = 0;
            string host, catalog, user, pass = "";
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, userid, username);

            Connection.GetConnection(out host, out catalog, out user, out pass, WilayahID.Value, SyarikatID.Value,
                NegaraID.Value, "LABOUR");

            CookiesValue = "";

            MVC_SYSTEM_Models dbr = MVC_SYSTEM_Models.ConnectToSqlServer(host, catalog, user, pass);

            try
            {
                Guid genidC = Guid.Parse(genid);
                var CheckIdentity = dbr.tbl_PdfGen.Where(x => x.fld_ID == genidC && x.fld_UserID == id).FirstOrDefault();

                if (CheckIdentity == null)
                {
                    result = false;
                }
                else
                {
                    result = true;
                    CookiesValue = CheckIdentity.fld_CookiesVal;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }


        //Close added


        [HttpPost]
        public ActionResult ConvertPDF(string myHtml, string filename, string reportname)
        {
            string linkfile = "";
            bool success = false;
            string msg = "";
            string status = "";
            string appname = Request.ApplicationPath;
            string domain = Request.Url.GetLeftPart(UriPartial.Authority);

            if (appname != "/")
            {
                domain = domain + appname;
            }

            linkfile = ConvertToPdf.DownloadAsPDF(myHtml, filename, User.Identity.Name, reportname, domain);

            if (linkfile != "")
            {
                success = true;
                status = "success";
            }
            else
            {
                success = false;
                msg = "Something wrong.";
                status = "danger";
            }

            return Json(new { success = success, id = linkfile, msg = msg, status = status });
        }
        [HttpPost]
        public ActionResult ConvertPDF2(string myHtml, string filename, string reportname)
        {
            bool success = false;
            string msg = "";
            string status = "";
            tblHtmlReport tblHtmlReport = new tblHtmlReport();

            tblHtmlReport.fldHtlmCode = myHtml;
            tblHtmlReport.fldFileName = filename;
            tblHtmlReport.fldReportName = reportname;

            Masterdb.tblHtmlReport.Add(tblHtmlReport);
            Masterdb.SaveChanges();

            success = true;
            status = "success";

            return Json(new { success = success, id = tblHtmlReport.fldID, msg = msg, status = status, link = Url.Action("GetPDF", "LabourPrmtPsprt", null, "http") + "/" + tblHtmlReport.fldID });
        }
        public ActionResult GetPDF(int id)
        {
            int? NegaraID = 0;
            int? SyarikatID = 0;
            int? WilayahID = 0;
            int? LadangID = 0;
            int? getuserid = getidentity.ID(User.Identity.Name);
            string width = "", height = "";
            string imagepath = Server.MapPath("~/Asset/Images/");

            var gethtml = Masterdb.tblHtmlReport.Find(id);
            var getsize = Masterdb.tblReportLists.Where(x => x.fldReportListAction == gethtml.fldReportName.ToString()).FirstOrDefault();
            if (getsize != null)
            {
                width = getsize.fldWidthReport.ToString();
                height = getsize.fldHeightReport.ToString();
            }
            else
            {
                var getsizesubreport = Masterdb.tblSubReportLists.Where(x => x.fldSubReportListAction == gethtml.fldReportName.ToString()).FirstOrDefault();
                width = getsizesubreport.fldSubWidthReport.ToString();
                height = getsizesubreport.fldSubHeightReport.ToString();
            }
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, getuserid, User.Identity.Name);
            var logosyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID).Select(s => s.fld_LogoName).FirstOrDefault();

            //Export HTML String as PDF.
            //Image logo = Image.GetInstance(imagepath + logosyarikat);
            //Image alignment
            //logo.ScaleToFit(50f, 50f);
            //logo.Alignment = Image.TEXTWRAP | Image.ALIGN_CENTER;
            //StringReader sr = new StringReader(gethtml.fldHtlmCode);
            Document pdfDoc = new Document(new Rectangle(int.Parse(width), int.Parse(height)), 50f, 50f, 50f, 50f);
            //HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();
            //pdfDoc.Add(logo);
            using (TextReader sr = new StringReader(gethtml.fldHtlmCode))
            {
                using (var htmlWorker = new HTMLWorkerExtended(pdfDoc, imagepath + logosyarikat))
                {
                    htmlWorker.Open();
                    htmlWorker.Parse(sr);
                }
            }
            pdfDoc.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + gethtml.fldFileName + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

            db.Entry(gethtml).State = EntityState.Deleted;
            db.SaveChanges();
            return View();
        }
        public JsonResult GetLadang(int WilayahID)
        {
            List<SelectListItem> ladanglist = new List<SelectListItem>();

            int? NegaraID = 0;
            int? SyarikatID = 0;
            int? WilayahID2 = 0;
            int? LadangID = 0;
            int? getuserid = getidentity.ID(User.Identity.Name);

            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID2, out LadangID, getuserid, User.Identity.Name);

            if (getwilyah.GetAvailableWilayah(SyarikatID))
            {
                if (WilayahID == 0)
                {
                    ladanglist = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_Deleted == false).OrderBy(o => o.fld_LdgName).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                }
                else
                {
                    ladanglist = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_WlyhID == WilayahID && x.fld_Deleted == false).OrderBy(o => o.fld_LdgName).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                }
            }

            return Json(ladanglist);
        }

        //Commented by Shazana on 28/10
        // public ActionResult PrintPrmtPsprtPdf(int? WilayahIDList, int? LadangIDList, string FreeText, int? MonthList, int? MonthList2, int? YearList, int id, string genid)
        //Added by Shazana on 28/10
        public ActionResult PrintPrmtPsprtPdf(int? RadioGroup, int? WilayahIDList, int? LadangIDList, string FreeText, int? MonthList, int? MonthList2, int? YearList, int id, string genid)
        {
            int? getuserid = 0;
            string getusername = "";
            string getcookiesval = "";
            bool checkidentity = false;
            MVC_SYSTEM_MasterModels db = new MVC_SYSTEM_MasterModels();

            //geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
            var getuser = db.tblUsers.Where(u => u.fldUserID == id && u.fldDeleted == false).SingleOrDefault();
            {
                getuserid = GetIdentity.ID(getuser.fldUserName);
                getusername = getuser.fldUserName;
            }

            checkidentity = CheckGenIdentity(id, genid, getuserid, getusername, out getcookiesval);

            ActionAsPdf report = new ActionAsPdf("");

            if (checkidentity)
            {
                getBackAuth(getcookiesval);
                var cookies = Request.Cookies.AllKeys.ToDictionary(k => k, k => Request.Cookies[k].Value);
                //geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
                string print = "Yes";

                //Commented by Shazana on 28/10
                //report = new ActionAsPdf("Index", new {WilayahIDList, LadangIDList, MonthList, MonthList2, YearList, FreeText, print })
                //Added by Shazana on 28/10
                report = new ActionAsPdf("Index", new { RadioGroup, WilayahIDList, LadangIDList, MonthList, MonthList2, YearList, FreeText, print })
                {
                    FormsAuthenticationCookieName = FormsAuthentication.FormsCookieName,
                    Cookies = cookies
                };
            }
            else
            {
                report = new ActionAsPdf("PDFInvalid");
            }

            return report;
        }

        public string PDFInvalid()
        {
            return GlobalResEstate.msgInvalidPDFConvert;
        }


        public async Task<ActionResult> PermitUpdate(Guid? id)
        {
            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            List<SelectListItem> PermitRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatus = new List<SelectListItem>();
            PermitRenewalStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "permitrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text").ToList();
            PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            PassportPermitStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text").ToList();
            PassportPermitStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            ViewBag.fld_PermitRenewalStatus = PermitRenewalStatus;
            ViewBag.fld_PermitStatus = PassportPermitStatus;

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PermitUpdate(CustMod_PermitPassport tbl_LbrPrmtPsprtUpdate)
        {
            //fld_PurposeIndicator = 1 is for permit
            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            var GetExistingPermit = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrPrmtPsprtUpdate.fld_LbrRefID && (x.fld_NewPrmtPsprtNo == tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo || x.fld_OldPrmtPsprtNo == tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo) && x.fld_PurposeIndicator == 1 && x.fld_Deleted == false).Count();
            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrPrmtPsprtUpdate.fld_LbrRefID);
            if (GetExistingPermit == 0)
            {
                if (ModelState.IsValid)
                {
                    tbl_LbrPrmtPsprtUpdate LbrPrmtPsprtUpdate = new tbl_LbrPrmtPsprtUpdate();
                    LbrPrmtPsprtUpdate.fld_LbrRefID = tbl_LbrPrmtPsprtUpdate.fld_LbrRefID;
                    LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo;
                    LbrPrmtPsprtUpdate.fld_NewPrmtPsrtEndDT = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsrtEndDT;
                    LbrPrmtPsprtUpdate.fld_OldPrmtPsprtNo = tbl_LbrPrmtPsprtUpdate.fld_OldPrmtPsprtNo;
                    LbrPrmtPsprtUpdate.fld_OldPrmtPsrtEndDT = tbl_LbrPrmtPsprtUpdate.fld_OldPrmtPsrtEndDT;
                    LbrPrmtPsprtUpdate.fld_CreatedBy = GetUserID;
                    LbrPrmtPsprtUpdate.fld_CreatedDT = DT;
                    LbrPrmtPsprtUpdate.fld_PurposeIndicator = 1;
                    LbrPrmtPsprtUpdate.fld_LadangID = GetLabourDetails.fld_LadangID;
                    LbrPrmtPsprtUpdate.fld_NegaraID = GetLabourDetails.fld_NegaraID;
                    LbrPrmtPsprtUpdate.fld_SyarikatID = GetLabourDetails.fld_SyarikatID;
                    LbrPrmtPsprtUpdate.fld_WilayahID = GetLabourDetails.fld_WilayahID;
                    LbrPrmtPsprtUpdate.fld_Deleted = false;
                    //Added by Shazana 8/4/2024
                    LbrPrmtPsprtUpdate.fld_PermitRenewalStatus = tbl_LbrPrmtPsprtUpdate.fld_PermitRenewalStatus;
                    LbrPrmtPsprtUpdate.fld_PermitStatus = tbl_LbrPrmtPsprtUpdate.fld_PermitStatus;
                    LbrPrmtPsprtUpdate.fld_PermitRenewalStartDate = tbl_LbrPrmtPsprtUpdate.fld_PermitRenewalStartDate;
                    db.tbl_LbrPrmtPsprtUpdate.Add(LbrPrmtPsprtUpdate);
                    db.SaveChanges();

                    GetLabourDetails.fld_PermitNo = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo;
                    GetLabourDetails.fld_PermitEndDT = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsrtEndDT;
                    GetLabourDetails.fld_ModifiedBy = GetUserID;
                    GetLabourDetails.fld_ModifiedDT = DT;
                    GetLabourDetails.fld_PermitRenewalStatus = tbl_LbrPrmtPsprtUpdate.fld_PermitRenewalStatus;
                    GetLabourDetails.fld_PermitStatus = tbl_LbrPrmtPsprtUpdate.fld_PermitStatus;
                    GetLabourDetails.fld_PermitRenewalStartDate = tbl_LbrPrmtPsprtUpdate.fld_PermitRenewalStartDate;
                    db.Entry(GetLabourDetails).State = EntityState.Modified;
                    db.SaveChanges();


                    var GetPreviousPermit = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrPrmtPsprtUpdate.fld_LbrRefID && x.fld_NewPrmtPsprtNo != tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo && x.fld_PurposeIndicator ==1  && x.fld_Deleted == false).OrderByDescending(x => x.fld_CreatedDT).FirstOrDefault();
                    if (GetPreviousPermit != null)
                    {
                        GetPreviousPermit.fld_Deleted = true;
                        GetPreviousPermit.fld_DeletedBy = GetUserID;
                        GetPreviousPermit.fld_DeletedDT = DT;
                        db.Entry(GetPreviousPermit).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    SyncToCheckRollFunc(GetLabourDetails, 1);
                    ModelState.AddModelError("", "Update Successfully");
                    ViewBag.MsgColor = "color: green";
                }
                else
                {
                    ModelState.AddModelError("", "Please Fulfill * Remark");
                    ViewBag.MsgColor = "color: red";
                }
            }
            else
            {
                ModelState.AddModelError("", "Permit Already Renewed");
                ViewBag.MsgColor = "color: orange";
            }
            List<SelectListItem> PermitRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatus = new List<SelectListItem>();
            PermitRenewalStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "permitrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrPrmtPsprtUpdate.fld_PermitRenewalStatus).ToList();
            PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            PassportPermitStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrPrmtPsprtUpdate.fld_PermitStatus).ToList();
            PassportPermitStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            ViewBag.fld_PermitRenewalStatus = PermitRenewalStatus;
            ViewBag.fld_PermitStatus = PassportPermitStatus;

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(tbl_LbrPrmtPsprtUpdate.fld_LbrRefID);
            return View(LbrDataInfo);
        }
        //Added by Shazana 8/4/2024
        public ActionResult PermitEdit(Guid? id, int? fld_id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            var LbrDataInfo = db.tbl_LbrDataInfo.Find(id);
            List<SelectListItem> PermitRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PermitPermitStatus = new List<SelectListItem>();
            PermitRenewalStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "permitrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", LbrDataInfo.fld_PermitRenewalStatus).ToList();
            PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            ViewBag.fld_PermitRenewalStatus = PermitRenewalStatus;
            ViewBag.fld_id = fld_id;
            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PermitEdit(CustMod_PermitPassport tbl_LbrPrmtPsprtUpdate)
        {
            //fld_PurposeIndicator = 2 is for passport
            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            var GetExistingPermit = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrPrmtPsprtUpdate.fld_LbrRefID && (x.fld_ID == tbl_LbrPrmtPsprtUpdate.fld_ID) && x.fld_PurposeIndicator == 1 && x.fld_Deleted == false).FirstOrDefault();
            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrPrmtPsprtUpdate.fld_LbrRefID);

            if (GetLabourDetails != null)
            {
                GetExistingPermit.fld_LbrRefID = tbl_LbrPrmtPsprtUpdate.fld_LbrRefID;
                GetExistingPermit.fld_PermitRenewalStatus = tbl_LbrPrmtPsprtUpdate.fld_PermitRenewalStatus;
                GetExistingPermit.fld_NewPrmtPsprtNo = GetExistingPermit.fld_NewPrmtPsprtNo;
                GetExistingPermit.fld_NewPrmtPsrtEndDT = GetExistingPermit.fld_NewPrmtPsrtEndDT;
                GetExistingPermit.fld_ModifiedBy = GetUserID;
                GetExistingPermit.fld_ModifiedDT = DT;
                db.Entry(GetExistingPermit).State = EntityState.Modified;
                db.SaveChanges();

                GetLabourDetails.fld_ModifiedBy = GetUserID;
                GetLabourDetails.fld_ModifiedDT = DT;
                GetLabourDetails.fld_PermitRenewalStatus = tbl_LbrPrmtPsprtUpdate.fld_PermitRenewalStatus;
                db.Entry(GetLabourDetails).State = EntityState.Modified;
                db.SaveChanges();

                SyncToCheckRollFunc(GetLabourDetails, 2);
                ViewBag.MsgColor = "color: green";
            }
            List<SelectListItem> PermitRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatus = new List<SelectListItem>();
            PermitRenewalStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "permitrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrPrmtPsprtUpdate.fld_PermitRenewalStatus).ToList();
            PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            PassportPermitStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrPrmtPsprtUpdate.fld_PermitStatus).ToList();
            PassportPermitStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            ViewBag.fld_PermitRenewalStatus = PermitRenewalStatus;
            ViewBag.fld_PermitStatus = PassportPermitStatus;
            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(tbl_LbrPrmtPsprtUpdate.fld_LbrRefID);
            return RedirectToAction("PermitUpdate", "LabourPrmtPsprt", new { id = LbrDataInfo.fld_ID });

        }
        public ActionResult _LabourPermitDetail(Guid LabourID)
        {
            return View(db.vw_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == LabourID && x.fld_PurposeIndicator == 1).ToList());
        }

        public async Task<ActionResult> PassportUpdate(Guid? id)
        {
            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            //Added by Shazana 29/3/2024
            List<SelectListItem> PassportRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatus = new List<SelectListItem>();
            PassportRenewalStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text").ToList();
            PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            PassportPermitStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text").ToList();
            PassportPermitStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            ViewBag.fld_PassportRenewalStatus = PassportRenewalStatus;
            ViewBag.fld_PassportStatus = PassportPermitStatus;

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PassportUpdate(CustMod_PermitPassport tbl_LbrPrmtPsprtUpdate)
        {
            //fld_PurposeIndicator = 2 is for passport
            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            var GetExistingPassport = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrPrmtPsprtUpdate.fld_LbrRefID && (x.fld_NewPrmtPsprtNo == tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo || x.fld_OldPrmtPsprtNo == tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo) && x.fld_PurposeIndicator == 2 && x.fld_Deleted == false).Count();
            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrPrmtPsprtUpdate.fld_LbrRefID);

            if (GetExistingPassport == 0)
            {
                if (ModelState.IsValid)
                {
                    tbl_LbrPrmtPsprtUpdate LbrPrmtPsprtUpdate = new tbl_LbrPrmtPsprtUpdate();
                    LbrPrmtPsprtUpdate.fld_LbrRefID = tbl_LbrPrmtPsprtUpdate.fld_LbrRefID;
                    LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo;
                    LbrPrmtPsprtUpdate.fld_NewPrmtPsrtEndDT = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsrtEndDT;
                    LbrPrmtPsprtUpdate.fld_OldPrmtPsprtNo = tbl_LbrPrmtPsprtUpdate.fld_OldPrmtPsprtNo;
                    LbrPrmtPsprtUpdate.fld_OldPrmtPsrtEndDT = tbl_LbrPrmtPsprtUpdate.fld_OldPrmtPsrtEndDT;
                    LbrPrmtPsprtUpdate.fld_CreatedBy = GetUserID;
                    LbrPrmtPsprtUpdate.fld_CreatedDT = DT;
                    LbrPrmtPsprtUpdate.fld_PurposeIndicator = 2;
                    LbrPrmtPsprtUpdate.fld_LadangID = GetLabourDetails.fld_LadangID;
                    LbrPrmtPsprtUpdate.fld_NegaraID = GetLabourDetails.fld_NegaraID;
                    LbrPrmtPsprtUpdate.fld_SyarikatID = GetLabourDetails.fld_SyarikatID;
                    LbrPrmtPsprtUpdate.fld_WilayahID = GetLabourDetails.fld_WilayahID;
                    LbrPrmtPsprtUpdate.fld_Deleted = false;
                    //Added by Shazana 8/4/2024
                    LbrPrmtPsprtUpdate.fld_PassportRenewalStatus = tbl_LbrPrmtPsprtUpdate.fld_PassportRenewalStatus;
                    LbrPrmtPsprtUpdate.fld_PassportStatus = tbl_LbrPrmtPsprtUpdate.fld_PassportStatus;
                    LbrPrmtPsprtUpdate.fld_PassportRenewalStartDate = tbl_LbrPrmtPsprtUpdate.fld_PassportRenewalStartDate;
                    db.tbl_LbrPrmtPsprtUpdate.Add(LbrPrmtPsprtUpdate);
                    db.SaveChanges();

                    GetLabourDetails.fld_WorkerIDNo = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo;
                    GetLabourDetails.fld_PassportEndDT = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsrtEndDT;
                    GetLabourDetails.fld_ModifiedBy = GetUserID;
                    GetLabourDetails.fld_ModifiedDT = DT;
                    //Added by Shazana 8/4/2024
                    GetLabourDetails.fld_PassportRenewalStatus = tbl_LbrPrmtPsprtUpdate.fld_PassportRenewalStatus;
                    GetLabourDetails.fld_PassportStatus = tbl_LbrPrmtPsprtUpdate.fld_PassportStatus;
                    GetLabourDetails.fld_PassportRenewalStartDate = tbl_LbrPrmtPsprtUpdate.fld_PassportRenewalStartDate;
                    db.Entry(GetLabourDetails).State = EntityState.Modified;
                    db.SaveChanges();

                    var GetPreviousPermit = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrPrmtPsprtUpdate.fld_LbrRefID && x.fld_NewPrmtPsprtNo != tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo && x.fld_PurposeIndicator == 2 && x.fld_Deleted == false).OrderByDescending(x => x.fld_CreatedDT).FirstOrDefault();
                    if (GetPreviousPermit != null)
                    {
                        GetPreviousPermit.fld_Deleted = true;
                        GetPreviousPermit.fld_DeletedBy = GetUserID;
                        GetPreviousPermit.fld_DeletedDT = DT;
                        db.Entry(GetPreviousPermit).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    SyncToCheckRollFunc(GetLabourDetails, 2);

                    ModelState.AddModelError("", "Update Successfully");
                    ViewBag.MsgColor = "color: green";
                }
                else
                {
                    ModelState.AddModelError("", "Please Fulfill * Remark");
                    ViewBag.MsgColor = "color: red";
                }
            }
            else
            {
                ModelState.AddModelError("", "Passport Already Renewed");
                ViewBag.MsgColor = "color: orange";
            }
            List<SelectListItem> PassportRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatus = new List<SelectListItem>();
            PassportRenewalStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrPrmtPsprtUpdate.fld_PassportRenewalStatus).ToList();
            PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            PassportPermitStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrPrmtPsprtUpdate.fld_PassportStatus).ToList();
            PassportPermitStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            ViewBag.fld_PassportRenewalStatus = PassportRenewalStatus;
            ViewBag.fld_PassportStatus = PassportPermitStatus;
            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(tbl_LbrPrmtPsprtUpdate.fld_LbrRefID);
            return View(LbrDataInfo);
        }

        public ActionResult _LabourPassportDetail(Guid LabourID)
        {
            return View(db.vw_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == LabourID && x.fld_PurposeIndicator == 2).OrderBy(x=>x.fld_CreatedDT).ToList());
        }

        //Added by Shazana 8/4/2024
        public ActionResult PassportEdit(Guid? id, int? fld_id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            var LbrDataInfo = db.tbl_LbrDataInfo.Find(id);
            List<SelectListItem> PassportRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatus = new List<SelectListItem>();
            PassportRenewalStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", LbrDataInfo.fld_PassportRenewalStatus).ToList();
            PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            ViewBag.fld_PassportRenewalStatus = PassportRenewalStatus;
            ViewBag.fld_id = fld_id;
            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PassportEdit(CustMod_PermitPassport tbl_LbrPrmtPsprtUpdate)
        {
            //fld_PurposeIndicator = 2 is for passport
            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            var GetExistingPassport = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrPrmtPsprtUpdate.fld_LbrRefID && (x.fld_ID == tbl_LbrPrmtPsprtUpdate.fld_ID) && x.fld_PurposeIndicator == 2 && x.fld_Deleted == false).FirstOrDefault();
            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrPrmtPsprtUpdate.fld_LbrRefID);
  
            if (GetLabourDetails != null)
            {
                    GetExistingPassport.fld_LbrRefID = tbl_LbrPrmtPsprtUpdate.fld_LbrRefID;
                    GetExistingPassport.fld_PassportRenewalStatus = tbl_LbrPrmtPsprtUpdate.fld_PassportRenewalStatus;
                    GetExistingPassport.fld_NewPrmtPsprtNo = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo;
                    GetExistingPassport.fld_ModifiedBy = GetUserID;
                    GetExistingPassport.fld_ModifiedDT = DT;
                    db.Entry(GetExistingPassport).State = EntityState.Modified;
                    db.SaveChanges();

                    GetLabourDetails.fld_ModifiedBy = GetUserID;
                    GetLabourDetails.fld_ModifiedDT = DT;
                    GetLabourDetails.fld_PassportRenewalStatus = tbl_LbrPrmtPsprtUpdate.fld_PassportRenewalStatus;
                    GetLabourDetails.fld_WorkerIDNo = tbl_LbrPrmtPsprtUpdate.fld_NewPrmtPsprtNo;
                    db.Entry(GetLabourDetails).State = EntityState.Modified;
                    db.SaveChanges();

                    SyncToCheckRollFunc(GetLabourDetails, 2);
                    ViewBag.MsgColor = "color: green";
            }
            List<SelectListItem> PassportRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatus = new List<SelectListItem>();
            PassportRenewalStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrPrmtPsprtUpdate.fld_PassportRenewalStatus).ToList();
            PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            PassportPermitStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrPrmtPsprtUpdate.fld_PassportStatus).ToList();
            PassportPermitStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "0" }));
            ViewBag.fld_PassportRenewalStatus = PassportRenewalStatus;
            ViewBag.fld_PassportStatus = PassportPermitStatus;
            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(tbl_LbrPrmtPsprtUpdate.fld_LbrRefID);
            return RedirectToAction("PassportUpdate", "LabourPrmtPsprt", new { id = LbrDataInfo.fld_ID });
 
        }
        public bool SyncToCheckRollFunc(tbl_LbrDataInfo tbl_LbrDataInfo, short Indc)
        {
            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            bool Result = false;
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, tbl_LbrDataInfo.fld_WilayahID, tbl_LbrDataInfo.fld_SyarikatID, tbl_LbrDataInfo.fld_NegaraID, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
            tbl_Pkjmast tbl_Pkjmast = new tbl_Pkjmast();

            try
            {
                tbl_Pkjmast = dbEstate.tbl_Pkjmast.Where(x => x.fld_Nopkj == tbl_LbrDataInfo.fld_WorkerNo && x.fld_NegaraID == tbl_LbrDataInfo.fld_NegaraID && x.fld_SyarikatID == tbl_LbrDataInfo.fld_SyarikatID && x.fld_WilayahID == tbl_LbrDataInfo.fld_WilayahID && x.fld_LadangID == tbl_LbrDataInfo.fld_LadangID).FirstOrDefault();
                if (Indc == 1)
                {
                    tbl_Pkjmast.fld_Prmtno = tbl_LbrDataInfo.fld_PermitNo;
                    tbl_Pkjmast.fld_T2prmt = tbl_LbrDataInfo.fld_PermitEndDT;
                }
                else
                {
                    tbl_Pkjmast.fld_Psptno = tbl_LbrDataInfo.fld_WorkerIDNo;
                    tbl_Pkjmast.fld_T2pspt = tbl_LbrDataInfo.fld_PassportEndDT;
                }
                dbEstate.Entry(tbl_Pkjmast).State = EntityState.Modified;
                dbEstate.SaveChanges();
                Result = true;
            }
            catch (Exception ex)
            {
                Result = false;
            }
            finally
            {
                dbEstate.Dispose();
            }

            return Result;
        }

        public async Task<ActionResult> UpDownloadFilePrmtPsprt(Guid LabourID)
        {
            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            List<SelectListItem> fld_UploadTypeFile = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "uploadtypefile" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            tbl_LbrDataInfo LbrDataInfo = new tbl_LbrDataInfo();
            fld_UploadTypeFile = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "uploadtypefile" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", null).ToList();

            ViewBag.fld_UploadTypeFile = fld_UploadTypeFile;
            LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(LabourID);

            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpDownloadFilePrmtPsprt(tbl_LbrDataInfo tbl_LbrDataInfo, string fld_UploadTypeFile, HttpPostedFileBase FileUpload)
        {
            ViewBag.LabourPrmtPsprt = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            tbl_LbrDataInfo LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(tbl_LbrDataInfo.fld_ID);

            try
            {
                if (FileUpload != null && FileUpload.ContentLength > 0)
                {
                    var FileName = Path.GetFileName(FileUpload.FileName);
                    FileName = FileName.Replace(" ", "_");
                    var path = Server.MapPath("~/FileUploaded/Profile/" + tbl_LbrDataInfo.fld_ID + "/");
                    bool Exist = Directory.Exists(path);
                    if (!Exist)
                    {
                        Directory.CreateDirectory(path);
                    }
                    var pathfile = Path.Combine(path, fld_UploadTypeFile + "_" + DT.ToString("dd-MM-yyyy") + ".pdf");
                    FileUpload.SaveAs(pathfile);
                    ModelState.AddModelError("", "File Successfully Uploaded");
                    ViewBag.MsgColor = "color: green";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Fail Uploaded");
                ViewBag.MsgColor = "color: red";
            }

            List<SelectListItem> fld_UploadTypeFile2 = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "uploadtypefile" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            fld_UploadTypeFile2 = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "uploadtypefile" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", null).ToList();

            ViewBag.fld_UploadTypeFile = fld_UploadTypeFile2;

            return View(LbrDataInfo);
        }

        public ActionResult _LabourProfileUploadedFileDetail(Guid LabourID)
        {
            string GetPath = Server.MapPath("~/FileUploaded/Profile/" + LabourID + "/");
            FileInfo[] Files = null;
            DirectoryInfo DirectoryInfo = new DirectoryInfo(GetPath);
            List<CustMod_FileUpload2> CustMod_FileUpload = new List<CustMod_FileUpload2>();
            IOrderedEnumerable<FileInfo> GetFiles;
            bool Exist = Directory.Exists(GetPath);
            if (Exist)
            {
                Files = DirectoryInfo.GetFiles();
                GetFiles = Files.Where(f => f.Extension == ".pdf").OrderBy(o => o.CreationTime);
                int I = 1;
                foreach (var GetFile in GetFiles)
                {
                    CustMod_FileUpload.Add(new CustMod_FileUpload2() { ID = I, FileName = GetFile.Name, DateTimeCreated = GetFile.CreationTime, DateTimeModified = GetFile.LastWriteTime, SizeFile = GetFile.Length, RefID = LabourID });
                    I++;
                }
            }

            return View(CustMod_FileUpload);
        }

        public FileResult DownloadFile(string FileName, Guid FileRefID)
        {
            DownloadFiles.FileDownloads DownloadObj = new DownloadFiles.FileDownloads();
            string GetPath = Server.MapPath("~/FileUploaded/Profile/" + FileRefID + "/");
            var GetFileDetail = DownloadObj.GetFiles(GetPath);
            var CurrentFileName = GetFileDetail.Where(x => x.FileName == FileName).FirstOrDefault();

            string contentType = string.Empty;
            contentType = "application/pdf";
            return File(CurrentFileName.FilePath, contentType, CurrentFileName.FileName);
        }

        public ActionResult DeleteFile(string FileName, Guid FileRefID)
        {
            string GetPathFile = Server.MapPath("~/FileUploaded/Profile/" + FileRefID + "/" + FileName);

            bool Exists = System.IO.File.Exists(GetPathFile);
            if (Exists)
            {
                System.IO.File.Delete(GetPathFile);
            }

            return RedirectToAction("UpDownloadFilePrmtPsprt", new { LabourID = FileRefID });
        }

    }

}
