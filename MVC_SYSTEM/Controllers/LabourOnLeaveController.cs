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
using MVC_SYSTEM.App_LocalResources;
namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    public class LabourOnLeaveController : Controller
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
        // GET: LabourOnLeave
        public async Task<ActionResult> Index(int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, string onleaveList)
        {
            ViewBag.LabourOnLeave = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "statusonleave" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

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
            List<SelectListItem> onLeave = new List<SelectListItem>();
            onLeave = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusonleave" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfDesc, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            onLeave.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

            ViewBag.onleaveList = onLeave;
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;

            GetTriager GetTriager = new GetTriager();

            var GetWorkerOnStatus = GetTriager.StatusWorker(3);

            foreach (var some in db.tbl_LbrDataInfo)
            {
                var OnStatus = GetWorkerOnStatus.Contains(some.fld_ID);
                if (OnStatus)
                {
                    some.fld_Onleave = "YES";
                }
                else
                {
                    some.fld_Onleave = "NO";
                }
            }
            db.SaveChanges();
            if (WilayahID == 0 && LadangID == 0)
            {
                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {
                        if (onleaveList != (0).ToString())
                        {
                            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Onleave == onleaveList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();

                        }
                        else
                        {
                            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();

                        }

                    } //display all 

                    else if (WilayahIDList != 0 && LadangIDList == 0 && onleaveList == (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA" && x.fld_Onleave == onleaveList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                    }
                    else if (WilayahIDList != 0 && LadangIDList != 0 && onleaveList == (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                    }

                    else
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Onleave == onleaveList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                    }
                }
                else
                {
                    LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(FreeText))
                {
                    LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Onleave == onleaveList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                }
                else
                {
                    LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Onleave == onleaveList && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                }
            }


            return View(LbrDataInfo);
        }

        //Added by Shazana on 18/8
        public ActionResult _LabourOnLeave(int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, string onleaveList, string print)
        {
            ViewBag.LabourOnLeave = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "statusonleave" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            ViewBag.Print = print;
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
            List<SelectListItem> onLeave = new List<SelectListItem>();
            onLeave = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusonleave" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfDesc, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            onLeave.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

            ViewBag.onleaveList = onLeave;
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;

            GetTriager GetTriager = new GetTriager();

            var GetWorkerOnStatus = GetTriager.StatusWorker(3);

            foreach (var some in db.tbl_LbrDataInfo)
            {
                var OnStatus = GetWorkerOnStatus.Contains(some.fld_ID);
                if (OnStatus)
                {
                    some.fld_Onleave = "YES";
                }
                else
                {
                    some.fld_Onleave = "NO";
                }
            }
            db.SaveChanges();
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
                        if (onleaveList != (0).ToString())
                        {
                            var LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Onleave == onleaveList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                            return View(LbrDataInfo);
                        }
                        else
                        {
                            var LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                            return View(LbrDataInfo);
                        }

                    } //display all 

                    else if (WilayahIDList != 0 && LadangIDList == 0 && onleaveList == (0).ToString())
                    {
                        var LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                        return View(LbrDataInfo);
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        var LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA" && x.fld_Onleave == onleaveList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                        return View(LbrDataInfo);
                    }
                    else if (WilayahIDList != 0 && LadangIDList != 0 && onleaveList == (0).ToString())
                    {
                        var LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                        return View(LbrDataInfo);
                    }

                    else
                    {
                        var LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Onleave == onleaveList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                        return View(LbrDataInfo);
                    }
                }
                else
                {
                    var LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                    return View(LbrDataInfo);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(FreeText))
                {
                    var LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Onleave == onleaveList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                    return View(LbrDataInfo);
                }
                else
                {
                    var LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Onleave == onleaveList && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                    return View(LbrDataInfo);
                }
            }

     
            //return View(LbrDataInfo);
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
                  //  geterror.testlog("Try if : " + User.Identity.Name, "Try if : " + CookiesValue, "Try if : " + getcookiesval);
                }
               // geterror.testlog("Try no if : " + User.Identity.Name, "Try no if : " + CookiesValue, "Try no if : " + getcookiesval);
            }
            catch
            {
                HttpCookie authoCookies = new HttpCookie(FormsAuthentication.FormsCookieName, getcookiesval);
                Response.SetCookie(authoCookies);
             //   geterror.testlog("Catch : " + User.Identity.Name, "Catch : " + CookiesValue, "Catch : " + getcookiesval);
            }
           // geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
        }

        public ActionResult PrintWorkerPdf(int? RadioGroup, string WilayahIDList, string LadangIDList, string onleaveList, string FreeText, int id, string genid)
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
                report = new ActionAsPdf("_LabourOnLeave", new { WilayahIDList, LadangIDList, onleaveList, FreeText, print })
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
        public async Task<ActionResult> OnLeaveRegister(Guid? id)
        {
            ViewBag.LabourOnLeave = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            var CheckExistingOnLeave = db.vw_LbrOnLeave.Where(x => x.fld_LbrRefID == id && x.fld_Deleted == false).Count();
            if (CheckExistingOnLeave > 0)
            {
                ViewBag.DisableAll = "Yes";
            }

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> OnLeaveRegister(tbl_LbrOnLeave tbl_LbrOnLeave)
        {
            ViewBag.LabourOnLeave = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            var GetExistingOnLeave = db.tbl_LbrOnLeave.Where(x => x.fld_LbrRefID == tbl_LbrOnLeave.fld_LbrRefID && x.fld_Deleted == false).Count();
            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrOnLeave.fld_LbrRefID);
            if (GetExistingOnLeave == 0)
            {
                if (ModelState.IsValid)
                {
                    tbl_LbrOnLeave LbrOnLeave = new tbl_LbrOnLeave();
                    LbrOnLeave.fld_LbrRefID = tbl_LbrOnLeave.fld_LbrRefID;
                    LbrOnLeave.fld_CreatedBy = GetUserID;
                    LbrOnLeave.fld_CreatedDT = DT;
                    LbrOnLeave.fld_LadangID = GetLabourDetails.fld_LadangID;
                    LbrOnLeave.fld_NegaraID = GetLabourDetails.fld_NegaraID;
                    LbrOnLeave.fld_SyarikatID = GetLabourDetails.fld_SyarikatID;
                    LbrOnLeave.fld_WilayahID = GetLabourDetails.fld_WilayahID;
                    LbrOnLeave.fld_LeaveStartDT = tbl_LbrOnLeave.fld_LeaveStartDT;
                    LbrOnLeave.fld_LeaveEndDT = tbl_LbrOnLeave.fld_LeaveEndDT;
                    LbrOnLeave.fld_LeaveTotalDay = tbl_LbrOnLeave.fld_LeaveTotalDay;
                    LbrOnLeave.fld_Deleted = false;
                    db.tbl_LbrOnLeave.Add(LbrOnLeave);
                    db.SaveChanges();

                    GetLabourDetails.fld_OnLeaveStatus = true;
                    GetLabourDetails.fld_OnLeaveLastDT = tbl_LbrOnLeave.fld_LeaveEndDT;
                    GetLabourDetails.fld_InactiveReason = "04";
                    db.Entry(GetLabourDetails).State = EntityState.Modified;
                    db.SaveChanges();
                    
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
                ModelState.AddModelError("", "On Leave Already Applied");
                ViewBag.MsgColor = "color: orange";
            }

            var CheckExistingOnLeave = db.vw_LbrOnLeave.Where(x => x.fld_LbrRefID == tbl_LbrOnLeave.fld_LbrRefID && x.fld_Deleted == false).Count();
            if (CheckExistingOnLeave > 0)
            {
                ViewBag.DisableAll = "Yes";
            }

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(tbl_LbrOnLeave.fld_LbrRefID);
            return View(LbrDataInfo);
        }

        public ActionResult _LabourOnLeaveDetail(Guid LabourID)
        {
            return View(db.vw_LbrOnLeave.Where(x => x.fld_LbrRefID == LabourID).ToList());
        }

        public async Task<ActionResult> CancelLabourOnLeave(int id)
        {
            ViewBag.LabourOnLeave = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vw_LbrOnLeave vw_LbrOnLeave = await db.vw_LbrOnLeave.FindAsync(id);
            if (vw_LbrOnLeave == null)
            {
                return HttpNotFound();
            }

            return View(vw_LbrOnLeave);
        }

        // POST: HQLabourQuota/Delete/5
        [HttpPost, ActionName("CancelLabourOnLeave")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelLabourOnLeaveConfirmed(int id)
        {
            ViewBag.LabourOnLeave = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            tbl_LbrOnLeave tbl_LbrOnLeave = await db.tbl_LbrOnLeave.FindAsync(id);
            tbl_LbrOnLeave.fld_Deleted = true;
            tbl_LbrOnLeave.fld_DeletedDT = DT;
            tbl_LbrOnLeave.fld_DeletedBy = GetUserID;
            db.Entry(tbl_LbrOnLeave).State = EntityState.Modified;
            await db.SaveChangesAsync();

            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrOnLeave.fld_LbrRefID);
            GetLabourDetails.fld_OnLeaveStatus = null;
            GetLabourDetails.fld_OnLeaveLastDT = null;
            GetLabourDetails.fld_InactiveReason = null;
            db.Entry(GetLabourDetails).State = EntityState.Modified;
            db.SaveChanges();

            //SyncToCheckRollFunc(GetLabourDetails);

            return RedirectToAction("OnLeaveRegister", new { id = tbl_LbrOnLeave.fld_LbrRefID });
        }

    }
}