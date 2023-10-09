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
using MVC_SYSTEM.Attributes;
using MVC_SYSTEM.App_LocalResources;
using MVC_SYSTEM.MasterModels;

//Added by Shazana on 10/8
using System.Web.Security;
using MVC_SYSTEM.log;
using Rotativa;


namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
    public class EstLabourQuotaController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private GetConfig GetConfig = new GetConfig();
        private List<tbl_LbrEstQuota> LbrEstQuota = new List<tbl_LbrEstQuota>();
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;
        errorlog geterror = new errorlog();//added by Shazana on 17/8
        ConvertToPdf ConvertToPdf = new ConvertToPdf(); //added by Shazana on 17/8
        // GET: EstLabourQuota
        public ActionResult Index(int? Year, int? WilayahIDList, int? LadangIDList)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int? GetWilayahID = 0;
            int YearRange = 0;
            int CurrentYear = 0;
            YearRange = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1;
            CurrentYear = DT.Year;
            var yearlist = new List<SelectListItem>();
            for (var i = YearRange; i <= CurrentYear; i++)
            {
                if (i == DT.Year)
                {
                    yearlist.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = true });
                }
                else
                {
                    yearlist.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                }
            }
            ViewBag.Year = yearlist;
            if (Year == null)
            {
                Year = short.Parse(DT.Year.ToString());
            }


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
                if (string.IsNullOrEmpty(""))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {
                        var LbrEstQuota =  db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year).ToList();
                        return View(LbrEstQuota);
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        var LbrEstQuota =  db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Year == Year).ToList();
                        return View(LbrEstQuota);
                    }
                    else
                    {
                        var LbrEstQuota =  db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).ToList();
                        return View(LbrEstQuota);
                    }
                }
                else
                {
                    var LbrEstQuota =  db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).ToList();
                    return View(LbrEstQuota);
                }
            }
            else
            {
                return View(LbrEstQuota);

            }
            
            
        }


        //Added By Shazana on 17/8
        public ActionResult _EstLabourQuota(int? Year, int? WilayahIDList, int? LadangIDList, string print)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int? GetWilayahID = 0;
            int YearRange = 0;
            int CurrentYear = 0;
            YearRange = DT.Year - int.Parse(GetConfig.GetData("yeardisplay")) + 1;
            CurrentYear = DT.Year;
            ViewBag.Print = print;
            var yearlist = new List<SelectListItem>();
            for (var i = YearRange; i <= CurrentYear; i++)
            {
                if (i == DT.Year)
                {
                    yearlist.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = true });
                }
                else
                {
                    yearlist.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                }
            }
            ViewBag.Year = yearlist;
            if (Year == null)
            {
                Year = short.Parse(DT.Year.ToString());
            }


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
                if (string.IsNullOrEmpty(""))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {
                        var LbrEstQuota = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year).ToList();
                        return View(LbrEstQuota);
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        var LbrEstQuota = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Year == Year).ToList();
                        return View(LbrEstQuota);
                    }
                    else
                    {
                        var LbrEstQuota = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).ToList();
                        return View(LbrEstQuota);
                    }
                }
                else
                {
                    var LbrEstQuota = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).ToList();
                    return View(LbrEstQuota);
                }
            }
            else
            {
                return View(LbrEstQuota);

            }


        }
        public ActionResult PrintWorkerPdf(int? RadioGroup, string WilayahIDList, string LadangIDList, string Year, int id, string genid)
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
                report = new ActionAsPdf("_EstLabourQuota", new { WilayahIDList, LadangIDList, Year,  print })
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
                   //// geterror.testlog("Try if : " + User.Identity.Name, "Try if : " + CookiesValue, "Try if : " + getcookiesval);
                }
               // geterror.testlog("Try no if : " + User.Identity.Name, "Try no if : " + CookiesValue, "Try no if : " + getcookiesval);
            }
            catch
            {
                HttpCookie authoCookies = new HttpCookie(FormsAuthentication.FormsCookieName, getcookiesval);
                Response.SetCookie(authoCookies);
               // geterror.testlog("Catch : " + User.Identity.Name, "Catch : " + CookiesValue, "Catch : " + getcookiesval);
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
        public ActionResult ExportPDF()
        {
            return new Rotativa.ActionAsPdf("Index")
            {
                FileName = Server.MapPath("EstateLabourQuota.pdf"),
                PageMargins = { Left = 20, Bottom = 20, Right = 20, Top = 20 },
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                PageSize = Rotativa.Options.Size.A4
            };
        }

        // GET: EstLabourQuota/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrDataInfo tbl_LbrDataInfo = new tbl_LbrDataInfo();

            tbl_LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);

            return View(tbl_LbrDataInfo);
        }

      

        // GET: EstLabourQuota/Create
        public ActionResult Create()
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();

            var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
            fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
            var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();
            fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetTopWilayahID.fld_ID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();

            ViewBag.fld_WilayahID = fld_WilayahID;
            ViewBag.fld_LadangID = fld_LadangID;

            return View();
        }

        // POST: EstLabourQuota/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(tbl_LbrEstQuota tbl_LbrEstQuota)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            short Year = short.Parse(DT.Year.ToString());
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();

            var LbrEstQuotaData = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_Deleted == false).ToList();
            var GetLbrEstQuotaExist = LbrEstQuotaData.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == tbl_LbrEstQuota.fld_WilayahID && x.fld_LadangID == tbl_LbrEstQuota.fld_LadangID && x.fld_Year == Year && x.fld_Deleted == false).FirstOrDefault();
            if (GetLbrEstQuotaExist == null)
            {
                var GetHQQuota = db.tbl_LbrHQQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_Deleted == false).Select(s => s.fld_LbrHQQuota).FirstOrDefault();
                var GetReadyEstQuota = LbrEstQuotaData.Sum(s => s.fld_LbrEstQuota);
                var TotalEstReadyQuota = short.Parse(GetReadyEstQuota.ToString());
                if (GetHQQuota >= tbl_LbrEstQuota.fld_LbrEstQuota + TotalEstReadyQuota)
                {
                    tbl_LbrEstQuota.fld_Year = Year;
                    tbl_LbrEstQuota.fld_Deleted = false;
                    tbl_LbrEstQuota.fld_NegaraID = NegaraID;
                    tbl_LbrEstQuota.fld_SyarikatID = SyarikatID;
                    tbl_LbrEstQuota.fld_CreatedBy = GetUserID;
                    tbl_LbrEstQuota.fld_CreatedDT = DT;
                    tbl_LbrEstQuota.fld_ModifiedBy = GetUserID;
                    tbl_LbrEstQuota.fld_ModifiedDT = DT;
                    db.tbl_LbrEstQuota.Add(tbl_LbrEstQuota);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", GlobalResGeneral.msgAppQuotaMore);
                }

            }
            else
            {
                ModelState.AddModelError("", GlobalResGeneral.msgExisting);
            }
            var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
            fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName", tbl_LbrEstQuota.fld_WilayahID).ToList();

            fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == tbl_LbrEstQuota.fld_WilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text", tbl_LbrEstQuota.fld_LadangID).ToList();

            ViewBag.fld_WilayahID = fld_WilayahID;
            ViewBag.fld_LadangID = fld_LadangID;
            return View(tbl_LbrEstQuota);
        }

        // GET: EstLabourQuota/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            short Year = short.Parse(DT.Year.ToString());
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrEstQuota tbl_LbrEstQuota = await db.tbl_LbrEstQuota.FindAsync(id);
            if (tbl_LbrEstQuota == null)
            {
                return HttpNotFound();
            }
            var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
            fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName", tbl_LbrEstQuota.fld_WilayahID).ToList();

            fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == tbl_LbrEstQuota.fld_WilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text", tbl_LbrEstQuota.fld_LadangID).ToList();

            ViewBag.fld_WilayahID = fld_WilayahID;
            ViewBag.fld_LadangID = fld_LadangID;
            return View(tbl_LbrEstQuota);
        }

        // POST: EstLabourQuota/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(tbl_LbrEstQuota tbl_LbrEstQuota)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            short Year = short.Parse(DT.Year.ToString());
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            var LbrEstQuota = db.tbl_LbrEstQuota.Find(tbl_LbrEstQuota.fld_ID);
            var LbrEstQuotaData = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_Deleted == false).ToList();
            var GetHQQuota = db.tbl_LbrHQQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_Deleted == false).Select(s => s.fld_LbrHQQuota).FirstOrDefault();
            var GetReadyEstQuota = LbrEstQuotaData.Sum(s => s.fld_LbrEstQuota);
            var TotalEstReadyQuota = short.Parse(GetReadyEstQuota.ToString());
            var GetTotalEstAppQuota = db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == LbrEstQuota.fld_WilayahID && x.fld_LadangID == LbrEstQuota.fld_LadangID && x.fld_Year == Year && (x.fld_ApprovedStatus == 1 || x.fld_ApprovedStatus == 0)).Sum(s => s.fld_AppReqQty);
            var TotalEstAppQuota = GetTotalEstAppQuota == null ? 0 : short.Parse(GetTotalEstAppQuota.ToString());
            if (GetHQQuota >= tbl_LbrEstQuota.fld_LbrEstQuota - LbrEstQuota.fld_LbrEstQuota + TotalEstReadyQuota && tbl_LbrEstQuota.fld_LbrEstQuota >= TotalEstAppQuota)
            {
                LbrEstQuota.fld_LbrEstQuota = tbl_LbrEstQuota.fld_LbrEstQuota;
                LbrEstQuota.fld_ModifiedDT = DT;
                LbrEstQuota.fld_ModifiedBy = GetUserID;
                db.Entry(LbrEstQuota).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", GlobalResGeneral.msgAppQuotaMore);
            }


            var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
            fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName", LbrEstQuota.fld_WilayahID).ToList();

            fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == LbrEstQuota.fld_WilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text", LbrEstQuota.fld_LadangID).ToList();

            ViewBag.fld_WilayahID = fld_WilayahID;
            ViewBag.fld_LadangID = fld_LadangID;
            return View(LbrEstQuota);
        }

        // GET: EstLabourQuota/Delete/5
        public async Task<ActionResult> Delete(int? id, short? DeleteStatus = 99)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrEstQuota tbl_LbrEstQuota = await db.tbl_LbrEstQuota.FindAsync(id);
            if (tbl_LbrEstQuota == null)
            {
                return HttpNotFound();
            }
            if (DeleteStatus == 0)
            {
                ViewBag.DeleteStatus = GlobalResGeneral.msgQuotaUsed;
            }
            return View(tbl_LbrEstQuota);
        }

        // POST: EstLabourQuota/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            short DeleteStatus = 0;
            short Year = short.Parse(DT.Year.ToString());
            tbl_LbrEstQuota tbl_LbrEstQuota = await db.tbl_LbrEstQuota.FindAsync(id);
            var GetTotalEstAppQuota = db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == tbl_LbrEstQuota.fld_WilayahID && x.fld_LadangID == tbl_LbrEstQuota.fld_LadangID && x.fld_Year == Year && x.fld_ApprovedStatus == 1).Count();
            if (GetTotalEstAppQuota == 0)
            {
                tbl_LbrEstQuota.fld_Deleted = true;
                tbl_LbrEstQuota.fld_ModifiedDT = DT;
                tbl_LbrEstQuota.fld_ModifiedBy = GetUserID;
                db.Entry(tbl_LbrEstQuota).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            else
            {
                return RedirectToAction("Delete", new { id, DeleteStatus });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                Masterdb.Dispose();
            }
            base.Dispose(disposing);
        }

        //Added By Shazana on 17/8

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



        //    Close added
    }
}
