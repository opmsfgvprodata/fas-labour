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

namespace MVC_SYSTEM.Controllers
{
    public class LabourRequestController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private GetConfig GetConfig = new GetConfig();
        private List<tbl_LbrRqst> LbrRqst = new List<tbl_LbrRqst>();
        private GeneralFunc GeneralFunc = new GeneralFunc();
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;

        // GET: LabourRequest
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        public async Task<ActionResult> Index(short? Year, int? WilayahIDList, int? LadangIDList)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int YearRange = 0;
            int CurrentYear = 0;
            int? GetWilayahID = 0;
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
                if (string.IsNullOrEmpty(""))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {
                        LbrRqst = await db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year).OrderBy(o => new { o.fld_ApprovedStatus }).ToListAsync();
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        LbrRqst = await db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID &&  x.fld_WilayahID == WilayahIDList && x.fld_Year == Year).OrderBy(o => new { o.fld_ApprovedStatus }).ToListAsync();

                    }
                    else
                    {
                        LbrRqst = await db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Year == Year).OrderBy(o => new { o.fld_ApprovedStatus }).ToListAsync();
                    }
                }
                else
                {
                    LbrRqst = await db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Year == Year).OrderBy(o => new { o.fld_ApprovedStatus }).ToListAsync();

                }

            return View(LbrRqst);
        }

        // GET: LabourRequest/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrRqst tbl_LbrRqst = await db.tbl_LbrRqst.FindAsync(id);
            if (tbl_LbrRqst == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrRqst);
        }


        //Edited by Shazana on 26/8
        public ActionResult CreateRoute()
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            if (WilayahID == 0 && LadangID == 0)
            {
                return RedirectToAction("CreateHQ");
            }
            else
            {
                return RedirectToAction("Create");
            }
        }

        // GET: LabourRequest/Create
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
        public ActionResult CreateHQ(int? CodeWilayahID, int? CodeLadangID)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();

            var Wilayah = CodeWilayahID;
            var Ladang = CodeLadangID;
            var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
            fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
            var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();
            fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetTopWilayahID.fld_ID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();

            ViewBag.fld_WilayahID = fld_WilayahID;
            ViewBag.fld_LadangID = fld_LadangID;
            return View();
        }

        // POST: LabourRequest/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateHQ(tbl_LbrRqst tbl_LbrRqst)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            short Year = short.Parse(DT.Year.ToString());
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            short Month = short.Parse(DT.Month.ToString());
            var GetTotalLabourRequest = db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == tbl_LbrRqst.fld_WilayahID && x.fld_LadangID == tbl_LbrRqst.fld_LadangID && x.fld_Year == Year && (x.fld_ApprovedStatus == 0 || x.fld_ApprovedStatus == 1) && x.fld_Deleted == false).ToList();
            var GetPendingRequest = GetTotalLabourRequest.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == tbl_LbrRqst.fld_WilayahID && x.fld_LadangID == tbl_LbrRqst.fld_LadangID && x.fld_Year == Year && x.fld_ApprovedStatus == 0 && x.fld_Deleted == false).Count();
            int LbrReqQty = tbl_LbrRqst.fld_TKAQty.Value + tbl_LbrRqst.fld_TKTQty.Value;
            var GetEstLabourQuota = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == tbl_LbrRqst.fld_WilayahID && x.fld_LadangID == tbl_LbrRqst.fld_LadangID && x.fld_Year == Year && x.fld_Deleted == false).Select(s => s.fld_LbrEstQuota).FirstOrDefault();
            int GetEstLabourQuotaInt = int.Parse(GetEstLabourQuota.ToString());
            var GetTotalEstLabourRequest = GetTotalLabourRequest.Sum(s => s.fld_AppReqQty);
            if (GetPendingRequest == 0 && GetEstLabourQuotaInt >= GetTotalEstLabourRequest + LbrReqQty)
            {
                string BatchNo = GeneralFunc.BatchNoFunc(NegaraID, SyarikatID, tbl_LbrRqst.fld_WilayahID, tbl_LbrRqst.fld_LadangID, "LBRREQ", "lbrreqbatchno", db);

                tbl_LbrRqst.fld_BatchNo = BatchNo;
                tbl_LbrRqst.fld_ApprovedStatus = 0;
                tbl_LbrRqst.fld_Month = Month;
                tbl_LbrRqst.fld_NegaraID = NegaraID;
                tbl_LbrRqst.fld_SyarikatID = SyarikatID;
                tbl_LbrRqst.fld_CreatedBy = GetUserID;
                tbl_LbrRqst.fld_CreatedDT = DT;
                tbl_LbrRqst.fld_ModifiedBy = GetUserID;
                tbl_LbrRqst.fld_ModifiedDT = DT;
                tbl_LbrRqst.fld_LbrReqQty = short.Parse(LbrReqQty.ToString());
                tbl_LbrRqst.fld_ProcessStatus = 0;
                tbl_LbrRqst.fld_Deleted = false;
                tbl_LbrRqst.fld_Year = Year;
                db.tbl_LbrRqst.Add(tbl_LbrRqst);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                if (GetPendingRequest > 0)
                {
                    ModelState.AddModelError("", GlobalResGeneral.msgPendingProcess);
                }
                else
                {
                    ModelState.AddModelError("", GlobalResGeneral.msgNoLabourQuota);
                }
            }
            var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
            fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName", tbl_LbrRqst.fld_WilayahID).ToList();

            fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == tbl_LbrRqst.fld_WilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text", tbl_LbrRqst.fld_LadangID).ToList();

            ViewBag.fld_WilayahID = fld_WilayahID;
            ViewBag.fld_LadangID = fld_LadangID;
            return View(tbl_LbrRqst);
        }

        // GET: LabourRequest/Create
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        public ActionResult Create()
        {
            ViewBag.LabourRequest = "class = active";

            return View();
        }

        // POST: LabourRequest/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(tbl_LbrRqst tbl_LbrRqst)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            short Year = short.Parse(DT.Year.ToString());
            short Month = short.Parse(DT.Month.ToString());
            var GetTotalLabourRequest = db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Year == Year && (x.fld_ApprovedStatus == 0 || x.fld_ApprovedStatus == 1) && x.fld_Deleted == false).ToList();
            var GetPendingRequest = GetTotalLabourRequest.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Year == Year && x.fld_ApprovedStatus == 0 && x.fld_Deleted == false).Count();
            int LbrReqQty = tbl_LbrRqst.fld_TKAQty.Value + tbl_LbrRqst.fld_TKTQty.Value;
            var GetEstLabourQuota = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Year == Year && x.fld_Deleted == false).Select(s => s.fld_LbrEstQuota).FirstOrDefault();
            int GetEstLabourQuotaInt = int.Parse(GetEstLabourQuota.ToString());
            var GetTotalEstLabourRequest = GetTotalLabourRequest.Sum(s => s.fld_AppReqQty);
            if (GetPendingRequest == 0 && GetEstLabourQuotaInt >= GetTotalEstLabourRequest + LbrReqQty)
            {
                string BatchNo = GeneralFunc.BatchNoFunc(NegaraID, SyarikatID, WilayahID, LadangID, "LBRREQ", "lbrreqbatchno", db);

                tbl_LbrRqst.fld_BatchNo = BatchNo;
                tbl_LbrRqst.fld_ApprovedStatus = 0;
                tbl_LbrRqst.fld_Month = Month;
                tbl_LbrRqst.fld_NegaraID = NegaraID;
                tbl_LbrRqst.fld_SyarikatID = SyarikatID;
                tbl_LbrRqst.fld_WilayahID = WilayahID;
                tbl_LbrRqst.fld_LadangID = LadangID;
                tbl_LbrRqst.fld_CreatedBy = GetUserID;
                tbl_LbrRqst.fld_CreatedDT = DT;
                tbl_LbrRqst.fld_ModifiedBy = GetUserID;
                tbl_LbrRqst.fld_ModifiedDT = DT;
                tbl_LbrRqst.fld_LbrReqQty = short.Parse(LbrReqQty.ToString());
                tbl_LbrRqst.fld_ProcessStatus = 0;
                tbl_LbrRqst.fld_Deleted = false;
                tbl_LbrRqst.fld_Year = Year;
                db.tbl_LbrRqst.Add(tbl_LbrRqst);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                if (GetPendingRequest > 0)
                {
                    ModelState.AddModelError("", GlobalResGeneral.msgPendingProcess);
                }
                else
                {
                    ModelState.AddModelError("", GlobalResGeneral.msgNoLabourQuota);
                }
            }

            return View(tbl_LbrRqst);
        }

        // GET: LabourRequest/Edit/5
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
        public async Task<ActionResult> Approve(long? id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrRqst tbl_LbrRqst = await db.tbl_LbrRqst.FindAsync(id);
            if (tbl_LbrRqst == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrRqst);
        }

        // POST: LabourRequest/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(tbl_LbrRqst tbl_LbrRqst)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            short Year = short.Parse(DT.Year.ToString());
            var LbrRqst = db.tbl_LbrRqst.Find(tbl_LbrRqst.fld_ID);
            var GetEstLabourQuota = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == LbrRqst.fld_WilayahID && x.fld_LadangID == LbrRqst.fld_LadangID && x.fld_Year == Year && x.fld_Deleted == false).Select(s => s.fld_LbrEstQuota).FirstOrDefault();
            int GetEstLabourQuotaInt = int.Parse(GetEstLabourQuota.ToString());
            int LbrAppQty = tbl_LbrRqst.fld_TKAQty.Value + tbl_LbrRqst.fld_TKTQty.Value;
            var GetTotalLabourApp = db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == LbrRqst.fld_WilayahID && x.fld_LadangID == LbrRqst.fld_LadangID && x.fld_Year == Year && x.fld_ApprovedStatus == 1 && x.fld_Deleted == false).Sum(s => s.fld_AppReqQty);
            int GetTotalLabourAppInt = GetTotalLabourApp == null ? 0 : int.Parse(GetTotalLabourApp.ToString());
            if (GetEstLabourQuotaInt >= GetTotalLabourAppInt + LbrAppQty)
            {
                LbrRqst.fld_AppReqQty = short.Parse(LbrAppQty.ToString());
                LbrRqst.fld_TKAQty = tbl_LbrRqst.fld_TKAQty;
                LbrRqst.fld_TKTQty = tbl_LbrRqst.fld_TKTQty;
                LbrRqst.fld_ApprovedBy = GetUserID;
                LbrRqst.fld_ApprovedDT = DT;
                LbrRqst.fld_ModifiedBy = GetUserID;
                LbrRqst.fld_ModifiedDT = DT;
                LbrRqst.fld_ApprovedStatus = 1;
                LbrRqst.fld_ProcessStatus = 1;
                if (ModelState.IsValid)
                {
                    db.Entry(LbrRqst).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                ModelState.AddModelError("", GlobalResGeneral.msgAppQuotaMore);
            }
            
            return View(tbl_LbrRqst);
        }

        // GET: LabourRequest/Delete/5
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
        public async Task<ActionResult> Reject(long? id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrRqst tbl_LbrRqst = await db.tbl_LbrRqst.FindAsync(id);
            if (tbl_LbrRqst == null)
            {
                return HttpNotFound();
            }
            var GetTKTProcess = db.tbl_LbrTKTProcess.Where(x => x.fld_LbrRqstID == id).Count();
            var GetTKAProcess = db.tbl_LbrTKAProcess.Where(x => x.fld_LbrRqstID == id).Count();
            if (GetTKTProcess > 0 || GetTKAProcess > 0)
            {
                ModelState.AddModelError("", "Batch already labour to process, batch connot be reject");
                ViewBag.DisabledBtn = true;
            }
            else
            {
                ViewBag.DisabledBtn = false;
            }
            return View(tbl_LbrRqst);
        }

        // POST: LabourRequest/Delete/5
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
        [HttpPost, ActionName("Reject")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RejectConfirmed(long id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            tbl_LbrRqst tbl_LbrRqst = await db.tbl_LbrRqst.FindAsync(id);
            tbl_LbrRqst.fld_ProcessStatus = 0;
            tbl_LbrRqst.fld_ApprovedStatus = 2;
            tbl_LbrRqst.fld_ApprovedBy = null;
            tbl_LbrRqst.fld_ApprovedDT = null;
            tbl_LbrRqst.fld_ModifiedBy = GetUserID;
            tbl_LbrRqst.fld_ModifiedDT = DT;
            db.Entry(tbl_LbrRqst).State = EntityState.Modified;
            await db.SaveChangesAsync();
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
    }
}
