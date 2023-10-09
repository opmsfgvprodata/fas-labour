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
using MVC_SYSTEM.Attributes;
using MVC_SYSTEM.Class;
using MVC_SYSTEM.Security;
using MVC_SYSTEM.App_LocalResources;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
    public class HQLabourQuotaController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private GetConfig GetConfig = new GetConfig();
        private List<tbl_LbrHQQuota> LbrHQQuota = new List<tbl_LbrHQQuota>();
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;

        // GET: HQLabourQuota
        public async Task<ActionResult> Index(short? Year)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
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
            LbrHQQuota = await db.tbl_LbrHQQuota.Where(x=> x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_Deleted == false).ToListAsync();
            return View(LbrHQQuota);
        }



        // GET: HQLabourQuota/Details/5
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
            tbl_LbrHQQuota tbl_LbrHQQuota = await db.tbl_LbrHQQuota.FindAsync(id);
            if (tbl_LbrHQQuota == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrHQQuota);
        }

        // GET: HQLabourQuota/Create
        public ActionResult Create()
        {
            ViewBag.LabourQuota = "class = active";
            return View();
        }

        // POST: HQLabourQuota/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(tbl_LbrHQQuota tbl_LbrHQQuota)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            short Year = short.Parse(DT.Year.ToString());
            var GetLbrHQQuotaExist = db.tbl_LbrHQQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_Deleted == false).FirstOrDefault();
            if (GetLbrHQQuotaExist == null)
            {
                tbl_LbrHQQuota.fld_Year = Year;
                tbl_LbrHQQuota.fld_Deleted = false;
                tbl_LbrHQQuota.fld_NegaraID = NegaraID;
                tbl_LbrHQQuota.fld_SyarikatID = SyarikatID;
                tbl_LbrHQQuota.fld_CreatedBy = GetUserID;
                tbl_LbrHQQuota.fld_CreatedDT = DT;
                tbl_LbrHQQuota.fld_ModifiedBy = GetUserID;
                tbl_LbrHQQuota.fld_ModifiedDT = DT;
                db.tbl_LbrHQQuota.Add(tbl_LbrHQQuota);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", GlobalResGeneral.msgExisting);
            }

            return View(tbl_LbrHQQuota);
        }

        // GET: HQLabourQuota/Edit/5
        public async Task<ActionResult> Edit(int? id)
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
            tbl_LbrHQQuota tbl_LbrHQQuota = await db.tbl_LbrHQQuota.FindAsync(id);
            if (tbl_LbrHQQuota == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrHQQuota);
        }

        // POST: HQLabourQuota/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(tbl_LbrHQQuota tbl_LbrHQQuota)
        {
            ViewBag.LabourQuota = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            short Year = short.Parse(DT.Year.ToString());
            var GetTotalEstAppQuota = db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && (x.fld_ApprovedStatus == 1 || x.fld_ApprovedStatus == 0)).Sum(s => s.fld_AppReqQty);
            var TotalEstAppQuota = GetTotalEstAppQuota == null ? 0 : short.Parse(GetTotalEstAppQuota.ToString());
            var LbrEstQuotaData = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_Deleted == false).ToList();
            var GetReadyEstQuota = LbrEstQuotaData.Sum(s => s.fld_LbrEstQuota);
            var TotalEstReadyQuota = short.Parse(GetReadyEstQuota.ToString());
            var LbrHQQuota = db.tbl_LbrHQQuota.Find(tbl_LbrHQQuota.fld_ID);
            if (tbl_LbrHQQuota.fld_LbrHQQuota >= TotalEstAppQuota && tbl_LbrHQQuota.fld_LbrHQQuota >= TotalEstReadyQuota)
            {
                LbrHQQuota.fld_LbrHQQuota = tbl_LbrHQQuota.fld_LbrHQQuota;
                LbrHQQuota.fld_ModifiedBy = GetUserID;
                LbrHQQuota.fld_ModifiedDT = DT;
                db.Entry(LbrHQQuota).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", GlobalResGeneral.msgAppQuotaMore);
            }
            return View(tbl_LbrHQQuota);
        }

        // GET: HQLabourQuota/Delete/5
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
            tbl_LbrHQQuota tbl_LbrHQQuota = await db.tbl_LbrHQQuota.FindAsync(id);
            if (tbl_LbrHQQuota == null)
            {
                return HttpNotFound();
            }
            if (DeleteStatus == 0)
            {
                ViewBag.DeleteStatus = GlobalResGeneral.msgQuotaUsed;
            }
            return View(tbl_LbrHQQuota);
        }

        // POST: HQLabourQuota/Delete/5
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
            var GetReadyEstQuota = db.tbl_LbrEstQuota.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && x.fld_Deleted == false).Count();
            var GetTotalEstAppQuota = db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Year == Year && (x.fld_ApprovedStatus == 1 || x.fld_ApprovedStatus == 0)).Count();
            if (GetReadyEstQuota == 0 && GetTotalEstAppQuota == 0)
            {
                tbl_LbrHQQuota tbl_LbrHQQuota = await db.tbl_LbrHQQuota.FindAsync(id);
                tbl_LbrHQQuota.fld_Deleted = true;
                tbl_LbrHQQuota.fld_ModifiedDT = DT;
                tbl_LbrHQQuota.fld_ModifiedBy = GetUserID;
                db.Entry(tbl_LbrHQQuota).State = EntityState.Modified;
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
            }
            base.Dispose(disposing);
        }
    }
}
