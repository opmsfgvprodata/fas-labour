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
using MVC_SYSTEM.App_LocalResources;
using MVC_SYSTEM.Attributes;

namespace MVC_SYSTEM.Controllers
{
    public class LabourTKTProcessController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private List<tbl_LbrEstQuota> LbrEstQuota = new List<tbl_LbrEstQuota>();
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;

        // GET: LabourTKTProcess
        public async Task<ActionResult> Index()
        {
            return View(await db.tbl_LbrTKTProcess.ToListAsync());
        }

        // GET: LabourTKTProcess/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrTKTProcess tbl_LbrTKTProcess = await db.tbl_LbrTKTProcess.FindAsync(id);
            if (tbl_LbrTKTProcess == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrTKTProcess);
        }

        // GET: LabourTKTProcess/Create
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        public ActionResult Create(string BatchNo)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            var GetStatusBatchNo = db.tbl_LbrRqst.Where(x => x.fld_BatchNo == BatchNo && x.fld_ApprovedStatus == 1 && x.fld_ProcessStatus != 99).FirstOrDefault();

            //Added by Shazana 31/3/2021
            var GetBatchInfolatest = (from t in db.tbl_LbrRqst
                                      where t.fld_BatchNo == BatchNo && t.fld_ApprovedStatus == 1 && t.fld_ProcessStatus != 99
                                      orderby t.fld_ID descending
                                      select t).Take(1).FirstOrDefault();
            //Close Added by Shazana 31/3/2021

            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            if (GetBatchInfolatest != null)
            {
                ViewBag.fld_LbrRqstID = GetStatusBatchNo.fld_ID;//Commented by Shazana 31/3/2021
                ViewBag.fld_LbrRqstID = GetBatchInfolatest.fld_ID; //Added by Shazana 31 / 3 / 2021
                ViewBag.NotApprovedClosed = true;
                fld_SexType = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text").ToList();
                ViewBag.fld_SexType = fld_SexType;
            }
            else
            {
                ViewBag.NotApprovedClosed = false;
                ModelState.AddModelError("", GlobalResGeneral.msgNotApproved);
            }
            return View();
        }

        // POST: LabourTKTProcess/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(tbl_LbrTKTProcess tbl_LbrTKTProcess)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            string fld_NoIC = tbl_LbrTKTProcess.fld_NoIC;
            fld_NoIC = fld_NoIC.Replace("-", "");
            fld_NoIC = fld_NoIC.Replace(" ", "");
            var CheckExistData = db.tbl_LbrTKTProcess.Where(x => x.fld_SuccessStatus == null || x.fld_SuccessStatus == true).ToList();
            var CheckICExist = CheckExistData.Where(x => x.fld_NoIC == fld_NoIC && (x.fld_SuccessStatus == null || x.fld_SuccessStatus == true)).Count();
            var CheckAppWorkerCount = CheckExistData.Where(x => x.fld_LbrRqstID == tbl_LbrTKTProcess.fld_LbrRqstID && x.fld_SuccessStatus == true).Count();
            var GetBatchInfo = db.tbl_LbrRqst.Find(tbl_LbrTKTProcess.fld_LbrRqstID);

            //Added by Shazana 24/3/2021
            var GetBatchInfolatest = (from t in db.tbl_LbrRqst
                                      where t.fld_BatchNo == GetBatchInfo.fld_BatchNo
                                      orderby t.fld_ID descending
                                      select t).Take(1).FirstOrDefault();

            if (GetBatchInfolatest.fld_TKTQty > CheckAppWorkerCount)
            //Close Added by Shazana 24/3/2021
            //Commented by Shazana 24/3/2021
            //if (GetBatchInfo.fld_TKTQty > CheckAppWorkerCount)
            {
                if (CheckICExist == 0)
                {
                    tbl_LbrTKTProcess.fld_NegaraID = NegaraID;
                    tbl_LbrTKTProcess.fld_SyarikatID = SyarikatID;
                    //Commented by Shazana 26/3/2021
                    //tbl_LbrTKTProcess.fld_WilayahID = GetBatchInfo.fld_WilayahID;
                    //tbl_LbrTKTProcess.fld_LadangID = GetBatchInfo.fld_LadangID;
                    //Close Commented by Shazana 26/3/2021
                    tbl_LbrTKTProcess.fld_CreatedBy = GetUserID;
                    tbl_LbrTKTProcess.fld_CreatedDT = DT;
                    tbl_LbrTKTProcess.fld_ModifiedBy = GetUserID;
                    tbl_LbrTKTProcess.fld_ModifiedDT = DT;
                    tbl_LbrTKTProcess.fld_Nama = tbl_LbrTKTProcess.fld_Nama.ToUpper();
                    tbl_LbrTKTProcess.fld_NoIC = fld_NoIC.ToUpper();
                    tbl_LbrTKTProcess.fld_ProcessStatus = 0;
                    //Added by Shazana 26/3/2021
                    tbl_LbrTKTProcess.fld_WilayahID = GetBatchInfolatest.fld_WilayahID;
                    tbl_LbrTKTProcess.fld_LadangID = GetBatchInfolatest.fld_LadangID;
                    tbl_LbrTKTProcess.fld_LbrRqstID = GetBatchInfolatest.fld_ID;
                    //Close Added by Shazana 26/3/2021

                    if (ModelState.IsValid)
                    {
                        db.tbl_LbrTKTProcess.Add(tbl_LbrTKTProcess);
                        await db.SaveChangesAsync();
                        ModelState.AddModelError("", "Add Successfully");
                        ViewBag.MsgColor = "color: green";
                    }
                }
                else
                {
                    ModelState.AddModelError("", "IC No already exist");
                    ViewBag.MsgColor = "color: red";
                }
            }
            else
            {
                ModelState.AddModelError("", "Quota TKT already full");
                ViewBag.MsgColor = "color: red";
            }
            fld_SexType = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text").ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_LbrRqstID = tbl_LbrTKTProcess.fld_LbrRqstID;
            ViewBag.NotApprovedClosed = true;
            return View(tbl_LbrTKTProcess);
        }

        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        public ActionResult _ListOfLabourTKTProcess(long LbrRqstID)
        {
            return View(db.tbl_LbrTKTProcess.Where(x => x.fld_LbrRqstID == LbrRqstID).ToList());
        }

        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User")]
        public async Task<ActionResult> ApproveReject(Guid? id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_SuccessStatus = new List<SelectListItem>();
            List<SelectListItem> fld_UnsuccessReason = new List<SelectListItem>();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrTKTProcess tbl_LbrTKTProcess = await db.tbl_LbrTKTProcess.FindAsync(id);
            if (tbl_LbrTKTProcess == null)
            {
                return HttpNotFound();
            }
            var GetBatchNo = db.tbl_LbrRqst.Where(x => x.fld_ID == tbl_LbrTKTProcess.fld_LbrRqstID).Select(s => s.fld_BatchNo).FirstOrDefault();
            fld_SexType = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKTProcess.fld_SexType).ToList();
            fld_SuccessStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "successstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKTProcess.fld_SuccessStatus).ToList();
            fld_UnsuccessReason = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "unsuccessreason" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKTProcess.fld_UnsuccessReason).ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_SuccessStatus = fld_SuccessStatus;
            ViewBag.fld_UnsuccessReason = fld_UnsuccessReason;
            ViewBag.GetBatchNo = GetBatchNo;
            return View(tbl_LbrTKTProcess);
        }

        // POST: LabourTKTProcess/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveReject(tbl_LbrTKTProcess tbl_LbrTKTProcess)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_SuccessStatus = new List<SelectListItem>();
            List<SelectListItem> fld_UnsuccessReason = new List<SelectListItem>();
            var GetLbrTKTProcessData = db.tbl_LbrTKTProcess.Where(x=>x.fld_LbrRqstID == tbl_LbrTKTProcess.fld_LbrRqstID).ToList();
            var LbrTKTProcess = GetLbrTKTProcessData.Where(x => x.fld_ID == tbl_LbrTKTProcess.fld_ID).FirstOrDefault();
            var GetBatchInfo = db.tbl_LbrRqst.Find(LbrTKTProcess.fld_LbrRqstID);
            int GetQuota = int.Parse(GetBatchInfo.fld_TKTQty.ToString());
            int GetAppTKT = GetLbrTKTProcessData.Where(x => x.fld_SuccessStatus == true).Count();

            //Added by Shazana 24/3/2021
            var GetBatchInfolatest = (from t in db.tbl_LbrRqst
                                      where t.fld_BatchNo == GetBatchInfo.fld_BatchNo
                                      orderby t.fld_ID descending
                                      select t).Take(1).FirstOrDefault();
            int GetQuotalaBatchlatest = int.Parse(GetBatchInfolatest.fld_TKTQty.ToString());
            if (GetQuotalaBatchlatest > GetAppTKT)
            //Close Added by Shazana 24/3/2021
            //Commented by Shazana 24/3/2021
            //if (GetQuota > GetAppTKT)
            {
                LbrTKTProcess.fld_Age = tbl_LbrTKTProcess.fld_Age;
                LbrTKTProcess.fld_Nama = tbl_LbrTKTProcess.fld_Nama.ToUpper();
                LbrTKTProcess.fld_PhoneNo = tbl_LbrTKTProcess.fld_PhoneNo;
                LbrTKTProcess.fld_Notes = tbl_LbrTKTProcess.fld_Notes;
                LbrTKTProcess.fld_SexType = tbl_LbrTKTProcess.fld_SexType;
                LbrTKTProcess.fld_ProcessStatus = 3;
                LbrTKTProcess.fld_BOD = tbl_LbrTKTProcess.fld_BOD;
                if (tbl_LbrTKTProcess.fld_SuccessStatus == false)
                {
                    tbl_LbrTKTProcess.fld_UnsuccessReason = tbl_LbrTKTProcess.fld_UnsuccessReason;
                }
                LbrTKTProcess.fld_SuccessStatus = tbl_LbrTKTProcess.fld_SuccessStatus;
                LbrTKTProcess.fld_ModifiedBy = GetUserID;
                LbrTKTProcess.fld_ModifiedDT = DT;
                db.Entry(LbrTKTProcess).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Create", new { BatchNo = GetBatchInfo.fld_BatchNo });
            }
            else
            {
                var GetOtherCandidate = db.tbl_LbrTKTProcess.Where(x => x.fld_LbrRqstID == tbl_LbrTKTProcess.fld_LbrRqstID && x.fld_SuccessStatus != true).ToList();
                GetOtherCandidate.ForEach(u =>
                {
                    u.fld_SuccessStatus = false;
                    u.fld_ProcessStatus = 3;
                });
                db.SaveChanges();
                ModelState.AddModelError("", "Quota TKT already full");
            }
            fld_SexType = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", LbrTKTProcess.fld_SexType).ToList();
            fld_SuccessStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "successstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", LbrTKTProcess.fld_SuccessStatus).ToList();
            fld_UnsuccessReason = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "unsuccessreason" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", LbrTKTProcess.fld_UnsuccessReason).ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_SuccessStatus = fld_SuccessStatus;
            ViewBag.fld_UnsuccessReason = fld_UnsuccessReason;
            return View(LbrTKTProcess);
        }


        // GET: LabourTKTProcess/Edit/5
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_ProcessStatus = new List<SelectListItem>();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrTKTProcess tbl_LbrTKTProcess = await db.tbl_LbrTKTProcess.FindAsync(id);
            if (tbl_LbrTKTProcess == null)
            {
                return HttpNotFound();
            }
            var GetBatchNo = db.tbl_LbrRqst.Where(x => x.fld_ID == tbl_LbrTKTProcess.fld_LbrRqstID).Select(s => s.fld_BatchNo).FirstOrDefault();
            fld_SexType = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKTProcess.fld_SexType).ToList();
            fld_ProcessStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "processstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false && x.fldOptConfFlag2 == "1").OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKTProcess.fld_ProcessStatus).ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_ProcessStatus = fld_ProcessStatus;
            ViewBag.GetBatchNo = GetBatchNo;
            return View(tbl_LbrTKTProcess);
        }

        // POST: LabourTKTProcess/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(tbl_LbrTKTProcess tbl_LbrTKTProcess)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_ProcessStatus = new List<SelectListItem>();
            var LbrTKTProcess = await db.tbl_LbrTKTProcess.FindAsync(tbl_LbrTKTProcess.fld_ID);
            var GetBatchInfo = db.tbl_LbrRqst.Find(LbrTKTProcess.fld_LbrRqstID);
            LbrTKTProcess.fld_Age = tbl_LbrTKTProcess.fld_Age;
            LbrTKTProcess.fld_Nama = tbl_LbrTKTProcess.fld_Nama.ToUpper();
            LbrTKTProcess.fld_PhoneNo = tbl_LbrTKTProcess.fld_PhoneNo;
            LbrTKTProcess.fld_Notes = tbl_LbrTKTProcess.fld_Notes;
            LbrTKTProcess.fld_SexType = tbl_LbrTKTProcess.fld_SexType;
            LbrTKTProcess.fld_ProcessStatus = tbl_LbrTKTProcess.fld_ProcessStatus;
            LbrTKTProcess.fld_ModifiedBy = GetUserID;
            LbrTKTProcess.fld_ModifiedDT = DT;
            LbrTKTProcess.fld_BOD = tbl_LbrTKTProcess.fld_BOD;
            if (ModelState.IsValid)
            {
                db.Entry(LbrTKTProcess).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Create", new { BatchNo = GetBatchInfo.fld_BatchNo });
            }
            fld_SexType = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKTProcess.fld_SexType).ToList();
            fld_ProcessStatus = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "processstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKTProcess.fld_SexType).ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_ProcessStatus = fld_ProcessStatus;
            return View(tbl_LbrTKTProcess);
        }

        // GET: LabourTKTProcess/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrTKTProcess tbl_LbrTKTProcess = await db.tbl_LbrTKTProcess.FindAsync(id);
            if (tbl_LbrTKTProcess == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrTKTProcess);
        }

        // POST: LabourTKTProcess/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            tbl_LbrTKTProcess tbl_LbrTKTProcess = await db.tbl_LbrTKTProcess.FindAsync(id);
            db.tbl_LbrTKTProcess.Remove(tbl_LbrTKTProcess);
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
