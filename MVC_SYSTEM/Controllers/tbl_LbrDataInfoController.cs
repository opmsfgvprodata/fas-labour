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

namespace MVC_SYSTEM.Controllers
{
    public class tbl_LbrDataInfoController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();

        // GET: tbl_LbrDataInfo
        public async Task<ActionResult> Index()
        {
            return View(await db.tbl_LbrDataInfo.ToListAsync());
        }

        // GET: tbl_LbrDataInfo/Details/5
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

        // GET: tbl_LbrDataInfo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: tbl_LbrDataInfo/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "fld_ID,fld_WorkerIDNo,fld_WorkerNo,fld_WorkerName,fld_WorkerAddress,fld_Postcode,fld_State,fld_Country,fld_PhoneNo,fld_SexType,fld_Race,fld_Religion,fld_BOD,fld_Age,fld_Nationality,fld_MarriedStatus,fld_FeldaRelated,fld_ActiveStatus,fld_InactiveReason,fld_InactiveDT,fld_Notes,fld_WorkingStartDT,fld_ConfirmationDT,fld_WorkerType,fld_WorkCtgry,fld_NegaraID,fld_SyarikatID,fld_WilayahID,fld_LadangID,fld_DivisionID,fld_LbrProcessID")] tbl_LbrDataInfo tbl_LbrDataInfo)
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

        // GET: tbl_LbrDataInfo/Edit/5
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

        // POST: tbl_LbrDataInfo/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "fld_ID,fld_WorkerIDNo,fld_WorkerNo,fld_WorkerName,fld_WorkerAddress,fld_Postcode,fld_State,fld_Country,fld_PhoneNo,fld_SexType,fld_Race,fld_Religion,fld_BOD,fld_Age,fld_Nationality,fld_MarriedStatus,fld_FeldaRelated,fld_ActiveStatus,fld_InactiveReason,fld_InactiveDT,fld_Notes,fld_WorkingStartDT,fld_ConfirmationDT,fld_WorkerType,fld_WorkCtgry,fld_NegaraID,fld_SyarikatID,fld_WilayahID,fld_LadangID,fld_DivisionID,fld_LbrProcessID")] tbl_LbrDataInfo tbl_LbrDataInfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_LbrDataInfo).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(tbl_LbrDataInfo);
        }

        // GET: tbl_LbrDataInfo/Delete/5
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

        // POST: tbl_LbrDataInfo/Delete/5
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
