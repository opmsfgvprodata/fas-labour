using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVC_SYSTEM.LabourModels;
using MVC_SYSTEM.Class;
using MVC_SYSTEM.MasterModels;
using MVC_SYSTEM.App_LocalResources;
using MVC_SYSTEM.Attributes;
using MVC_SYSTEM.EstateModels;
using MVC_SYSTEM.CustomModels;
using static MVC_SYSTEM.Class.GlobalFunction;

//Added by Shazana on 10/8
using Rotativa;
using System.Web.Security;
using System.IO;
using MVC_SYSTEM.log;
using Itenso.TimePeriod;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    public class LabourReportController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        

        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private GetConfig GetConfig = new GetConfig();
        private List<tbl_LbrDataInfo> LbrDataInfo = new List<tbl_LbrDataInfo>();
        private GeneralFunc GeneralFunc = new GeneralFunc();
        private GetLadang GetLadang = new GetLadang();
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        ConvertToPdf ConvertToPdf = new ConvertToPdf();
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;
        int? BonusPercent;
        errorlog geterror = new errorlog(); //Added by Shazana on 17/8
        // GET: LabourReport

        public async Task<ActionResult> Index()
        {
            ViewBag.LabourReport = "class = active";
            int? getuserid = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, getuserid, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);

            //* added by nana: 17.07.2020
            List<SelectListItem> Fld_Reports = new List<SelectListItem>();
            var GetReportList = Masterdb.tblMenuLists.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Flag == "LabourReportList" && x.fldDeleted == false).ToList();
            Fld_Reports = new SelectList(GetReportList, "fld_Id", "fld_Desc").ToList();
            ViewBag.ReportList = Fld_Reports;
            //* close added by nana: 17.07.2020

            return View(await Masterdb.tblMenuLists.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Flag == "LabourReportList" && x.fldDeleted == false).ToListAsync());
        }

        //* added by nana: 17.07.2020
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Resource,Viewer")]
        public ActionResult Index(int ReportList)
        {
            string action = "", controller = "";
            var getreport = Masterdb.tblMenuLists.Find(ReportList);

            //if (getreport. == true )
            //{
            //getreport = Masterdb.tblMenuLists.Where(x => x.fld_ID == ReportList).FirstOrDefault();
            action = getreport.fld_Val;
            controller = getreport.fld_Sub;
            //}
            //else
            //{
            action = getreport.fld_Sub;
            controller = getreport.fld_Val;
            //}
            return RedirectToAction(action, controller);
        }
        //* close added by nana: 17.07.2020

        public ActionResult LabourInfo(int? WilayahIDList, int? LadangIDList)
        {
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();

            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

            int? GetWilayahID = 0;
            
            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();

            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = "all";
                    ViewBag.Estate = "all";
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                     .Where(x => x.fld_ID == WilayahIDList)
                     .Select(s => s.fld_WlyhName)
                     .FirstOrDefault();
                    ViewBag.Estate = "all";
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                     .Where(x => x.fld_ID == WilayahIDList)
                     .Select(s => s.fld_WlyhName)
                     .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).ToList();
                ViewBag.Region = Masterdb.tbl_Wilayah
                         .Where(x => x.fld_ID == WilayahIDList)
                         .Select(s => s.fld_WlyhName)
                         .FirstOrDefault();
                ViewBag.Estate = Masterdb.tbl_Ladang
                   .Where(x => x.fld_ID == LadangIDList)
                   .Select(s => s.fld_LdgName)
                     .FirstOrDefault();
            }

            return View(tbl_LbrDataInfo);
        }

        public ActionResult LabourPrmtPsprtSurvey(int? WilayahIDList, int? LadangIDList, int? MonthList, int? YearList)
        {
            ViewBag.Report = "class = active";
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1; //modified by husna on 28/1/2020    (1)
            int RangeYear = DT.Year + 1;

            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

            var YearList1 = new List<SelectListItem>();
            for (var i = Year ; i <= RangeYear; i++)
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


            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.YearList = YearList1;
            ViewBag.MonthList = MonthList1;
            ViewBag.Year = YearList;
            ViewBag.Month = MonthList;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();

            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = "All"; //modified by husna on 28/1/2020      (2)
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020      (3)
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                           .Where(x => x.fld_ID == WilayahIDList)
                           .Select(s => s.fld_WlyhName)
                           .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020     (4)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                            .Where(x => x.fld_ID == WilayahIDList)
                            .Select(s => s.fld_WlyhName)
                            .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                .Where(x => x.fld_ID == WilayahIDList)
                                .Select(s => s.fld_WlyhName)
                                .FirstOrDefault();
                    ViewBag.Estate = "All";  //modified by husna on 28/1/2020     (5)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                               .Where(x => x.fld_ID == WilayahIDList)
                               .Select(s => s.fld_WlyhName)
                               .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }

            return View(tbl_LbrDataInfo);
        }

        public ActionResult LabourCostPrmtPsprtSurvey(int? WilayahIDList, int? LadangIDList, int? MonthList, int? YearList)
        {
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1; //modified by husna on 28/1/2020      (6)
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


            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.YearList = YearList1;
            ViewBag.MonthList = MonthList1;
            ViewBag.Year = YearList;
            ViewBag.Month = MonthList;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();

            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = "All"; //modified by husna on 28/1/2020        (7)
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (8)
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020       (9)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                              .Where(x => x.fld_ID == WilayahIDList)
                              .Select(s => s.fld_WlyhName)
                              .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                               .Where(x => x.fld_ID == WilayahIDList)
                               .Select(s => s.fld_WlyhName)
                               .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (10)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                               .Where(x => x.fld_ID == WilayahIDList)
                               .Select(s => s.fld_WlyhName)
                               .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }

            return View(tbl_LbrDataInfo);
        }


        public ActionResult _LabourCostPrmtPsprtSurvey(int? WilayahIDList, int? LadangIDList, int? MonthList, int? YearList, string print)
        {

            //LocalReport lr = new LocalReport();

            string path = "";
            path = Path.Combine(Server.MapPath("~/LabourReport"), "_LabourCostPrmtPsprtSurvey.cshtml");



            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1; //modified by husna on 28/1/2020      (6)
            int RangeYear = DT.Year + 1;

            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

            ViewBag.Print = print;
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


            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.YearList = YearList1;
            ViewBag.MonthList = MonthList1;
            ViewBag.Year = YearList;
            ViewBag.Month = MonthList;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();

            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = "All"; //modified by husna on 28/1/2020        (7)
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (8)
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020       (9)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                              .Where(x => x.fld_ID == WilayahIDList)
                              .Select(s => s.fld_WlyhName)
                              .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                               .Where(x => x.fld_ID == WilayahIDList)
                               .Select(s => s.fld_WlyhName)
                               .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (10)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                               .Where(x => x.fld_ID == WilayahIDList)
                               .Select(s => s.fld_WlyhName)
                               .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }

            if (WilayahIDList == null && LadangIDList == null )
            {
                ViewBag.Message = GlobalResEstate.msgChooseRegionEstateMonthYear;
               
            }

            return View(tbl_LbrDataInfo);
        }
        //public ActionResult LabourCostPrmtPsprtSurvey1(int? WilayahIDList, int? LadangIDList, int? MonthList, int? YearList)
        //{
        //    ViewBag.LabourReport = "class = active";
        //    GetUserID = GetIdentity.ID(User.Identity.Name);
        //    GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
        //    Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
        //    db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
        //    DT = ChangeTimeZone.gettimezone();
        //    List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
        //    List<SelectListItem> fld_LadangID = new List<SelectListItem>();
        //    int Month = DT.AddMonths(-1).Month;
        //    int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1; //modified by husna on 28/1/2020      (6)
        //    int RangeYear = DT.Year + 1;

        //    ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
        //    ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

        //    var YearList1 = new List<SelectListItem>();
        //    for (var i = Year; i <= RangeYear; i++)
        //    {
        //        if (i == DT.Year)
        //        {
        //            YearList1.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = true });
        //        }
        //        else
        //        {
        //            YearList1.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
        //        }
        //    }

        //    var MonthList1 = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "monthlist" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID), "fldOptConfValue", "fldOptConfDesc");


        //    int? GetWilayahID = 0;

        //    if (WilayahID == 0 && LadangID == 0)
        //    {
        //        var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
        //        fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
        //        fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
        //        var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

        //        if (WilayahIDList != null)
        //        {
        //            GetWilayahID = WilayahIDList;
        //        }
        //        else
        //        {
        //            GetWilayahID = GetTopWilayahID.fld_ID;
        //        }

        //        fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
        //        fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
        //    }
        //    else
        //    {
        //        var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_ID == WilayahID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
        //        fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();

        //        var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();
        //        if (WilayahIDList != null)
        //        {
        //            GetWilayahID = WilayahIDList;
        //        }
        //        else
        //        {
        //            GetWilayahID = GetTopWilayahID.fld_ID;
        //        }

        //        fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == WilayahID && x.fld_ID == LadangID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
        //    }
        //    ViewBag.WilayahIDList = fld_WilayahID;
        //    ViewBag.LadangIDList = fld_LadangID;
        //    ViewBag.YearList = YearList1;
        //    ViewBag.MonthList = MonthList1;
        //    ViewBag.Year = YearList;
        //    ViewBag.Month = MonthList;

        //    List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();

        //    if (WilayahID == 0 && LadangID == 0)
        //    {
        //        if (WilayahIDList == 0 && LadangIDList == 0)
        //        {
        //            tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
        //            ViewBag.Region = "All"; //modified by husna on 28/1/2020        (7)
        //            ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (8)
        //        }
        //        else if (WilayahIDList != 0 && LadangIDList == 0)
        //        {
        //            tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
        //            ViewBag.Region = Masterdb.tbl_Wilayah
        //                         .Where(x => x.fld_ID == WilayahIDList)
        //                         .Select(s => s.fld_WlyhName)
        //                         .FirstOrDefault();
        //            ViewBag.Estate = "All"; //modified by husna on 28/1/2020       (9)
        //        }
        //        else
        //        {
        //            tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
        //            ViewBag.Region = Masterdb.tbl_Wilayah
        //                      .Where(x => x.fld_ID == WilayahIDList)
        //                      .Select(s => s.fld_WlyhName)
        //                      .FirstOrDefault();
        //            ViewBag.Estate = Masterdb.tbl_Ladang
        //               .Where(x => x.fld_ID == LadangIDList)
        //               .Select(s => s.fld_LdgName)
        //                 .FirstOrDefault();
        //        }
        //    }
        //    else
        //    {
        //        if (LadangID == 0)
        //        {
        //            tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
        //            ViewBag.Region = Masterdb.tbl_Wilayah
        //                       .Where(x => x.fld_ID == WilayahIDList)
        //                       .Select(s => s.fld_WlyhName)
        //                       .FirstOrDefault();
        //            ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (10)
        //        }
        //        else
        //        {
        //            tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
        //            ViewBag.Region = Masterdb.tbl_Wilayah
        //                       .Where(x => x.fld_ID == WilayahIDList)
        //                       .Select(s => s.fld_WlyhName)
        //                       .FirstOrDefault();
        //            ViewBag.Estate = Masterdb.tbl_Ladang
        //               .Where(x => x.fld_ID == LadangIDList)
        //               .Select(s => s.fld_LdgName)
        //                 .FirstOrDefault();
        //        }
        //    }

        //    return View(tbl_LbrDataInfo);
        //}

        public ActionResult LabourMYEGTemplate(int? WilayahIDList, int? LadangIDList, int? MonthList, int? YearList, bool? AllDate)
        {
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1; //modified by husna on 28/1/2020      (11)
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


            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.YearList = YearList1;
            ViewBag.MonthList = MonthList1;
            ViewBag.Year = YearList;
            ViewBag.Month = MonthList;
            ViewBag.Region = WilayahIDList;
            ViewBag.Estate = LadangIDList;
            ViewBag.AllDate = AllDate;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();

            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = AllDate == false ? db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList()
                    :
                    db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList()
                    ;

                    ViewBag.Region = "All"; //modified by husna on 28/1/2020        (12)
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (13)
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = AllDate == false ? db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList() 
                    :
                    db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList()
                    ;
                    ViewBag.Region = Masterdb.tbl_Wilayah
                            .Where(x => x.fld_ID == WilayahIDList)
                            .Select(s => s.fld_WlyhName)
                            .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (14)
                }
                else
                {
                    tbl_LbrDataInfo = AllDate == false ? db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList()
                    :
                    db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList &&  x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList()
                    ;
                    ViewBag.Region = Masterdb.tbl_Wilayah
                            .Where(x => x.fld_ID == WilayahIDList)
                            .Select(s => s.fld_WlyhName)
                            .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = AllDate == false ? db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList()
                    :
                    db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList()
                    ;
                    ViewBag.Region = Masterdb.tbl_Wilayah
                     .Where(x => x.fld_ID == WilayahIDList)
                     .Select(s => s.fld_WlyhName)
                     .FirstOrDefault();
                    ViewBag.Estate = "All";    //modified by husna on 28/1/2020      (15)
                }
                else
                {
                    tbl_LbrDataInfo = AllDate == false ? db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList()
                    :
                    db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList()
                    ;
                    ViewBag.Region = Masterdb.tbl_Wilayah
                           .Where(x => x.fld_ID == WilayahIDList)
                           .Select(s => s.fld_WlyhName)
                           .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }

            return View(tbl_LbrDataInfo);
        }

        public ActionResult LabourCostMYEGTemplate(int? WilayahIDList, int? LadangIDList, int? MonthList, int? YearList)
        {
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1; //modified by husna on 28/1/2020      (16)
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


            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.YearList = YearList1;
            ViewBag.MonthList = MonthList1;
            ViewBag.Year = YearList;
            ViewBag.Month = MonthList;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();

            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = "All"; //modified by husna on 28/1/2020        (17)
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (18)
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                .Where(x => x.fld_ID == WilayahIDList)
                .Select(s => s.fld_WlyhName)
                .FirstOrDefault();
                    ViewBag.Estate = "All";//modified by husna on 28/1/2020         (19)

                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();

                    ViewBag.Region = Masterdb.tbl_Wilayah
                   .Where(x => x.fld_ID == WilayahIDList)
                   .Select(s => s.fld_WlyhName)
                   .FirstOrDefault();
                ViewBag.Estate = Masterdb.tbl_Ladang
                      .Where(x => x.fld_ID == LadangIDList)
                      .Select(s => s.fld_LdgName)
                        .FirstOrDefault();
                }

              
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (20)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_PermitEndDT.Value.Month == MonthList && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                    .Where(x => x.fld_ID == WilayahIDList)
                    .Select(s => s.fld_WlyhName)
                    .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                     .Where(x => x.fld_ID == LadangIDList)
                     .Select(s => s.fld_LdgName)
                       .FirstOrDefault();
                }
            }
            return View(tbl_LbrDataInfo);
        }

        public ActionResult LabourWorkerMovementStatus(int? WilayahIDList, int? LadangIDList, int? MonthList, int? YearList)
        {
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1; //modified by husna on 28/1/2020      (21)
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


            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.YearList = YearList1;
            ViewBag.MonthList = MonthList1;
            ViewBag.Year = YearList;
            ViewBag.Month = MonthList;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();
            List<vw_NSWL> GetNSWLList = new List<vw_NSWL>();

            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = "All"; //modified by husna on 28/1/2020        (22)
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (23)
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (24)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (25)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }

            DateTime LastMonth = DT.AddMonths(-1);
            var GetEstateSizeList = GetLadang.GetEstateAreaByLevel(SyarikatID, NegaraID);
            var RatioSetting = Masterdb.tblOptionConfigsWebs.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldOptConfFlag1 == "ratioperha").Select(s => s.fldOptConfValue).FirstOrDefault();
            decimal RatioPerHa = decimal.Parse(RatioSetting);
            List<CustMod_StatusMovementRpt> CustMod_StatusMovementRpt = new List<CustMod_StatusMovementRpt>();
            int ID = 1;
            if (tbl_LbrDataInfo.Count > 0)
            {
                foreach (var NSWL in GetNSWLList)
                {
                    var GetEstateSize = GetEstateSizeList.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID).FirstOrDefault();
                    Decimal Need = GetEstateSize.fld_SizeArea / RatioPerHa;
                    int NeedInt = Decimal.ToInt32(Need);
                    int LastMonthTotalData = 0;
                    if (MonthList == 1)
                    {
                        LastMonthTotalData = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Year == YearList).Count();
                    }
                    else
                    {
                        LastMonthTotalData = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Month < MonthList && x.fld_CreatedDT.Value.Year == YearList).Count();
                    }
                    var Absconded = db.tbl_LbrAbsconded.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_Deleted == false).ToList();
                    var AbscondedBI = Absconded.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Month == MonthList && x.fld_CreatedDT.Value.Year == YearList && x.fld_Deleted == false).Count();
                    var AbscondedHK = Absconded.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Year == YearList && x.fld_Deleted == false).Count();
                    var EndContract = db.tbl_LbrEndContract.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_Deleted == false).ToList();
                    var EndContractBI = EndContract.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Month == MonthList && x.fld_CreatedDT.Value.Year == YearList && x.fld_Deleted == false).Count();
                    var EndContractHK = EndContract.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Year == YearList && x.fld_Deleted == false).Count();
                    var FailFomema = db.tbl_LbrFomemaRslt.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_FomemaResult == false && x.fld_Deleted == false).ToList();
                    var FailFomemaBI = FailFomema.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Month == MonthList && x.fld_CreatedDT.Value.Year == YearList && x.fld_Deleted == false).Count();
                    var FailFomemaHK = FailFomema.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Year == YearList && x.fld_Deleted == false).Count();
                    var SickDeath = db.tbl_LbrSickDeath.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_Deleted == false).ToList();
                    var SickDeathBI = SickDeath.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Month == MonthList && x.fld_CreatedDT.Value.Year == YearList && x.fld_Deleted == false).Count();
                    var SickDeathHK = SickDeath.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Year == YearList && x.fld_Deleted == false).Count();
                    var TotalComBI = EndContractBI + FailFomemaBI + SickDeathBI;
                    var TotalComHK = EndContractHK + FailFomemaHK + SickDeathHK;

                    var BalanceWorker = LastMonthTotalData - TotalComBI;

                    var NewWorkerBI = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Month == MonthList && x.fld_CreatedDT.Value.Year == YearList && x.fld_TransferToChckrollWorkerTransferStatus == null && x.fld_WorkerTransferCode == null).Count();
                    var NewWorkerHK = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Year == YearList && x.fld_TransferToChckrollWorkerTransferStatus == null && x.fld_WorkerTransferCode == null).Count();

                    var TransferOut = db.tbl_LbrTrnsferHistory.Where(x => x.fld_NegaraIDFrom == NSWL.fld_NegaraID && x.fld_SyarikatIDFrom == NSWL.fld_SyarikatID && x.fld_WilayahIDFrom == NSWL.fld_WilayahID && x.fld_LadangIDFrom == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Month == MonthList && x.fld_CreatedDT.Value.Year == YearList && x.fld_SuccessTransferd == true).Count();
                    var TransferIn = db.tbl_LbrTrnsferHistory.Where(x => x.fld_NegaraIDTo == NSWL.fld_NegaraID && x.fld_SyarikatIDTo == NSWL.fld_SyarikatID && x.fld_WilayahIDTo == NSWL.fld_WilayahID && x.fld_LadangIDTo == NSWL.fld_LadangID && x.fld_CreatedDT.Value.Month == MonthList && x.fld_CreatedDT.Value.Year == YearList && x.fld_SuccessTransferd == true).Count();

                    var BalanceWorkerBI = BalanceWorker - TransferOut + TransferIn;

                    var TotalTKI = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "IN").Count();
                    var TotalTKB = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "BA").Count();
                    var TotalTKD = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "ID").Count();
                    var TotalTKN = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "NE").Count();
                    var TotalPOL = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "MA").Count();

                    var TotalWorker = TotalTKI + TotalTKB + TotalTKD + TotalTKN + TotalPOL;

                    var Approximately = TotalWorker - NeedInt;

                    var ApproximatelyRatio = 0;

                    CustMod_StatusMovementRpt.Add(new CustMod_StatusMovementRpt()
                    {
                        fld_ID = ID,
                        fld_NegaraID = NSWL.fld_NegaraID,
                        fld_SyarikatID = NSWL.fld_SyarikatID,
                        fld_WilayahID = NSWL.fld_WilayahID,
                        fld_LadangID = NSWL.fld_LadangID,
                        fld_EstateName = NSWL.fld_NamaLadang,
                        fld_EstateSize = GetEstateSize.fld_SizeArea,
                        fld_Need = NeedInt,
                        fld_RatioPerHa = RatioSetting,
                        fld_TotalLastMonth = LastMonthTotalData,
                        fld_TotalAbscondedBI = AbscondedBI,
                        fld_TotalAbscondedHK = AbscondedHK,
                        fld_TotalCOMBI = TotalComBI,
                        fld_TotalCOMHK = TotalComHK,
                        fld_WorkerBalance = BalanceWorker,
                        fld_NewWorkerBI = NewWorkerBI,
                        fld_NewWorkerHK = NewWorkerHK,
                        fld_TransferIn = TransferIn,
                        fld_TransferOut = TransferOut,
                        fld_WorkerBalanceBI = BalanceWorkerBI,
                        fld_TotalTKI = TotalTKI,
                        fld_TotalTKB = TotalTKB,
                        fld_TotalTKD = TotalTKD,
                        fld_TotalTKN = TotalTKN,
                        fld_TotalPOL = TotalPOL,
                        fld_TotalWorker = TotalWorker,
                        fld_Approximately = Approximately,
                        fld_ApproximatelyRatio = ApproximatelyRatio
                    });
                }
            }
            var CustMod_StatusMovementRptSorting = CustMod_StatusMovementRpt.OrderBy(o => o.fld_EstateName).ToList();
            return View(CustMod_StatusMovementRptSorting);
        }

        public ActionResult LabourWorkerBonus(int? WilayahIDList, int? LadangIDList, string Quarter, int? YearList)
        {
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1; //modified by husna on 28/1/2020      (26)
            int RangeYear = DT.Year + 1;

            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

            string[] WebConfigFilter = new string[] { "quarter", "ratebonus" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

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

            var Quarter1 = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "quarter" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            
            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.YearList = YearList1;
            ViewBag.Quarter = Quarter1;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();
            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = "All"; //modified by husna on 28/1/2020        (27)
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (28)
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                   .Where(x => x.fld_ID == WilayahIDList)
                                   .Select(s => s.fld_WlyhName)
                                   .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (29)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                    .Where(x => x.fld_ID == WilayahIDList)
                                    .Select(s => s.fld_WlyhName)
                                    .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                   .Where(x => x.fld_ID == WilayahIDList)
                                   .Select(s => s.fld_WlyhName)
                                   .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (30)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                    .Where(x => x.fld_ID == WilayahIDList)
                                    .Select(s => s.fld_WlyhName)
                                    .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            //var CustMod_GajiKasar = new List<CustMod_GajiKasar>();
            List<tbl_LbrDataInfo> LbrDataInfo = new List<tbl_LbrDataInfo>();
            List<tbl_LbrDataInfo> LbrDataInfoFinal = new List<tbl_LbrDataInfo>();
            List<CustMod_GajiKasar> CustMod_GajiKasar = new List<CustMod_GajiKasar>();  //Added by Shazana on 23/8
            List<tbl_LbrDataInfo> LbrDataInfoFinal2 = new List<tbl_LbrDataInfo>();
            return View(CustMod_GajiKasar.OrderBy(x => x.fld_LadangID));
        }

        public ActionResult _LabourWorkerBonus(int? WilayahIDList, int? LadangIDList, string Quarter, int? YearList,string print)
        {
            ViewBag.Print = print;
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1; //modified by husna on 28/1/2020      (26)
            int RangeYear = DT.Year + 1;
          

            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

            string[] WebConfigFilter = new string[] { "quarter", "ratebonus" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

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

            var Quarter1 = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "quarter" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();

            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.YearList = YearList1;
            ViewBag.Quarter = Quarter1;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();
            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = "All"; //modified by husna on 28/1/2020        (27)
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (28)
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                   .Where(x => x.fld_ID == WilayahIDList)
                                   .Select(s => s.fld_WlyhName)
                                   .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (29)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                    .Where(x => x.fld_ID == WilayahIDList)
                                    .Select(s => s.fld_WlyhName)
                                    .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                   .Where(x => x.fld_ID == WilayahIDList)
                                   .Select(s => s.fld_WlyhName)
                                   .FirstOrDefault();
                    ViewBag.Estate = "All"; //modified by husna on 28/1/2020        (30)
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_ActiveStatus == "1" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA").OrderBy(o => o.fld_WorkerName).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                    .Where(x => x.fld_ID == WilayahIDList)
                                    .Select(s => s.fld_WlyhName)
                                    .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            //var CustMod_GajiKasar = new List<CustMod_GajiKasar>();
            List<tbl_LbrDataInfo> LbrDataInfo = new List<tbl_LbrDataInfo>();
            List<tbl_LbrDataInfo> LbrDataInfoFinal = new List<tbl_LbrDataInfo>();
            List<CustMod_GajiKasar> CustMod_GajiKasar = new List<CustMod_GajiKasar>();  //Added by Shazana on 23/8
            List<tbl_LbrDataInfo> LbrDataInfoFinal2 = new List<tbl_LbrDataInfo>();

            //Added by Shazana on 21 / 8

            if (Quarter != null && WilayahIDList == 1)
            {
                
                var GetRateBonus = GetDropdownList.Where(x => x.fldOptConfFlag1 == "ratebonus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
                var GetQuaterDetail = GetDropdownList.Where(x => x.fldOptConfFlag1 == "quarter" && x.fldOptConfValue == Quarter && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).FirstOrDefault();
                foreach (var RateBonus in GetRateBonus)
                {
                    var SelectionMonthStart = int.Parse(GetQuaterDetail.fldOptConfFlag2);
                    var SelectionMonthEnd = int.Parse(GetQuaterDetail.fldOptConfFlag3);
                    var GetYearService = int.Parse(RateBonus.fldOptConfFlag2);
                    var GetYearServiceLookUp = YearList - GetYearService;
                    if (GetYearService < 10)
                    {
                        LbrDataInfo = tbl_LbrDataInfo.Where(x => x.fld_ArrivedDT.Value.Month >= SelectionMonthStart && x.fld_ArrivedDT.Value.Month <= SelectionMonthEnd && x.fld_ArrivedDT.Value.Year == GetYearServiceLookUp).ToList();
                    }
                    else
                    {
                        LbrDataInfo = tbl_LbrDataInfo.Where(x => x.fld_ArrivedDT.Value.Month >= SelectionMonthStart && x.fld_ArrivedDT.Value.Month <= SelectionMonthEnd && x.fld_ArrivedDT.Value.Year <= GetYearServiceLookUp).ToList();
                    }
                    LbrDataInfoFinal.AddRange(LbrDataInfo);
                }
                var Alldetals = LbrDataInfoFinal.ToList();


                string Host1, Catalog1, UserID1, Pass1 = "";
                string purpose2 = "CHECKROLL";
                MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
                if (WilayahIDList == 1)
                {
                    Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, 1, SyarikatID, NegaraID, purpose2);
                    dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
                }
                else
                {
                    Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, 2, SyarikatID, NegaraID, purpose2);
                    dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
                }


                var SelectedMonthStart = int.Parse(GetQuaterDetail.fldOptConfFlag2);
                var SelectedMonthEnd = int.Parse(GetQuaterDetail.fldOptConfFlag3);
               
                
                int Tahun;
                Tahun = (int)YearList;
                String QuarterDuration =  Quarter + " / " +  Tahun;
                ViewBag.Duration = QuarterDuration;
                foreach (var some in LbrDataInfoFinal)
                {
                    DateTime fromDate = new DateTime(Tahun, some.fld_ArrivedDT.Value.Month, 28).AddMonths(-12);
                    //DateTime.Now() <= p.EndDate.AddDays(7)
                    //sarah added 10/01/2022
                    DateTime endDate = new DateTime(Tahun, some.fld_ArrivedDT.Value.Month, 28);
                    //original code
                    //var GajiKasarSetahun = dbEstate.tbl_GajiBulanan.Where(x => x.fld_Nopkj == some.fld_WorkerNo && fromDate <= x.fld_DTCreated).ToList();
                    //sarah modified 10/01/2022
                    var GajiKasarSetahun = dbEstate.tbl_GajiBulanan.Where(x => x.fld_Nopkj == some.fld_WorkerNo && fromDate <= x.fld_DTCreated && endDate >= x.fld_DTCreated).ToList();
                    //sarah end
                    var JumlahGajiKasarSetahun =(GajiKasarSetahun.Sum(s => s.fld_GajiKasar));
                    var PurataPendapatanSetahun = ( JumlahGajiKasarSetahun / 12);
                    var YearStiker = GeneralFunc.CalAge(some.fld_ArrivedDT.Value);


                    if (YearStiker >=  4)
                    {

                        var GetConfigData = GetConfig.GetConfigDesc(NegaraID, SyarikatID, WebConfigFilter);
                        

                            if (YearStiker <= 10)
                            {
                            BonusPercent = GetConfigData.Where(x => int.Parse(x.fldOptConfFlag2) == YearStiker && x.fldOptConfFlag1 == "ratebonus").Select(s => int.Parse(s.fldOptConfFlag3)).FirstOrDefault();
                            }
                        else
                            {
                            BonusPercent = GetConfigData.Where(x => int.Parse(x.fldOptConfFlag2) == 10 && x.fldOptConfFlag1 == "ratebonus").Select(s =>int.Parse( s.fldOptConfFlag3)).FirstOrDefault();
                        }

                       var BonusIndividu = (PurataPendapatanSetahun * (BonusPercent) / 100);

                        CustMod_GajiKasar.Add(new CustMod_GajiKasar()
                        {

                            fld_NegaraID = some.fld_NegaraID,
                            fld_WorkerIDNo = some.fld_WorkerIDNo,
                            fld_PermitNo = some.fld_PermitNo,
                            fld_WorkerNo = some.fld_WorkerNo,
                            fld_WorkerName = some.fld_WorkerName,
                            fld_Nationality = some.fld_Nationality,
                            fld_MarriedStatus = some.fld_MarriedStatus,
                            fld_FeldaRelated = some.fld_FeldaRelated,
                            fld_SyarikatID = some.fld_SyarikatID,
                            fld_WilayahID = some.fld_WilayahID,
                            fld_LadangID = some.fld_LadangID,
                            fld_GajiKasarSetahun = JumlahGajiKasarSetahun,
                            fld_ArrivedDT = some.fld_ArrivedDT,
                            fld_PassportEndDT = some.fld_PassportEndDT,
                            fld_PermitEndDT = some.fld_PermitEndDT,
                            fld_BOD = some.fld_BOD,
                            BonusIndividu = BonusIndividu,
                            PurataPendapatanSetahun = PurataPendapatanSetahun,
                            //CustMod_GajiKasarData = CustMod_GajiKasar.ToList();
                        }); ;


                    }
                    


                    //nanana
                }

               // return View(CustMod_GajiKasar);
            }

            return View(CustMod_GajiKasar.OrderBy(x=> x.fld_LadangID));
        }

        public ActionResult PrintWorkerPdfLabourWorkerBonus(int? RadioGroup, string WilayahIDList, string LadangIDList, string Quarter, int? YearList, int id, string genid)
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
                report = new ActionAsPdf("_LabourWorkerBonus", new { WilayahIDList, LadangIDList, Quarter, YearList , print })
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
        public string GetWorkerFromEstateData(int? NegaraID, int? SyarikatID, int? WilayahID, int? LadangID, string nopkjnew)
        {
            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);

            var getpkj = dbEstate.tbl_Pkjmast.Where(x => x.fld_KodSAPPekerja.Contains(nopkjnew) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID).Select(s => s.fld_Nopkj.Trim()).Distinct().OrderByDescending(s => s).FirstOrDefault();

            dbEstate.Dispose();

            return getpkj;
        }

        //comment by shazana
        //public ActionResult LabourMasterData(int? NegaraID, int? SyarikatID, int? WilayahID, int? WilayahIDList, int? LadangIDList, string NoPkj, string Supplier, string Nationality, string CostCenter, string Gender, string Status, tbl_LbrDataInfo tbl_LbrDataInfo, string kodsap)
        //{
        //    ViewBag.LabourReport = "class = active";
        //    GetUserID = GetIdentity.ID(User.Identity.Name);
        //    GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
        //    Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
        //    db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
        //    List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
        //    List<SelectListItem> fld_LadangID = new List<SelectListItem>();

        //    long ID = 1;
        //    long ID2 = 1;





        //    ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
        //    ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

        //    int? GetWilayahID = 0;

        //    if (WilayahID == 0 && LadangID == 0)
        //    {
        //        var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
        //        fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
        //        fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
        //        var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

        //        if (WilayahIDList != null)
        //        {
        //            GetWilayahID = WilayahIDList;
        //        }
        //        else
        //        {
        //            GetWilayahID = GetTopWilayahID.fld_ID;
        //        }

        //        fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
        //        fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
        //    }
        //    else
        //    {
        //        var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_ID == WilayahID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
        //        fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();

        //        var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();
        //        if (WilayahIDList != null)
        //        {
        //            GetWilayahID = WilayahIDList;
        //        }
        //        else
        //        {
        //            GetWilayahID = GetTopWilayahID.fld_ID;
        //        }

        //        fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == WilayahID && x.fld_ID == LadangID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
        //    }
        //    List<SelectListItem> PilihanSts = new List<SelectListItem>();
        //    PilihanSts = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
        //    PilihanSts.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
        //    List<SelectListItem> supplier = new List<SelectListItem>();
        //    supplier = new SelectList(Masterdb.tbl_Pembekal.OrderBy(o => o.fld_KodPbkl).Select(s => new SelectListItem { Value = s.fld_KodPbkl, Text = s.fld_KodPbkl + " - " + s.fld_NamaPbkl }), "Value", "Text").ToList();
        //    supplier.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
        //    List<SelectListItem> nationality = new List<SelectListItem>();
        //    nationality = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
        //    nationality.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
        //    List<SelectListItem> gender = new List<SelectListItem>();
        //    gender = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "gender" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
        //    gender.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
        //    List<SelectListItem> costctr = new List<SelectListItem>();
        //    costctr = new SelectList(Masterdb.tbl_SAPCCPUP.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_CostCenter).Select(s => new SelectListItem { Value = s.fld_CostCenter, Text = s.fld_CostCenter + " - " + s.fld_CostCenterDesc }), "Value", "Text").ToList();
        //    costctr.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

        //    ViewBag.WilayahIDList = fld_WilayahID;
        //    ViewBag.LadangIDList = fld_LadangID;
        //    ViewBag.Status = PilihanSts;
        //    ViewBag.Supplier = supplier;
        //    ViewBag.Nationality = nationality;
        //    ViewBag.Gender = gender;
        //    ViewBag.CostCenter = costctr;






        //    //  var GetWorkerDetails = dbEstate.tbl_Pkjmast.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID ).ToList();
        //    var LabourInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).ToList();
        //    var CustMod_MasterInfo = new List<CustMod_MasterInfo>();
        //    foreach (var labour in LabourInfo)
        //    {
        //        //  var app = Masterdb.tblPkjmastApp.Where(x => x.fldID == id).FirstOrDefault();
        //        string Host1, Catalog1, UserID1, Pass1 = "";
        //        string purpose2 = "CHECKROLL";
        //        MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
        //        if (labour.fld_WilayahID == 1)
        //        {
        //            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, 1, labour.fld_SyarikatID, labour.fld_NegaraID, purpose2);
        //            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
        //        }
        //        else if (labour.fld_WilayahID == 2)
        //        {
        //            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, 2, labour.fld_SyarikatID, labour.fld_NegaraID, purpose2);
        //            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
        //        }

        //        var CC = dbEstate.tbl_Pkjmast.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == labour.fld_WilayahID && x.fld_LadangID == labour.fld_LadangID && x.fld_Nopkj == labour.fld_WorkerNo).Select(s => s.fld_KodSAPPekerja).FirstOrDefault();
        //        CustMod_MasterInfo.Add(new CustMod_MasterInfo
        //        {
        //            id = ID,
        //            nopkj = labour.fld_WorkerNo,
        //            namepkj = labour.fld_WorkerName,
        //            gender = labour.fld_SexType,
        //            nationality = labour.fld_Nationality,
        //            status = labour.fld_ActiveStatus,
        //            kwsp = labour.fld_PerkesoNo,
        //            tarikhMasuk = labour.fld_ArrivedDT,
        //            passportTamat = labour.fld_PassportEndDT,
        //            PermitTamat = labour.fld_PermitEndDT,
        //            roc = labour.fld_Roc,
        //            agensi = labour.fld_SupplierCode,
        //            fld_NegaraID = labour.fld_NegaraID,
        //            fld_SyarikatID = labour.fld_SyarikatID,
        //            fld_WilayahID = labour.fld_WilayahID,
        //            fld_LadangID = labour.fld_LadangID,
        //            permitNo = labour.fld_PermitNo,
        //            costcenter = CC,

        //        }); ;
        //        ID++;
        //    }
        //    var CustMod_MasterInfo3 = new List<CustMod_MasterInfo>();
        //    var getdatadistincts = CustMod_MasterInfo.Select(s => new { s.costcenter, s.fld_WilayahID, s.fld_LadangID, s.gender, s.agensi, s.nationality, s.status, s.nopkj, s.fld_NegaraID, s.fld_SyarikatID }).Distinct().ToList();
        //    foreach (var getdatadistinct in getdatadistincts)
        //    {
        //        var getname = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.namepkj).FirstOrDefault();
        //        var getkwsp = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.kwsp).FirstOrDefault();
        //        var gettarikh = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.tarikhMasuk).FirstOrDefault();
        //        var getpass = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.passportTamat).FirstOrDefault();
        //        var getper = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.PermitTamat).FirstOrDefault();
        //        var getroc = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.roc).FirstOrDefault();
        //        var getpermitno = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.permitNo).FirstOrDefault();
        //        CustMod_MasterInfo3.Add(new CustMod_MasterInfo
        //        {
        //            id = ID2,
        //            nopkj = getdatadistinct.nopkj,
        //            namepkj = getname,
        //            gender = getdatadistinct.gender,
        //            nationality = getdatadistinct.nationality,
        //            status = getdatadistinct.status,
        //            kwsp = getkwsp,
        //            tarikhMasuk = gettarikh,
        //            passportTamat = getpass,
        //            PermitTamat = getper,
        //            roc = getroc,
        //            agensi = getdatadistinct.agensi,
        //            fld_NegaraID = getdatadistinct.fld_NegaraID,
        //            fld_SyarikatID = getdatadistinct.fld_SyarikatID,
        //            fld_WilayahID = getdatadistinct.fld_WilayahID,
        //            fld_LadangID = getdatadistinct.fld_LadangID,
        //            permitNo = getpermitno,
        //            costcenter = getdatadistinct.costcenter,
        //        }); ;
        //        ID2++;
        //    }

        //    var CustMod_MasterInfo2 = new List<CustMod_MasterInfo>();
        //    CustMod_MasterInfo2 = CustMod_MasterInfo3;

        //    if (WilayahIDList != 0)
        //    {
        //        CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.fld_WilayahID == WilayahIDList).ToList();
        //        ViewBag.Region = Masterdb.tbl_Wilayah
        //         .Where(x => x.fld_ID == WilayahIDList)
        //         .Select(s => s.fld_WlyhName)
        //         .FirstOrDefault();
        //    }
        //    if (LadangIDList != 0)
        //    {
        //        CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.fld_LadangID == LadangIDList).ToList();

        //        ViewBag.Estate = Masterdb.tbl_Ladang
        //              .Where(x => x.fld_ID == LadangIDList)
        //              .Select(s => s.fld_LdgName)
        //                .FirstOrDefault();

        //    }
        //    if (Status != "0")
        //    {
        //        CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.status == Status).ToList();
        //    }
        //    if (CostCenter != "0")
        //    {
        //        CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.costcenter == CostCenter).ToList();
        //    }
        //    if (Supplier != "0")
        //    {
        //        CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.agensi == Supplier).ToList();
        //    }
        //    if (Gender != "0")
        //    {
        //        CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.gender == Gender).ToList();
        //    }
        //    if (Nationality != "0")
        //    {
        //        CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.nationality == Nationality).ToList();
        //    }


        //    if (WilayahIDList == 0)
        //    {
        //        ViewBag.Region = "all";

        //    }
        //    if (LadangIDList == 0)
        //    {

        //        ViewBag.Estate = "all";
        //    }




        //    return View(CustMod_MasterInfo2);

        //}

        //Added by Shazana on 16/11
        public ActionResult LabourMasterData(int? NegaraID, int? SyarikatID, int? WilayahID, int? WilayahIDList, int? LadangIDList, string NoPkj, string Supplier, string Nationality, string CostCenter, string Gender, string Status)
        {
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();

            long ID = 1;
            long ID2 = 1;

            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            List<SelectListItem> PilihanSts = new List<SelectListItem>();
            PilihanSts = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            PilihanSts.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            List<SelectListItem> supplier = new List<SelectListItem>();
            supplier = new SelectList(Masterdb.tbl_Pembekal.OrderBy(o => o.fld_KodPbkl).Select(s => new SelectListItem { Value = s.fld_KodPbkl, Text = s.fld_KodPbkl + " - " + s.fld_NamaPbkl }), "Value", "Text").ToList();
            supplier.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            List<SelectListItem> nationality = new List<SelectListItem>();
            nationality = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            nationality.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            List<SelectListItem> gender = new List<SelectListItem>();
            gender = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "gender" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            gender.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            List<SelectListItem> costctr = new List<SelectListItem>();
            costctr = new SelectList(Masterdb.tbl_SAPCCPUP.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_CostCenter).Select(s => new SelectListItem { Value = s.fld_CostCenter, Text = s.fld_CostCenter + " - " + s.fld_CostCenterDesc }), "Value", "Text").ToList();
            costctr.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.Status = PilihanSts;
            ViewBag.Supplier = supplier;
            ViewBag.Nationality = nationality;
            ViewBag.Gender = gender;
            ViewBag.CostCenter = costctr;

            //  var GetWorkerDetails = dbEstate.tbl_Pkjmast.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID ).ToList();
            var LabourInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).ToList();
            var CustMod_MasterInfo = new List<CustMod_MasterInfo>();
            foreach (var labour in LabourInfo)
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
                CustMod_MasterInfo.Add(new CustMod_MasterInfo
                {
                    id = ID,
                    nopkj = labour.fld_WorkerNo,
                    namepkj = labour.fld_WorkerName,
                    gender = labour.fld_SexType,
                    nationality = labour.fld_Nationality,
                    status = labour.fld_ActiveStatus,
                    kwsp = labour.fld_PerkesoNo,
                    tarikhMasuk = labour.fld_ArrivedDT,
                    passportTamat = labour.fld_PassportEndDT,
                    PermitTamat = labour.fld_PermitEndDT,
                    roc = labour.fld_Roc,
                    agensi = labour.fld_SupplierCode,
                    fld_NegaraID = labour.fld_NegaraID,
                    fld_SyarikatID = labour.fld_SyarikatID,
                    fld_WilayahID = labour.fld_WilayahID,
                    fld_LadangID = labour.fld_LadangID,
                    permitNo = labour.fld_PermitNo,
                    costcenter = CC,

                }); ;
                ID++;
            }
            var CustMod_MasterInfo3 = new List<CustMod_MasterInfo>();
            var getdatadistincts = CustMod_MasterInfo.Select(s => new { s.costcenter, s.fld_WilayahID, s.fld_LadangID, s.gender, s.agensi, s.nationality, s.status, s.nopkj, s.fld_NegaraID, s.fld_SyarikatID }).Distinct().ToList();
            foreach (var getdatadistinct in getdatadistincts)
            {
                var getname = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.namepkj).FirstOrDefault();
                var getkwsp = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.kwsp).FirstOrDefault();
                var gettarikh = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.tarikhMasuk).FirstOrDefault();
                var getpass = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.passportTamat).FirstOrDefault();
                var getper = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.PermitTamat).FirstOrDefault();
                var getroc = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.roc).FirstOrDefault();
                var getpermitno = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.permitNo).FirstOrDefault();
                CustMod_MasterInfo3.Add(new CustMod_MasterInfo
                {
                    id = ID2,
                    nopkj = getdatadistinct.nopkj,
                    namepkj = getname,
                    gender = getdatadistinct.gender,
                    nationality = getdatadistinct.nationality,
                    status = getdatadistinct.status,
                    kwsp = getkwsp,
                    tarikhMasuk = gettarikh,
                    passportTamat = getpass,
                    PermitTamat = getper,
                    roc = getroc,
                    agensi = getdatadistinct.agensi,
                    fld_NegaraID = getdatadistinct.fld_NegaraID,
                    fld_SyarikatID = getdatadistinct.fld_SyarikatID,
                    fld_WilayahID = getdatadistinct.fld_WilayahID,
                    fld_LadangID = getdatadistinct.fld_LadangID,
                    permitNo = getpermitno,
                    costcenter = getdatadistinct.costcenter,
                }); ;
                ID2++;
            }

            var CustMod_MasterInfo2 = new List<CustMod_MasterInfo>();
            CustMod_MasterInfo2 = CustMod_MasterInfo3;

            if (WilayahIDList != 0)
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.fld_WilayahID == WilayahIDList).ToList();
                ViewBag.Region = Masterdb.tbl_Wilayah
                 .Where(x => x.fld_ID == WilayahIDList)
                 .Select(s => s.fld_WlyhName)
                 .FirstOrDefault();
            }
            if (LadangIDList != 0)
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.fld_LadangID == LadangIDList).ToList();

                ViewBag.Estate = Masterdb.tbl_Ladang
                      .Where(x => x.fld_ID == LadangIDList)
                      .Select(s => s.fld_LdgName)
                        .FirstOrDefault();

            }
            if (Status != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.status == Status).ToList();
            }
            if (CostCenter != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.costcenter == CostCenter).ToList();
            }
            if (Supplier != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.agensi == Supplier).ToList();
            }
            if (Gender != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.gender == Gender).ToList();
            }
            if (Nationality != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.nationality == Nationality).ToList();
            }


            if (WilayahIDList == 0)
            {
                ViewBag.Region = "all";

            }
            if (LadangIDList == 0)
            {

                ViewBag.Estate = "all";
            }

            return View(CustMod_MasterInfo2.OrderBy(x => x.nopkj));
        }

        public ActionResult _LabourMasterData(int? NegaraID, int? SyarikatID, int? WilayahID, int? WilayahIDList, int? LadangIDList, string Supplier, string Nationality, string CostCenter, string Gender, string Status, string print)
        {
            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();

            long ID = 1;
            long ID2 = 1;
            ViewBag.Print = print;
            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();

            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            List<SelectListItem> PilihanSts = new List<SelectListItem>();
            PilihanSts = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            PilihanSts.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            List<SelectListItem> supplier = new List<SelectListItem>();
            supplier = new SelectList(Masterdb.tbl_Pembekal.OrderBy(o => o.fld_KodPbkl).Select(s => new SelectListItem { Value = s.fld_KodPbkl, Text = s.fld_KodPbkl + " - " + s.fld_NamaPbkl }), "Value", "Text").ToList();
            supplier.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            List<SelectListItem> nationality = new List<SelectListItem>();
            nationality = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            nationality.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            List<SelectListItem> gender = new List<SelectListItem>();
            gender = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "gender" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            gender.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            List<SelectListItem> costctr = new List<SelectListItem>();
            costctr = new SelectList(Masterdb.tbl_SAPCCPUP.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_CostCenter).Select(s => new SelectListItem { Value = s.fld_CostCenter, Text = s.fld_CostCenter + " - " + s.fld_CostCenterDesc }), "Value", "Text").ToList();
            costctr.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            ViewBag.Status = PilihanSts;
            ViewBag.Supplier = supplier;
            ViewBag.Nationality = nationality;
            ViewBag.Gender = gender;
            ViewBag.CostCenter = costctr;

            //  var GetWorkerDetails = dbEstate.tbl_Pkjmast.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID ).ToList();
            var LabourInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).ToList();
            var CustMod_MasterInfo = new List<CustMod_MasterInfo>();
            foreach (var labour in LabourInfo)
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
                CustMod_MasterInfo.Add(new CustMod_MasterInfo
                {
                    id = ID,
                    nopkj = labour.fld_WorkerNo,
                    namepkj = labour.fld_WorkerName,
                    gender = labour.fld_SexType,
                    nationality = labour.fld_Nationality,
                    status = labour.fld_ActiveStatus,
                    kwsp = labour.fld_PerkesoNo,
                    tarikhMasuk = labour.fld_ArrivedDT,
                    passportTamat = labour.fld_PassportEndDT,
                    PermitTamat = labour.fld_PermitEndDT,
                    roc = labour.fld_Roc,
                    agensi = labour.fld_SupplierCode,
                    fld_NegaraID = labour.fld_NegaraID,
                    fld_SyarikatID = labour.fld_SyarikatID,
                    fld_WilayahID = labour.fld_WilayahID,
                    fld_LadangID = labour.fld_LadangID,
                    permitNo = labour.fld_PermitNo,
                    costcenter = CC,

                }); ;
                ID++;
            }
            var CustMod_MasterInfo3 = new List<CustMod_MasterInfo>();
            var getdatadistincts = CustMod_MasterInfo.Select(s => new { s.costcenter, s.fld_WilayahID, s.fld_LadangID, s.gender, s.agensi, s.nationality, s.status, s.nopkj, s.fld_NegaraID, s.fld_SyarikatID }).Distinct().ToList();
            foreach (var getdatadistinct in getdatadistincts)
            {
                var getname = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.namepkj).FirstOrDefault();
                var getkwsp = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.kwsp).FirstOrDefault();
                var gettarikh = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.tarikhMasuk).FirstOrDefault();
                var getpass = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.passportTamat).FirstOrDefault();
                var getper = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.PermitTamat).FirstOrDefault();
                var getroc = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.roc).FirstOrDefault();
                var getpermitno = CustMod_MasterInfo.Where(x => x.nopkj == getdatadistinct.nopkj).Select(s => s.permitNo).FirstOrDefault();
                CustMod_MasterInfo3.Add(new CustMod_MasterInfo
                {
                    id = ID2,
                    nopkj = getdatadistinct.nopkj,
                    namepkj = getname,
                    gender = getdatadistinct.gender,
                    nationality = getdatadistinct.nationality,
                    status = getdatadistinct.status,
                    kwsp = getkwsp,
                    tarikhMasuk = gettarikh,
                    passportTamat = getpass,
                    PermitTamat = getper,
                    roc = getroc,
                    agensi = getdatadistinct.agensi,
                    fld_NegaraID = getdatadistinct.fld_NegaraID,
                    fld_SyarikatID = getdatadistinct.fld_SyarikatID,
                    fld_WilayahID = getdatadistinct.fld_WilayahID,
                    fld_LadangID = getdatadistinct.fld_LadangID,
                    permitNo = getpermitno,
                    costcenter = getdatadistinct.costcenter,
                }); ;
                ID2++;
            }

            var CustMod_MasterInfo2 = new List<CustMod_MasterInfo>();
            CustMod_MasterInfo2 = CustMod_MasterInfo3;

            if (WilayahIDList != 0)
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.fld_WilayahID == WilayahIDList).ToList();
                ViewBag.Region = Masterdb.tbl_Wilayah
                 .Where(x => x.fld_ID == WilayahIDList)
                 .Select(s => s.fld_WlyhName)
                 .FirstOrDefault();
            }
            if (LadangIDList != 0)
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.fld_LadangID == LadangIDList).ToList();

                ViewBag.Estate = Masterdb.tbl_Ladang
                      .Where(x => x.fld_ID == LadangIDList)
                      .Select(s => s.fld_LdgName)
                        .FirstOrDefault();

            }
            if (Status != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.status == Status).ToList();
            }
            if (CostCenter != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.costcenter == CostCenter).ToList();
            }
            if (Supplier != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.agensi == Supplier).ToList();
            }
            if (Gender != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.gender == Gender).ToList();
            }
            if (Nationality != "0")
            {
                CustMod_MasterInfo2 = CustMod_MasterInfo2.Where(x => x.nationality == Nationality).ToList();
            }


            if (WilayahIDList == 0)
            {
                ViewBag.Region = "all";

            }
            if (LadangIDList == 0)
            {

                ViewBag.Estate = "all";
            }

            //Commented by Shazana on 16/11
            //return View(CustMod_MasterInfo2);

            //Added by Shazana on 16/11
            return View(CustMod_MasterInfo2.OrderBy(x => x.nopkj));
        }

        public ActionResult PrintWorkerPdfMasterData(string NegaraID, string SyarikatID, string WilayahIDList, string LadangIDList, string Supplier, string Nationality, string CostCenter, string Gender, string Status, int id, string genid)
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
                report = new ActionAsPdf("_LabourMasterData", new { NegaraID, SyarikatID, WilayahIDList, LadangIDList, Supplier, Nationality, CostCenter, Gender, Status, print })
                {
                    FormsAuthenticationCookieName = FormsAuthentication.FormsCookieName,
                    Cookies = cookies
                };
            }
            else
            {
                report = new ActionAsPdf("PDFInvalid");
            }

            Response.AppendHeader("Content-Disposition", "inline; filename=" + "LabourMasterData" + ".pdf");

            return report;
        }

        public ActionResult LabourRumusanKeperluanPekerja(int? WilayahIDList, int? LadangIDList, string CurrDate)
        {

            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year;
            int RangeYear = DT.Year + 1;
            DateTime date3 = DT.Date;
            string testdate = CurrDate;
            if (testdate != null)
            {
                date3 = DateTime.ParseExact(testdate, @"dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }

            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();
            ViewBag.Tahun = Year;

            //added by nana on 21/7/2020
            ViewBag.Tarikh = testdate;

            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            //ViewBag.YearList = YearList1;
            //ViewBag.MonthList = MonthList1;
            //ViewBag.Year = YearList;
            //ViewBag.Month = MonthList;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();
            List<vw_NSWL> GetNSWLList = new List<vw_NSWL>();

            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = "SEMUA";
                    ViewBag.Estate = "SEMUA";
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = "SEMUA";
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = "ALL";
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }

            DateTime LastMonth = DT.AddMonths(-1);
            var GetEstateSizeList = GetLadang.GetEstateAreaByLevel(SyarikatID, NegaraID);
            var RatioSetting = Masterdb.tblOptionConfigsWebs.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldOptConfFlag1 == "ratioperha").Select(s => s.fldOptConfValue).FirstOrDefault();
            decimal RatioPerHa = decimal.Parse(RatioSetting);
            List<CustMod_RumKeperluanPekerjaRpt> CustMod_RumKeperluanPekerjaRpt = new List<CustMod_RumKeperluanPekerjaRpt>();
            int ID = 1;
            if (tbl_LbrDataInfo.Count > 0)
            {
                foreach (var NSWL in GetNSWLList)
                {
                    var GetEstateSize = GetEstateSizeList.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID).FirstOrDefault();
                    Decimal Need = GetEstateSize.fld_SizeArea / RatioPerHa;
                    int NeedInt = Decimal.ToInt32(Need);

                    Decimal Keperluan = GetEstateSize.fld_SizeArea / 11;
                    int KeperluanInt = Decimal.ToInt32(Keperluan);

                    var Indon = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "IN").Count();
                    var Bangla = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "BA").Count();
                    var India = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "ID").Count();
                    var Nepal = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "NE").Count();
                    var TotalTKA = Indon + Bangla + India + Nepal;
                    var POL = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "MA").Count();
                    var Agihan = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_WorkingStartDT <= date3).Count();
                    var TotalTK = TotalTKA + POL;
                    Decimal TotalTKDec = TotalTKA + POL;
                    var KekuranganSemasa =
                    -TotalTK;
                    Decimal Kedudukan = 0;
                    Decimal PeratusKekuranganSemasa = 0;
                    int PeratusKekuranganSemasaInt = 0;
                    if (KeperluanInt == 0)
                    {
                        Kedudukan = 0;
                        PeratusKekuranganSemasa = 0;
                        PeratusKekuranganSemasaInt = Decimal.ToInt32(PeratusKekuranganSemasa);
                    }
                    else
                    {
                        Kedudukan = (TotalTKDec / Keperluan) * 100;
                        PeratusKekuranganSemasa = (KekuranganSemasa / KeperluanInt) * 100;
                        PeratusKekuranganSemasaInt = Decimal.ToInt32(PeratusKekuranganSemasa);
                    }


                    CustMod_RumKeperluanPekerjaRpt.Add(new CustMod_RumKeperluanPekerjaRpt()
                    {
                        fld_ID = ID,
                        fld_NegaraID = NSWL.fld_NegaraID,
                        fld_SyarikatID = NSWL.fld_SyarikatID,
                        fld_WilayahID = NSWL.fld_WilayahID,
                        fld_LadangID = NSWL.fld_LadangID,
                        fld_EstateName = NSWL.fld_NamaLadang,
                        fld_EstateSize = GetEstateSize.fld_SizeArea,
                        fld_Need = NeedInt,
                        fld_RatioPerHa = RatioSetting,
                        fld_Keperluan = KeperluanInt,
                        fld_Indon = Indon,
                        fld_Bangla = Bangla,
                        fld_India = India,
                        fld_Nepal = Nepal,
                        fld_TotalTKA = TotalTKA,
                        fld_POL = POL,
                        fld_TotalTK = TotalTK,
                        fld_Kedudukan = Kedudukan,
                        fld_KekuranganSemasa = KekuranganSemasa,
                        fld_PeratusKekuranganSemasa = PeratusKekuranganSemasaInt,
                        fld_Agihan = Agihan
                    });
                }
            }
            var CustMod_RumKeperluanPekerjaRptSorting = CustMod_RumKeperluanPekerjaRpt.OrderBy(o => o.fld_EstateName).ToList();
            return View(CustMod_RumKeperluanPekerjaRptSorting);
        }

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
                    geterror.testlog("Try if : " + User.Identity.Name, "Try if : " + CookiesValue, "Try if : " + getcookiesval);
                }
                geterror.testlog("Try no if : " + User.Identity.Name, "Try no if : " + CookiesValue, "Try no if : " + getcookiesval);
            }
            catch
            {
                HttpCookie authoCookies = new HttpCookie(FormsAuthentication.FormsCookieName, getcookiesval);
                Response.SetCookie(authoCookies);
                geterror.testlog("Catch : " + User.Identity.Name, "Catch : " + CookiesValue, "Catch : " + getcookiesval);
            }
            geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
        }

        //   public bool CheckGenIdentity(int id, string genid, int? userid, string username, out string CookiesValue)
        //   {
        //       bool result = false;
        //       int? NegaraID, SyarikatID, WilayahID, LadangID = 0;
        //       string host, catalog, user, pass = "";
        //       GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, userid, username);
        //       Connection.GetConnection(out host, out catalog, out user, out pass, WilayahID.Value, SyarikatID.Value,
        //NegaraID.Value);
        //       CookiesValue = "";

        //       MVC_SYSTEM_Models dbr = MVC_SYSTEM_Models.ConnectToSqlServer(host, catalog, user, pass);

        //       try
        //       {
        //           Guid genidC = Guid.Parse(genid);
        //           var CheckIdentity = dbr.tbl_PdfGen.Where(x => x.fld_ID == genidC && x.fld_UserID == id).FirstOrDefault();

        //           if (CheckIdentity == null)
        //           {
        //               result = false;
        //           }
        //           else
        //           {
        //               result = true;
        //               CookiesValue = CheckIdentity.fld_CookiesVal;
        //           }
        //       }
        //       catch (Exception ex)
        //       {
        //           result = false;
        //       }
        //       return result;
        //   }

        public ActionResult PrintWorkerPdf(int? RadioGroup, string WilayahIDList, string LadangIDList, string MonthList, string YearList, int id, string genid)
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
                report = new ActionAsPdf("_LabourCostPrmtPsprtSurvey", new { WilayahIDList, LadangIDList, MonthList, YearList, print })
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

        //Added by Shazana on 17/8

        public ActionResult _LabourRumusanKeperluanPekerja(int? WilayahIDList, int? LadangIDList, string CurrDate, string print)
        {

            ViewBag.LabourReport = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int Month = DT.AddMonths(-1).Month;
            int Year = DT.Year;
            int RangeYear = DT.Year + 1;
            DateTime date3 = DT.Date;
            string testdate = CurrDate;
            ViewBag.Print = print;
            if (testdate != null)
            {
                date3 = DateTime.ParseExact(testdate, @"dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }

            ViewBag.NamaSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NamaSyarikat).FirstOrDefault();
            ViewBag.NoSyarikat = Masterdb.tbl_Syarikat.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).Select(s => s.fld_NoSyarikat).FirstOrDefault();
            ViewBag.Tahun = Year;

            //added by nana on 21/7/2020
            ViewBag.Tarikh = testdate;

            int? GetWilayahID = 0;

            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
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
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;
            //ViewBag.YearList = YearList1;
            //ViewBag.MonthList = MonthList1;
            //ViewBag.Year = YearList;
            //ViewBag.Month = MonthList;

            List<tbl_LbrDataInfo> tbl_LbrDataInfo = new List<tbl_LbrDataInfo>();
            List<vw_NSWL> GetNSWLList = new List<vw_NSWL>();

            if (WilayahID == 0 && LadangID == 0)
            {
                if (WilayahIDList == 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = "SEMUA";
                    ViewBag.Estate = "SEMUA";
                }
                else if (WilayahIDList != 0 && LadangIDList == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = "SEMUA";
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }
            else
            {
                if (LadangID == 0)
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = "ALL";
                }
                else
                {
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).OrderBy(o => o.fld_WorkerName).ToList();
                    GetNSWLList = Masterdb.vw_NSWL.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Deleted_L == false).OrderBy(o => o.fld_WilayahID).ToList();
                    ViewBag.Region = Masterdb.tbl_Wilayah
                                 .Where(x => x.fld_ID == WilayahIDList)
                                 .Select(s => s.fld_WlyhName)
                                 .FirstOrDefault();
                    ViewBag.Estate = Masterdb.tbl_Ladang
                       .Where(x => x.fld_ID == LadangIDList)
                       .Select(s => s.fld_LdgName)
                         .FirstOrDefault();
                }
            }

            DateTime LastMonth = DT.AddMonths(-1);
            var GetEstateSizeList = GetLadang.GetEstateAreaByLevel(SyarikatID, NegaraID);
            var RatioSetting = Masterdb.tblOptionConfigsWebs.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldOptConfFlag1 == "ratioperha").Select(s => s.fldOptConfValue).FirstOrDefault();
            decimal RatioPerHa = decimal.Parse(RatioSetting);
            List<CustMod_RumKeperluanPekerjaRpt> CustMod_RumKeperluanPekerjaRpt = new List<CustMod_RumKeperluanPekerjaRpt>();
            int ID = 1;
            if (tbl_LbrDataInfo.Count > 0)
            {
                foreach (var NSWL in GetNSWLList)
                {
                    var GetEstateSize = GetEstateSizeList.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID).FirstOrDefault();
                    Decimal Need = GetEstateSize.fld_SizeArea / RatioPerHa;
                    int NeedInt = Decimal.ToInt32(Need);

                    Decimal Keperluan = GetEstateSize.fld_SizeArea / 11;
                    int KeperluanInt = Decimal.ToInt32(Keperluan);

                    var Indon = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "IN").Count();
                    var Bangla = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "BA").Count();
                    var India = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "ID").Count();
                    var Nepal = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "NE").Count();
                    var TotalTKA = Indon + Bangla + India + Nepal;
                    var POL = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_Nationality == "MA").Count();
                    var Agihan = tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NSWL.fld_NegaraID && x.fld_SyarikatID == NSWL.fld_SyarikatID && x.fld_WilayahID == NSWL.fld_WilayahID && x.fld_LadangID == NSWL.fld_LadangID && x.fld_ActiveStatus == "1" && x.fld_WorkingStartDT <= date3).Count();
                    var TotalTK = TotalTKA + POL;
                    Decimal TotalTKDec = TotalTKA + POL;
                    var KekuranganSemasa =
                    -TotalTK;
                    Decimal Kedudukan = 0;
                    Decimal PeratusKekuranganSemasa = 0;
                    int PeratusKekuranganSemasaInt = 0;
                    if (KeperluanInt == 0)
                    {
                        Kedudukan = 0;
                        PeratusKekuranganSemasa = 0;
                        PeratusKekuranganSemasaInt = Decimal.ToInt32(PeratusKekuranganSemasa);
                    }
                    else
                    {
                        Kedudukan = (TotalTKDec / Keperluan) * 100;
                        PeratusKekuranganSemasa = (KekuranganSemasa / KeperluanInt) * 100;
                        PeratusKekuranganSemasaInt = Decimal.ToInt32(PeratusKekuranganSemasa);
                    }


                    CustMod_RumKeperluanPekerjaRpt.Add(new CustMod_RumKeperluanPekerjaRpt()
                    {
                        fld_ID = ID,
                        fld_NegaraID = NSWL.fld_NegaraID,
                        fld_SyarikatID = NSWL.fld_SyarikatID,
                        fld_WilayahID = NSWL.fld_WilayahID,
                        fld_LadangID = NSWL.fld_LadangID,
                        fld_EstateName = NSWL.fld_NamaLadang,
                        fld_EstateSize = GetEstateSize.fld_SizeArea,
                        fld_Need = NeedInt,
                        fld_RatioPerHa = RatioSetting,
                        fld_Keperluan = KeperluanInt,
                        fld_Indon = Indon,
                        fld_Bangla = Bangla,
                        fld_India = India,
                        fld_Nepal = Nepal,
                        fld_TotalTKA = TotalTKA,
                        fld_POL = POL,
                        fld_TotalTK = TotalTK,
                        fld_Kedudukan = Kedudukan,
                        fld_KekuranganSemasa = KekuranganSemasa,
                        fld_PeratusKekuranganSemasa = PeratusKekuranganSemasaInt,
                        fld_Agihan = Agihan
                    });
                }
            }
            var CustMod_RumKeperluanPekerjaRptSorting = CustMod_RumKeperluanPekerjaRpt.OrderBy(o => o.fld_EstateName).ToList();
            return View(CustMod_RumKeperluanPekerjaRptSorting);
        }

        public ActionResult PrintWorkerPdfRumusanKeperluanPekerja(int? RadioGroup, string WilayahIDList, string LadangIDList, string CurrDate, int id, string genid)
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
                report = new ActionAsPdf("_LabourRumusanKeperluanPekerja", new { WilayahIDList, LadangIDList, CurrDate, print })
                {
                    FormsAuthenticationCookieName = FormsAuthentication.FormsCookieName,
                    Cookies = cookies
                };
            }
            else
            {
                report = new ActionAsPdf("PDFInvalid");
            }

            //Added by Shazana on 18/11
            Response.AppendHeader("Content-Disposition", "inline; filename=" + "LabourRumusanKeperluanPekerja" + ".pdf");

            return report;
        }
    public bool CheckGenIdentity(int id, string genid, int? userid, string username, out string CookiesValue)
        {
            bool result = false;
            int? NegaraID, SyarikatID, WilayahID, LadangID = 0;
            string host, catalog, user, pass = "";
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, userid, username);

            Connection.GetConnection(out host, out catalog, out user, out pass, WilayahID.Value, SyarikatID.Value,
                NegaraID.Value,"LABOUR");

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
    

    
        //public ActionResult PrintWorkerPdf(int? RadioGroup, string WilayahIDList, string LadangIDList, string MonthList, string YearList, int id, string genid)
        //{

        //    //var param = '/?RadioGroup=' + RadioGroup + '&WilayahIDList=' + WilayahIDList + '&LadangIDList=' + LadangIDList + '&MonthList=' + MonthList + '&YearList=' + YearList;


        //    int? getuserid = 0;
        //    string getusername = "";
        //    string getcookiesval = "";
        //    bool checkidentity = false;
        //    //geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
        //    var getuser = Masterdb.tblUsers.Where(u => u.fldUserID == id && u.fldDeleted == false).SingleOrDefault();
        //    if (getuser != null)
        //    {
        //        getuserid = GetIdentity.ID(getuser.fldUserName);
        //        getusername = getuser.fldUserName;
        //    }

        //    checkidentity = CheckGenIdentity(id, genid, getuserid, getusername, out getcookiesval);

        //    ActionAsPdf report = new ActionAsPdf("");

        //    if (checkidentity)
        //    {
        //        getBackAuth(getcookiesval);
        //        var cookies = Request.Cookies.AllKeys.ToDictionary(k => k, k => Request.Cookies[k].Value);
        //        //geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
        //        string print = "Yes";
        //        report = new ActionAsPdf("_WorkerRptSearch", new { RadioGroup, WilayahIDList, LadangIDList, MonthList, YearList,print })
        //        {
        //            FormsAuthenticationCookieName = FormsAuthentication.FormsCookieName,
        //            Cookies = cookies
        //        };
        //    }
        //    else
        //    {
        //        report = new ActionAsPdf("PDFInvalid");
        //    }

        //    return report;
        //}

    }
}