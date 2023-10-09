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
using MVC_SYSTEM.MasterModels;
using MVC_SYSTEM.Class;

namespace MVC_SYSTEM.Controllers
{
    public class LabourDataInfomationController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private List<tbl_LbrDataInfo> LbrDataInfo = new List<tbl_LbrDataInfo>();
        private GeneralFunc GeneralFunc = new GeneralFunc();
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;

        // GET: LabourDataInfomation
        public async Task<ActionResult> Index()
        {
            ViewBag.LabourInfo = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (WilayahID == 0 && LadangID == 0)
            {
                LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderByDescending(o => new { o.fld_WorkerName, o.fld_Nationality }).ToListAsync();
            }
            else
            {
                LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID).OrderByDescending(o => new { o.fld_WorkerName, o.fld_Nationality }).ToListAsync();
            }
            return View(LbrDataInfo);
        }

        // GET: LabourDataInfomation/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrDataInfo tbl_LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            if (tbl_LbrDataInfo == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrDataInfo);
        }

        // GET: LabourDataInfomation/Create
        public ActionResult Create(string BatchNo)
        {
            return View();
        }

        // POST: LabourDataInfomation/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(tbl_LbrDataInfo tbl_LbrDataInfo)
        {
            if (ModelState.IsValid)
            {
                tbl_LbrDataInfo.fld_ID = Guid.NewGuid();
                db.tbl_LbrDataInfo.Add(tbl_LbrDataInfo);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(tbl_LbrDataInfo);
        }

        // GET: LabourDataInfomation/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrDataInfo tbl_LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            if (tbl_LbrDataInfo == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrDataInfo);
        }

        // POST: LabourDataInfomation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(tbl_LbrDataInfo tbl_LbrDataInfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_LbrDataInfo).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(tbl_LbrDataInfo);
        }

        // GET: LabourDataInfomation/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrDataInfo tbl_LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            if (tbl_LbrDataInfo == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrDataInfo);
        }

        // POST: LabourDataInfomation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            tbl_LbrDataInfo tbl_LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            db.tbl_LbrDataInfo.Remove(tbl_LbrDataInfo);
            await db.SaveChangesAsync();
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
