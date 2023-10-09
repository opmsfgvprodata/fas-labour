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


//Added by Shazana on 10/8
using Rotativa;
using System.Web.Security;
using MVC_SYSTEM.log;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    
    public class LabourFomemaController : Controller
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
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;
        errorlog geterror = new errorlog();
        public async Task<ActionResult> Index(int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, int? MonthList, int? MonthList2, int? YearList, string print)
        {

            ViewBag.LabourFomema = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int? GetWilayahID = 0;

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

           
            if (WilayahID == 0 && LadangID == 0)
            {
                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PermitEndDT.Value.Month >= MonthList && x.fld_PermitEndDT.Value.Month <= MonthList2) && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_PermitEndDT, o.fld_WorkerName }).ToListAsync();
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PermitEndDT.Value.Month >= MonthList && x.fld_PermitEndDT.Value.Month <= MonthList2) && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_PermitEndDT, o.fld_WorkerName }).ToListAsync();
                    }
                    else
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT.Value.Month >= MonthList && x.fld_PermitEndDT.Value.Month <= MonthList2) && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_PermitEndDT, o.fld_WorkerName }).ToListAsync();
                    }
                }
                else
                {
                    LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_PermitEndDT, o.fld_WorkerName }).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(FreeText))
                {
                    LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT.Value.Month >= MonthList && x.fld_PermitEndDT.Value.Month <= MonthList2) && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_PermitEndDT, o.fld_WorkerName }).ToListAsync();
                }
                else
                {
                    LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_PermitEndDT, o.fld_WorkerName }).ToListAsync();
                }
            }

            return View(LbrDataInfo);
        }
        public ActionResult _LabourFomema(int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, int? MonthList, int? MonthList2, int? YearList, string print)
        {
            ViewBag.LabourFomema = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int? GetWilayahID = 0;

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

            if (WilayahIDList == null && LadangIDList == null)
            {
                ViewBag.Message = GlobalResEstate.msgChooseRegionEstateMonthYear;

            }

            if (WilayahID == 0 && LadangID == 0)
            {
                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {
                      var LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_PermitEndDT.Value.Month >= MonthList && x.fld_PermitEndDT.Value.Month <= MonthList2) && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderByDescending(o => new { o.fld_ArrivedDT });
                      return View(LbrDataInfo);
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                       var LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && (x.fld_PermitEndDT.Value.Month >= MonthList && x.fld_PermitEndDT.Value.Month <= MonthList2) && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderByDescending(o => new { o.fld_ArrivedDT });
                        return View(LbrDataInfo);
                    }
                    else
                    {
                        var LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT.Value.Month >= MonthList && x.fld_PermitEndDT.Value.Month <= MonthList2) && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderByDescending(o => new { o.fld_ArrivedDT });
                        return View(LbrDataInfo);
                    }
                }
                else
                {
                    var LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderByDescending(o => new { o.fld_ArrivedDT });
                    return View(LbrDataInfo);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(FreeText))
                {
                    var LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && (x.fld_PermitEndDT.Value.Month >= MonthList && x.fld_PermitEndDT.Value.Month <= MonthList2) && x.fld_PermitEndDT.Value.Year == YearList && x.fld_Nationality != "MA").OrderByDescending(o => new { o.fld_ArrivedDT });
                    return View(LbrDataInfo);
                }
                else
                {
                    var LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderByDescending(o => new { o.fld_ArrivedDT });
                    return View(LbrDataInfo);
                }
            }

        }

        public ActionResult PrintWorkerPdf(int? RadioGroup, string WilayahIDList, string LadangIDList,string StatusList,string FreeText, int? MonthList, int? MonthList2, int? YearList, int id, string genid, string Print)
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
            checkidentity = CheckGenIdentity(id, genid, getuserid, getusername, out getcookiesval);

            ActionAsPdf report = new ActionAsPdf("");
           
            if (checkidentity)
            {
                getBackAuth(getcookiesval);
                var cookies = Request.Cookies.AllKeys.ToDictionary(k => k, k => Request.Cookies[k].Value);
                string print = "Yes";
                report = new ActionAsPdf("_LabourFomema", new { WilayahIDList, LadangIDList, StatusList, FreeText, MonthList, MonthList2, YearList, print })
                {
                     
                    FormsAuthenticationCookieName = FormsAuthentication.FormsCookieName,
                    Cookies = cookies,
                    // FileName = FileName
                    
            };
                    
            }
            else
            {
                report = new ActionAsPdf("PDFInvalid");
            }



            Response.AppendHeader("Content-Disposition", "inline; filename=" + "LabourFomema" + ".pdf");
           // return new ActionAsPdf("_LabourFomema", new { WilayahIDList, LadangIDList, StatusList, FreeText, MonthList, MonthList2, YearList, Print });

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
                    //geterror.testlog("Try if : " + User.Identity.Name, "Try if : " + CookiesValue, "Try if : " + getcookiesval);
                }
               // geterror.testlog("Try no if : " + User.Identity.Name, "Try no if : " + CookiesValue, "Try no if : " + getcookiesval);
            }
            catch
            {
                HttpCookie authoCookies = new HttpCookie(FormsAuthentication.FormsCookieName, getcookiesval);
                Response.SetCookie(authoCookies);
            //    geterror.testlog("Catch : " + User.Identity.Name, "Catch : " + CookiesValue, "Catch : " + getcookiesval);
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

        public async Task<ActionResult> FomemaRegister(Guid? id)
        {
            ViewBag.LabourFomema = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_FormemaTypeCode = new List<SelectListItem>();
            List<SelectListItem> fld_FomemaResult = new List<SelectListItem>();
            List<SelectListItem> fld_AcknoTypeCode = new List<SelectListItem>();
            List<SelectListItem> fld_BizSectorCode = new List<SelectListItem>();
            List<SelectListItem> fld_ClinicID = new List<SelectListItem>();
            List<SelectListItem> fld_StateCode = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "fomematype", "acknwldmnttype", "sector", "negeri", "result" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            fld_FormemaTypeCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "fomematype" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            fld_FomemaResult = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "result" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            fld_AcknoTypeCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "acknwldmnttype" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            fld_BizSectorCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "sector" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            fld_StateCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            var GetOneState = GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Take(1).FirstOrDefault();
            var GetClinic = Masterdb.tbl_Clinic.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_StateCode == GetOneState.fldOptConfValue && x.fld_Deleted == false).ToList();
            string DoctorName = "";
            if (GetClinic.Count > 0)
            {
                fld_ClinicID = new SelectList(GetClinic.OrderBy(o => o.fld_ClinicName).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_ClinicName + " - " + s.fld_District }), "Value", "Text").ToList();
                DoctorName = GetClinic.Select(s => s.fld_DoctorName).Take(1).FirstOrDefault();
            }
            else
            {
                fld_ClinicID.Insert(0, (new SelectListItem { Text = "No Data", Value = "" }));
            }

            var CheckExistingFailFmm = db.vw_LbrFememaRslt.Where(x => x.fld_LbrRefID == id && x.fld_FomemaResult == false && x.fld_Deleted == false).Count();
            if (CheckExistingFailFmm > 0)
            {
                ViewBag.DisableAll = "Yes";
            }
            
            ViewBag.fld_FormemaTypeCode = fld_FormemaTypeCode;
            ViewBag.fld_FomemaResult = fld_FomemaResult;
            ViewBag.fld_AcknoTypeCode = fld_AcknoTypeCode;
            ViewBag.fld_BizSectorCode = fld_BizSectorCode;
            ViewBag.fld_ClinicID = fld_ClinicID;
            ViewBag.fld_StateCode = fld_StateCode;
            ViewBag.DoctorName = DoctorName;

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FomemaRegister(tbl_LbrFomemaRslt tbl_LbrFomemaRslt)
        {
            ViewBag.LabourFomema = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            var GetExistingFomema = db.tbl_LbrFomemaRslt.Where(x => x.fld_LbrRefID == tbl_LbrFomemaRslt.fld_LbrRefID && x.fld_FormemaTypeCode == tbl_LbrFomemaRslt.fld_FormemaTypeCode && x.fld_Deleted == false).Count();
            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrFomemaRslt.fld_LbrRefID);
            if (GetExistingFomema == 0)
            {
                if (ModelState.IsValid)
                {
                    tbl_LbrFomemaRslt LbrFomemaRslt = new tbl_LbrFomemaRslt();
                    LbrFomemaRslt.fld_LbrRefID = tbl_LbrFomemaRslt.fld_LbrRefID;
                    LbrFomemaRslt.fld_AcknoTypeCode = tbl_LbrFomemaRslt.fld_AcknoTypeCode;
                    LbrFomemaRslt.fld_BizSectorCode = tbl_LbrFomemaRslt.fld_BizSectorCode;
                    LbrFomemaRslt.fld_ClinicID = tbl_LbrFomemaRslt.fld_ClinicID;
                    LbrFomemaRslt.fld_CreatedBy = GetUserID;
                    LbrFomemaRslt.fld_CreatedDT = DT;
                    LbrFomemaRslt.fld_FomemaResult = tbl_LbrFomemaRslt.fld_FomemaResult;
                    LbrFomemaRslt.fld_FormemaTypeCode = tbl_LbrFomemaRslt.fld_FormemaTypeCode;
                    LbrFomemaRslt.fld_LadangID = GetLabourDetails.fld_LadangID;
                    LbrFomemaRslt.fld_NegaraID = GetLabourDetails.fld_NegaraID;
                    LbrFomemaRslt.fld_Remark = tbl_LbrFomemaRslt.fld_Remark;
                    LbrFomemaRslt.fld_ResultDT = tbl_LbrFomemaRslt.fld_ResultDT;
                    LbrFomemaRslt.fld_StateCode = tbl_LbrFomemaRslt.fld_StateCode;
                    LbrFomemaRslt.fld_SyarikatID = GetLabourDetails.fld_SyarikatID;
                    LbrFomemaRslt.fld_WilayahID = GetLabourDetails.fld_WilayahID;
                    LbrFomemaRslt.fld_Deleted = false;
                    db.tbl_LbrFomemaRslt.Add(LbrFomemaRslt);
                    db.SaveChanges();

                    if (tbl_LbrFomemaRslt.fld_FomemaResult == false)
                    {
                        GetLabourDetails.fld_ActiveStatus = "2";
                        GetLabourDetails.fld_InactiveReason = "08";
                        GetLabourDetails.fld_InactiveDT = DT;
                        db.Entry(GetLabourDetails).State = EntityState.Modified;
                        db.SaveChanges();

                        SyncToCheckRollFunc(GetLabourDetails);
                    }

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
                ModelState.AddModelError("", "Fomema Already Applied");
                ViewBag.MsgColor = "color: orange";
            }

            List<SelectListItem> fld_FormemaTypeCode = new List<SelectListItem>();
            List<SelectListItem> fld_FomemaResult = new List<SelectListItem>();
            List<SelectListItem> fld_AcknoTypeCode = new List<SelectListItem>();
            List<SelectListItem> fld_BizSectorCode = new List<SelectListItem>();
            List<SelectListItem> fld_ClinicID = new List<SelectListItem>();
            List<SelectListItem> fld_StateCode = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "fomematype", "acknwldmnttype", "sector", "negeri", "result" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            fld_FormemaTypeCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "fomematype" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            fld_FomemaResult = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "result" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            fld_AcknoTypeCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "acknwldmnttype" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            fld_BizSectorCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "sector" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            fld_StateCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            var GetOneState = GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Take(1).FirstOrDefault();
            var GetClinic = Masterdb.tbl_Clinic.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_StateCode == GetOneState.fldOptConfValue && x.fld_Deleted == false).ToList();
            string DoctorName = "";
            if (GetClinic.Count > 0)
            {
                fld_ClinicID = new SelectList(GetClinic.OrderBy(o => o.fld_ClinicName).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_ClinicName + " - " + s.fld_District }), "Value", "Text").ToList();
                DoctorName = GetClinic.Select(s => s.fld_DoctorName).Take(1).FirstOrDefault();
            }
            else
            {
                fld_ClinicID.Insert(0, (new SelectListItem { Text = "No Data", Value = "" }));
            }

            var CheckExistingFailFmm = db.vw_LbrFememaRslt.Where(x => x.fld_LbrRefID == tbl_LbrFomemaRslt.fld_LbrRefID && x.fld_FomemaResult == false && x.fld_Deleted == false).Count();
            if (CheckExistingFailFmm > 0)
            {
                ViewBag.DisableAll = "Yes";
            }

            ViewBag.fld_FormemaTypeCode = fld_FormemaTypeCode;
            ViewBag.fld_FomemaResult = fld_FomemaResult;
            ViewBag.fld_AcknoTypeCode = fld_AcknoTypeCode;
            ViewBag.fld_BizSectorCode = fld_BizSectorCode;
            ViewBag.fld_ClinicID = fld_ClinicID;
            ViewBag.fld_StateCode = fld_StateCode;
            ViewBag.DoctorName = DoctorName;

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(tbl_LbrFomemaRslt.fld_LbrRefID);
            return View(LbrDataInfo);
        }

        public ActionResult _LabourFomemaDetail(Guid LabourID)
        {
            return View(db.vw_LbrFememaRslt.Where(x => x.fld_LbrRefID == LabourID).ToList());
        }

        public bool SyncToCheckRollFunc(tbl_LbrDataInfo tbl_LbrDataInfo)
        {
            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            bool Result = false;
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, tbl_LbrDataInfo.fld_WilayahID, tbl_LbrDataInfo.fld_SyarikatID, tbl_LbrDataInfo.fld_NegaraID, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
            tbl_Pkjmast tbl_Pkjmast = new tbl_Pkjmast();

            tbl_Pkjmast = dbEstate.tbl_Pkjmast.Where(x => x.fld_Nopkj == tbl_LbrDataInfo.fld_WorkerNo && x.fld_NegaraID == tbl_LbrDataInfo.fld_NegaraID && x.fld_SyarikatID == tbl_LbrDataInfo.fld_SyarikatID && x.fld_WilayahID == tbl_LbrDataInfo.fld_WilayahID && x.fld_LadangID == tbl_LbrDataInfo.fld_LadangID).FirstOrDefault();
            
            tbl_Pkjmast.fld_Trtakf = tbl_LbrDataInfo.fld_InactiveDT;
            tbl_Pkjmast.fld_Kdaktf = tbl_LbrDataInfo.fld_ActiveStatus;
            tbl_Pkjmast.fld_Sbtakf = tbl_LbrDataInfo.fld_InactiveReason;

            try
            {
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

        // GET: HQLabourQuota/Delete/5
        public async Task<ActionResult> CancelLabourFomema(int id)
        {
            ViewBag.LabourFomema = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vw_LbrFomemaRslt vw_LbrFememaRslt = await db.vw_LbrFememaRslt.FindAsync(id);
            if (vw_LbrFememaRslt == null)
            {
                return HttpNotFound();
            }
            
            return View(vw_LbrFememaRslt);
        }

        // POST: HQLabourQuota/Delete/5
        [HttpPost, ActionName("CancelLabourFomema")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelLabourFomemaConfirmed(int id)
        {
            ViewBag.LabourFomema = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            
            tbl_LbrFomemaRslt tbl_LbrFomemaRslt = await db.tbl_LbrFomemaRslt.FindAsync(id);
            tbl_LbrFomemaRslt.fld_Deleted = true;
            tbl_LbrFomemaRslt.fld_DeletedDT = DT;
            tbl_LbrFomemaRslt.fld_DeletedBy = GetUserID;
            db.Entry(tbl_LbrFomemaRslt).State = EntityState.Modified;
            await db.SaveChangesAsync();

            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrFomemaRslt.fld_LbrRefID);
            GetLabourDetails.fld_ActiveStatus = "1";
            GetLabourDetails.fld_InactiveReason = null;
            GetLabourDetails.fld_InactiveDT = null;
            db.Entry(GetLabourDetails).State = EntityState.Modified;
            db.SaveChanges();

            SyncToCheckRollFunc(GetLabourDetails);

            return RedirectToAction("FomemaRegister", new { id = tbl_LbrFomemaRslt.fld_LbrRefID });
        }
    }
}