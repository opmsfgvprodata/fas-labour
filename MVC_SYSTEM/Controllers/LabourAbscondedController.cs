using MVC_SYSTEM.Attributes;
using MVC_SYSTEM.Class;
using MVC_SYSTEM.CustomModels;
using MVC_SYSTEM.EstateModels;
using MVC_SYSTEM.LabourModels;
using MVC_SYSTEM.MasterModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

//Added by Shazana on 10/8
using Rotativa;
using System.Web.Security;
using MVC_SYSTEM.log;
using MVC_SYSTEM.App_LocalResources;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    public class LabourAbscondedController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private GetConfig GetConfig = new GetConfig();
        private List<tbl_LbrDataInfo> LbrDataInfo = new List<tbl_LbrDataInfo>();
        private List<tbl_LbrAbsconded> LbrAbsconded = new List<tbl_LbrAbsconded>();
        private GeneralFunc GeneralFunc = new GeneralFunc();
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;
        errorlog geterror = new errorlog();
        // GET: LabourAbsconded
        public async Task<ActionResult> Index(int? WilayahIDList, int? LadangIDList, string StatusList, string FreeText, string AbscondedList, tbl_LbrAbsconded tbl_LbrAbsconded, tbl_LbrDataInfo tbl_LbrDataInfo)
        {
            ViewBag.LabourAbsconded = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "statusabscondent" };
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
            List<SelectListItem> Absconded = new List<SelectListItem>();
            Absconded = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusabscondent" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfDesc, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            Absconded.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

            ViewBag.AbscondedList = Absconded;
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;

            GetTriager GetTriager = new GetTriager();
          
           var GetWorkerOnStatus = GetTriager.StatusWorker(2);
          
            foreach (var some in db.tbl_LbrDataInfo)
            {
                var OnStatus = GetWorkerOnStatus.Contains(some.fld_ID);
                if (OnStatus)
                {
                    some.fld_Absconded = "YES";
                }
                else
                {
                    some.fld_Absconded = "NO";
                }
            }
            db.SaveChanges();
          

            if (WilayahID == 0 && LadangID == 0)
                {


                    if (string.IsNullOrEmpty(FreeText))
                    {
                        if (WilayahIDList == 0 && LadangIDList == 0)
                        {
                        if (AbscondedList != (0).ToString())
                        {
                            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == AbscondedList).OrderBy(o => new { o.fld_LadangID ,o.fld_WorkerName }).ToListAsync();
                        }
                        else
                        {
                            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                        }
                        }
                        else if (WilayahIDList != 0 && LadangIDList == 0 && AbscondedList == (0).ToString())
                        {
                                LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA" ).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                        }
                        else if   (WilayahIDList != 0 && LadangIDList == 0)
                        {
                                LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA" && x.fld_Absconded == AbscondedList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                         }
                        else if (WilayahIDList != 0 && LadangIDList != 0 && AbscondedList == (0).ToString())
                        {
                                LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" ).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                         }
                        else
                        {
                            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == AbscondedList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                        }
                    }
                    else
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID  && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(FreeText))
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == AbscondedList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                    }
                    else
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Absconded == AbscondedList && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                    }
                }

            
            return View(LbrDataInfo);
        }


        //Added by Shazana on 16/8
        public ActionResult _LabourAbsconded(int? WilayahIDList, int? LadangIDList,  string FreeText, string AbscondedList, tbl_LbrAbsconded tbl_LbrAbsconded, tbl_LbrDataInfo tbl_LbrDataInfo, string print)
        {
            ViewBag.LabourAbsconded = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "statusabscondent" };
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
            List<SelectListItem> Absconded = new List<SelectListItem>();
            Absconded = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusabscondent" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfDesc, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            Absconded.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

            ViewBag.AbscondedList = Absconded;
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;

            GetTriager GetTriager = new GetTriager();

            var GetWorkerOnStatus = GetTriager.StatusWorker(2);

            foreach (var some in db.tbl_LbrDataInfo)
            {
                var OnStatus = GetWorkerOnStatus.Contains(some.fld_ID);
                if (OnStatus)
                {
                    some.fld_Absconded = "YES";
                }
                else
                {
                    some.fld_Absconded = "NO";
                }
            }
            db.SaveChanges();


            if (WilayahID == 0 && LadangID == 0)
            {


                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {
                        if (AbscondedList != (0).ToString())
                        {
                            LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA" && x.fld_Absconded == AbscondedList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                        }
                        else
                        {
                            LbrDataInfo =  db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                        }
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0 && AbscondedList == (0).ToString())
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Nationality != "MA" && x.fld_Absconded == AbscondedList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                    }
                    else if (WilayahIDList != 0 && LadangIDList != 0 && AbscondedList == (0).ToString())
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA").OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                    }
                    else
                    {
                        LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == AbscondedList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                    }
                }
                else
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(FreeText))
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Nationality != "MA" && x.fld_Absconded == AbscondedList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                }
                else
                {
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Absconded == AbscondedList && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToList();
                }
            }

            if (WilayahIDList == null && LadangIDList == null)
            {
                ViewBag.Message = GlobalResEstate.msgChooseRegionEstateMonthYear;

            }
            return View(LbrDataInfo);
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
               //     geterror.testlog("Try if : " + User.Identity.Name, "Try if : " + CookiesValue, "Try if : " + getcookiesval);
                }
               // geterror.testlog("Try no if : " + User.Identity.Name, "Try no if : " + CookiesValue, "Try no if : " + getcookiesval);
            }
            catch
            {
                HttpCookie authoCookies = new HttpCookie(FormsAuthentication.FormsCookieName, getcookiesval);
                Response.SetCookie(authoCookies);
             //   geterror.testlog("Catch : " + User.Identity.Name, "Catch : " + CookiesValue, "Catch : " + getcookiesval);
            }
          //  geterror.testlog("UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name, "UserName : " + User.Identity.Name);
        }

        public ActionResult PrintWorkerPdf(int? RadioGroup, string WilayahIDList, string LadangIDList, string AbscondedList, string FreeText, int id, string genid)
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
                report = new ActionAsPdf("_LabourAbsconded", new { WilayahIDList, LadangIDList, AbscondedList, FreeText, print })
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


        //Close Added
        public async Task<ActionResult> AbscondedRegister(Guid? id)
        {
            ViewBag.LabourAbsconded = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            var CheckExistingAbscnded = db.vw_LbrAbsconded.Where(x => x.fld_LbrRefID == id && x.fld_Deleted == false).Count();
            if (CheckExistingAbscnded > 0)
            {
                ViewBag.DisableAll = "Yes";
            }

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AbscondedRegister(tbl_LbrAbsconded tbl_LbrAbsconded)
        {
            ViewBag.LabourAbsconded = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            var GetExistingAbsconded = db.tbl_LbrAbsconded.Where(x => x.fld_LbrRefID == tbl_LbrAbsconded.fld_LbrRefID && x.fld_Deleted == false).Count();
            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrAbsconded.fld_LbrRefID);
            if (GetExistingAbsconded == 0)
            {
                if (ModelState.IsValid)
                {
                    tbl_LbrAbsconded LbrAbsconded = new tbl_LbrAbsconded();
                    LbrAbsconded.fld_LbrRefID = tbl_LbrAbsconded.fld_LbrRefID;
                    LbrAbsconded.fld_CreatedBy = GetUserID;
                    LbrAbsconded.fld_CreatedDT = DT;
                    LbrAbsconded.fld_LadangID = GetLabourDetails.fld_LadangID;
                    LbrAbsconded.fld_NegaraID = GetLabourDetails.fld_NegaraID;
                    LbrAbsconded.fld_SyarikatID = GetLabourDetails.fld_SyarikatID;
                    LbrAbsconded.fld_WilayahID = GetLabourDetails.fld_WilayahID;
                    LbrAbsconded.fld_AbscndDT = tbl_LbrAbsconded.fld_AbscndDT;
                    LbrAbsconded.fld_ReportPoliceDT = tbl_LbrAbsconded.fld_ReportPoliceDT;
                    LbrAbsconded.fld_ReportPoliceNo = tbl_LbrAbsconded.fld_ReportPoliceNo;
                    LbrAbsconded.fld_Deleted = false;
                    db.tbl_LbrAbsconded.Add(LbrAbsconded);
                    db.SaveChanges();

                    GetLabourDetails.fld_ActiveStatus = "2";
                    GetLabourDetails.fld_InactiveReason = "02";
                    GetLabourDetails.fld_InactiveDT = DT;
                    db.Entry(GetLabourDetails).State = EntityState.Modified;
                    db.SaveChanges();

                    SyncToCheckRollFunc(GetLabourDetails);

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
                ModelState.AddModelError("", "Absconded Already Applied");
                ViewBag.MsgColor = "color: orange";
            }

            var CheckExistingAbscnded = db.vw_LbrAbsconded.Where(x => x.fld_LbrRefID == tbl_LbrAbsconded.fld_LbrRefID && x.fld_Deleted == false).Count();
            if (CheckExistingAbscnded > 0)
            {
                ViewBag.DisableAll = "Yes";
            }

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(tbl_LbrAbsconded.fld_LbrRefID);
            return View(LbrDataInfo);
        }

        public ActionResult _LabourAbscondedDetail(Guid LabourID)
        {
            return View(db.vw_LbrAbsconded.Where(x => x.fld_LbrRefID == LabourID).ToList());
        }

        public async Task<ActionResult> CancelLabourAbsconded(int id)
        {
            ViewBag.LabourAbsconded = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vw_LbrAbsconded vw_LbrAbsconded = await db.vw_LbrAbsconded.FindAsync(id);
            if (vw_LbrAbsconded == null)
            {
                return HttpNotFound();
            }

            return View(vw_LbrAbsconded);
        }

        // POST: HQLabourQuota/Delete/5
        [HttpPost, ActionName("CancelLabourAbsconded")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelLabourAbscondedConfirmed(int id)
        {
            ViewBag.LabourAbsconded = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            tbl_LbrAbsconded tbl_LbrAbsconded = await db.tbl_LbrAbsconded.FindAsync(id);
            tbl_LbrAbsconded.fld_Deleted = true;
            tbl_LbrAbsconded.fld_DeletedDT = DT;
            tbl_LbrAbsconded.fld_DeletedBy = GetUserID;
            db.Entry(tbl_LbrAbsconded).State = EntityState.Modified;
            await db.SaveChangesAsync();

            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_LbrAbsconded.fld_LbrRefID);
            GetLabourDetails.fld_ActiveStatus = "1";
            GetLabourDetails.fld_InactiveReason = null;
            GetLabourDetails.fld_InactiveDT = null;
            db.Entry(GetLabourDetails).State = EntityState.Modified;
            db.SaveChanges();

            SyncToCheckRollFunc(GetLabourDetails);

            return RedirectToAction("AbscondedRegister", new { id = tbl_LbrAbsconded.fld_LbrRefID });
        }

        public async Task<ActionResult> UpDownloadFileLabourAbsconded(int id)
        {
            ViewBag.LabourAbsconded = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vw_LbrAbsconded vw_LbrAbsconded = await db.vw_LbrAbsconded.FindAsync(id);
            if (vw_LbrAbsconded == null)
            {
                return HttpNotFound();
            }

            return View(vw_LbrAbsconded);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpDownloadFileLabourAbsconded(tbl_LbrAbsconded LbrAbsconded, HttpPostedFileBase FileUpload)
        {
            ViewBag.LabourAbsconded = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            tbl_LbrAbsconded tbl_LbrAbsconded = db.tbl_LbrAbsconded.Find(LbrAbsconded.fld_ID);

            try
            {
                if (FileUpload != null && FileUpload.ContentLength > 0)
                {
                    var FileName = Path.GetFileName(FileUpload.FileName);
                    FileName = FileName.Replace(" ", "_");
                    var path = Server.MapPath("~/FileUploaded/AbscondedFile/" + tbl_LbrAbsconded.fld_ID + "/");
                    bool Exist = Directory.Exists(path);
                    if (!Exist)
                    {
                        Directory.CreateDirectory(path);
                    }
                    var pathfile = Path.Combine(path, FileName);
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

            vw_LbrAbsconded vw_LbrAbsconded = await db.vw_LbrAbsconded.FindAsync(LbrAbsconded.fld_ID);
            return View(vw_LbrAbsconded);
        }

        public ActionResult _LabourAbscondedUploadedFileDetail(int ID)
        {
            tbl_LbrAbsconded tbl_LbrAbsconded = db.tbl_LbrAbsconded.Find(ID);
            string GetPath = Server.MapPath("~/FileUploaded/AbscondedFile/" + tbl_LbrAbsconded.fld_ID + "/");
            FileInfo[] Files = null;
            DirectoryInfo DirectoryInfo = new DirectoryInfo(GetPath);
            List<CustMod_FileUpload> CustMod_FileUpload = new List<CustMod_FileUpload>();
            IOrderedEnumerable<FileInfo> GetFiles;
            bool Exist = Directory.Exists(GetPath);
            if (Exist)
            {
                Files = DirectoryInfo.GetFiles();
                GetFiles = Files.Where(f => f.Extension == ".pdf").OrderBy(o => o.CreationTime);
                int I = 1;
                foreach (var GetFile in GetFiles)
                {
                    CustMod_FileUpload.Add(new CustMod_FileUpload() { ID = I, FileName = GetFile.Name, DateTimeCreated = GetFile.CreationTime, DateTimeModified = GetFile.LastWriteTime, SizeFile = GetFile.Length, RefID = tbl_LbrAbsconded.fld_ID });
                    I++;
                }
            }

            return View(CustMod_FileUpload);
        }

        public FileResult DownloadFile(string FileName, int FileRefID)
        {
            DownloadFiles.FileDownloads DownloadObj = new DownloadFiles.FileDownloads();
            string GetPath = Server.MapPath("~/FileUploaded/AbscondedFile/" + FileRefID + "/");
            var GetFileDetail = DownloadObj.GetFiles(GetPath);
            var CurrentFileName = GetFileDetail.Where(x => x.FileName == FileName).FirstOrDefault();

            string contentType = string.Empty;
            contentType = "application/pdf";
            return File(CurrentFileName.FilePath, contentType, CurrentFileName.FileName);
        }

        public ActionResult DeleteFile(string FileName, int FileRefID)
        {
            string GetPathFile = Server.MapPath("~/FileUploaded/AbscondedFile/" + FileRefID + "/" + FileName);

            bool Exists = System.IO.File.Exists(GetPathFile);
            if (Exists)
            {
                System.IO.File.Delete(GetPathFile);
            }

            vw_LbrAbsconded vw_LbrAbsconded = db.vw_LbrAbsconded.Find(FileRefID);
            return RedirectToAction("UpDownloadFileLabourAbsconded", new { id = FileRefID });
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
    }
}