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
using System.IO;
using ExcelDataReader;
using System.Globalization;
using MVC_SYSTEM.Attributes;

namespace MVC_SYSTEM.Controllers
{
    public class LabourTKAProcessController : Controller
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

        // GET: LabourTKAProcess
        public async Task<ActionResult> Index()
        {
            return View(await db.tbl_LbrTKAProcess.ToListAsync());
        }

        // GET: LabourTKAProcess/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrTKAProcess tbl_LbrTKAProcess = await db.tbl_LbrTKAProcess.FindAsync(id);
            if (tbl_LbrTKAProcess == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrTKAProcess);
        }

        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
        public ActionResult UploadTKAData(string BatchNo)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            var GetStatusBatchNo = db.tbl_LbrRqst.Where(x => x.fld_BatchNo == BatchNo && x.fld_ApprovedStatus == 1 && x.fld_ProcessStatus != 99).FirstOrDefault();
            
            if (GetStatusBatchNo != null)
            {
                ViewBag.fld_LbrRqstID = GetStatusBatchNo.fld_ID;
                ViewBag.NotApprovedClosed = true;
            }
            else
            {
                ViewBag.NotApprovedClosed = false;
                ModelState.AddModelError("", GlobalResGeneral.msgNotApproved);
            }
            return View();
        }

        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadTKAData(HttpPostedFileBase FileUpload, long fld_LbrRqstID)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<tbl_LbrTKAProcess> LbrTKAProcessList = new List<tbl_LbrTKAProcess>();
            var GetLbrRqstDetails = db.tbl_LbrRqst.Find(fld_LbrRqstID);
            try
            {
                int LUPath = FileUpload.FileName.LastIndexOf("\\") + 1;
                int LUPathFileName = FileUpload.FileName.Length;
                int GetFileNameLength = LUPathFileName - LUPath;
                string FileName = FileUpload.FileName.Substring(LUPath, GetFileNameLength);
                if (FileUpload != null && FileUpload.ContentLength > 0 && GetLbrRqstDetails.fld_BatchNo == FileName.Replace(".xlsx",""))
                {
                    // ExcelDataReader works with the binary Excel file, so it needs a FileStream
                    // to get started. This is how we avoid dependencies on ACE or Interop:
                    Stream stream = FileUpload.InputStream;

                    // We return the interface, so that
                    IExcelDataReader reader = null;


                    if (FileUpload.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (FileUpload.FileName.EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        ModelState.AddModelError("", "This file format is not supported");
                        return View();
                    }

                    DataSet result = reader.AsDataSet();
                    reader.Close();
                    int loop = 1;
                    string WorkerName = "";
                    string PassportNo = "";
                    DateTime BOD = new DateTime();
                    string OriginBatchNo = "";
                    short QueueNo = 0;
                    foreach (DataRow row in result.Tables[0].Rows)
                    {
                        if (loop >= 4)
                        {
                            WorkerName = row.ItemArray[1].ToString();
                            PassportNo = row.ItemArray[5].ToString();
                            BOD = DateTime.ParseExact(row.ItemArray[6].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                            OriginBatchNo = row.ItemArray[7].ToString();
                            QueueNo = short.Parse(row.ItemArray[11].ToString());
                            LbrTKAProcessList.Add(new tbl_LbrTKAProcess { fld_Nama = WorkerName, fld_NoPassport = PassportNo, fld_BOD = BOD, fld_OriginBatchNo = OriginBatchNo, fld_QueueNo = QueueNo, fld_NegaraID = NegaraID, fld_SyarikatID = SyarikatID, fld_WilayahID = GetLbrRqstDetails.fld_WilayahID, fld_LadangID = GetLbrRqstDetails.fld_LadangID, fld_LbrRqstID = GetLbrRqstDetails.fld_ID, fld_CreatedBy = GetUserID, fld_CreatedDT = DT, fld_ModifiedBy = GetUserID, fld_ModifiedDT = DT, fld_ProcessStatus = 0, fld_Notes = "1) Upload by excel" });
                        }
                        loop++;
                    }
                    if (GetLbrRqstDetails.fld_TKAQty >= LbrTKAProcessList.Count)
                    {
                        db.tbl_LbrTKAProcess.AddRange(LbrTKAProcessList);
                        db.SaveChanges();
                        ModelState.AddModelError("", "Uploaded successfully");
                        ViewBag.MsgColor = "color: green";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please check with approval quota");
                    }
                    
                }
                else
                {
                    ModelState.AddModelError("", "File is not valid");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", GlobalResGeneral.lblContactTechnical);
            }
            ViewBag.fld_LbrRqstID = GetLbrRqstDetails.fld_ID;
            ViewBag.NotApprovedClosed = true;
            return View("UploadTKAData", new { BatchNo = GetLbrRqstDetails.fld_BatchNo });
        }

        // GET: LabourTKAProcess/Create
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        public ActionResult Create(string BatchNo)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            var GetStatusBatchNo = db.tbl_LbrRqst.Where(x => x.fld_BatchNo == BatchNo && x.fld_ApprovedStatus == 1 && x.fld_ProcessStatus != 99).FirstOrDefault();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            if (GetStatusBatchNo != null)
            {
                ViewBag.fld_LbrRqstID = GetStatusBatchNo.fld_ID;
                ViewBag.BatchNo = BatchNo;
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

        // POST: LabourTKAProcess/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(tbl_LbrTKAProcess tbl_LbrTKAProcess)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            string fld_NoPassport = tbl_LbrTKAProcess.fld_NoPassport;
            fld_NoPassport = fld_NoPassport.Replace("-", "");
            fld_NoPassport = fld_NoPassport.Replace(" ", "");
            var CheckExistData = db.tbl_LbrTKAProcess.Where(x => x.fld_SuccessStatus == null || x.fld_SuccessStatus == true).ToList();
            var CheckPassportExist = CheckExistData.Where(x => x.fld_NoPassport == fld_NoPassport && (x.fld_SuccessStatus == null || x.fld_SuccessStatus == true)).Count();
            var CheckAppWorkerCount = CheckExistData.Where(x => x.fld_LbrRqstID == tbl_LbrTKAProcess.fld_LbrRqstID && x.fld_SuccessStatus == true).Count();
            var GetBatchInfo = db.tbl_LbrRqst.Find(tbl_LbrTKAProcess.fld_LbrRqstID);
            if (GetBatchInfo.fld_TKAQty > CheckAppWorkerCount)
            {
                if (CheckPassportExist == 0)
                {
                    tbl_LbrTKAProcess.fld_NegaraID = NegaraID;
                    tbl_LbrTKAProcess.fld_SyarikatID = SyarikatID;
                    tbl_LbrTKAProcess.fld_WilayahID = GetBatchInfo.fld_WilayahID;
                    tbl_LbrTKAProcess.fld_LadangID = GetBatchInfo.fld_LadangID;
                    tbl_LbrTKAProcess.fld_CreatedBy = GetUserID;
                    tbl_LbrTKAProcess.fld_CreatedDT = DT;
                    tbl_LbrTKAProcess.fld_ModifiedBy = GetUserID;
                    tbl_LbrTKAProcess.fld_ModifiedDT = DT;
                    tbl_LbrTKAProcess.fld_Nama = tbl_LbrTKAProcess.fld_Nama.ToUpper();
                    tbl_LbrTKAProcess.fld_NoPassport = fld_NoPassport.ToUpper();
                    tbl_LbrTKAProcess.fld_ProcessStatus = 0;
                    if (ModelState.IsValid)
                    {
                        db.tbl_LbrTKAProcess.Add(tbl_LbrTKAProcess);
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
                ModelState.AddModelError("", "Quota TKA already full");
                ViewBag.MsgColor = "color: red";
            }

            fld_SexType = new SelectList(Masterdb.tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text").ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_LbrRqstID = tbl_LbrTKAProcess.fld_LbrRqstID;
            ViewBag.NotApprovedClosed = true;
            return View(tbl_LbrTKAProcess);
        }

        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        public ActionResult _ListOfLabourTKAProcess(long LbrRqstID)
        {
            return View(db.tbl_LbrTKAProcess.Where(x => x.fld_LbrRqstID == LbrRqstID).ToList());
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
            string[] WebConfigFilter = new string[] { "jantina", "successstatus", "unsuccessreasontka" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrTKAProcess tbl_LbrTKAProcess = await db.tbl_LbrTKAProcess.FindAsync(id);
            if (tbl_LbrTKAProcess == null)
            {
                return HttpNotFound();
            }
            var GetBatchNo = db.tbl_LbrRqst.Where(x => x.fld_ID == tbl_LbrTKAProcess.fld_LbrRqstID).Select(s => s.fld_BatchNo).FirstOrDefault();
            fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKAProcess.fld_SexType).ToList();
            fld_SuccessStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "successstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKAProcess.fld_SuccessStatus).ToList();
            fld_UnsuccessReason = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "unsuccessreasontka" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKAProcess.fld_UnsuccessReason).ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_SuccessStatus = fld_SuccessStatus;
            ViewBag.fld_UnsuccessReason = fld_UnsuccessReason;
            ViewBag.GetBatchNo = GetBatchNo;
            return View(tbl_LbrTKAProcess);
        }

        // POST: LabourTKTProcess/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveReject(tbl_LbrTKAProcess tbl_LbrTKAProcess)
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
            string[] WebConfigFilter = new string[] { "jantina", "successstatus", "unsuccessreasontka" };
            var GetLbrTKAProcessData = db.tbl_LbrTKAProcess.Where(x => x.fld_LbrRqstID == tbl_LbrTKAProcess.fld_LbrRqstID).ToList();
            var LbrTKAProcess = GetLbrTKAProcessData.Where(x => x.fld_ID == tbl_LbrTKAProcess.fld_ID).FirstOrDefault();
            var GetBatchInfo = db.tbl_LbrRqst.Find(LbrTKAProcess.fld_LbrRqstID);
            int GetQuota = int.Parse(GetBatchInfo.fld_TKAQty.ToString());
            int GetAppTKA = GetLbrTKAProcessData.Where(x => x.fld_SuccessStatus == true).Count();
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            if (GetQuota >= GetAppTKA)
            {
                LbrTKAProcess.fld_Age = tbl_LbrTKAProcess.fld_Age;
                LbrTKAProcess.fld_Nama = tbl_LbrTKAProcess.fld_Nama.ToUpper();
                LbrTKAProcess.fld_Notes = tbl_LbrTKAProcess.fld_Notes;
                LbrTKAProcess.fld_SexType = tbl_LbrTKAProcess.fld_SexType;
                LbrTKAProcess.fld_ProcessStatus = 3;
                LbrTKAProcess.fld_BOD = tbl_LbrTKAProcess.fld_BOD;
                if (tbl_LbrTKAProcess.fld_SuccessStatus == false)
                {
                    LbrTKAProcess.fld_UnsuccessReason = tbl_LbrTKAProcess.fld_UnsuccessReason;
                }
                else
                {
                    LbrTKAProcess.fld_ArrivedDT = tbl_LbrTKAProcess.fld_ArrivedDT;
                }
                LbrTKAProcess.fld_SuccessStatus = tbl_LbrTKAProcess.fld_SuccessStatus;
                LbrTKAProcess.fld_ModifiedBy = GetUserID;
                LbrTKAProcess.fld_ModifiedDT = DT;
                db.Entry(LbrTKAProcess).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Create", new { BatchNo = GetBatchInfo.fld_BatchNo });
            }
            else
            {
                ModelState.AddModelError("", "Quota TKA already full");
            }
            fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", LbrTKAProcess.fld_SexType).ToList();
            fld_SuccessStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "successstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", LbrTKAProcess.fld_SuccessStatus).ToList();
            fld_UnsuccessReason = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "unsuccessreasontka" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", LbrTKAProcess.fld_UnsuccessReason).ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_SuccessStatus = fld_SuccessStatus;
            ViewBag.fld_UnsuccessReason = fld_UnsuccessReason;
            return View(LbrTKAProcess);
        }

        // GET: LabourTKAProcess/Edit/5
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            string[] WebConfigFilter = new string[] { "jantina", "processstatustka" };
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_ProcessStatus = new List<SelectListItem>();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrTKAProcess tbl_LbrTKAProcess = await db.tbl_LbrTKAProcess.FindAsync(id);
            if (tbl_LbrTKAProcess == null)
            {
                return HttpNotFound();
            }
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            var GetBatchNo = db.tbl_LbrRqst.Where(x => x.fld_ID == tbl_LbrTKAProcess.fld_LbrRqstID).Select(s => s.fld_BatchNo).FirstOrDefault();
            fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKAProcess.fld_SexType).ToList();
            fld_ProcessStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "processstatustka" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false && x.fldOptConfFlag2 == "1").OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKAProcess.fld_ProcessStatus).ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_ProcessStatus = fld_ProcessStatus;
            ViewBag.GetBatchNo = GetBatchNo;
            return View(tbl_LbrTKAProcess);
        }

        // POST: LabourTKAProcess/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(tbl_LbrTKAProcess tbl_LbrTKAProcess)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            string[] WebConfigFilter = new string[] { "jantina", "processstatustka" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_ProcessStatus = new List<SelectListItem>();
            var LbrTKAProcess = await db.tbl_LbrTKAProcess.FindAsync(tbl_LbrTKAProcess.fld_ID);
            var GetBatchInfo = db.tbl_LbrRqst.Find(LbrTKAProcess.fld_LbrRqstID);
            LbrTKAProcess.fld_Age = tbl_LbrTKAProcess.fld_Age;
            LbrTKAProcess.fld_Nama = tbl_LbrTKAProcess.fld_Nama.ToUpper();
            LbrTKAProcess.fld_Notes = tbl_LbrTKAProcess.fld_Notes;
            LbrTKAProcess.fld_SexType = tbl_LbrTKAProcess.fld_SexType;
            LbrTKAProcess.fld_ProcessStatus = tbl_LbrTKAProcess.fld_ProcessStatus;
            LbrTKAProcess.fld_ModifiedBy = GetUserID;
            LbrTKAProcess.fld_ModifiedDT = DT;
            LbrTKAProcess.fld_BOD = tbl_LbrTKAProcess.fld_BOD;
            if (ModelState.IsValid)
            {
                db.Entry(LbrTKAProcess).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Create", new { BatchNo = GetBatchInfo.fld_BatchNo });
            }
            fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKAProcess.fld_SexType).ToList();
            fld_ProcessStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "processstatustka" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrTKAProcess.fld_SexType).ToList();
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_ProcessStatus = fld_ProcessStatus;
            return View(tbl_LbrTKAProcess);
        }

        // GET: LabourTKAProcess/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrTKAProcess tbl_LbrTKAProcess = await db.tbl_LbrTKAProcess.FindAsync(id);
            if (tbl_LbrTKAProcess == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrTKAProcess);
        }

        // POST: LabourTKAProcess/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            tbl_LbrTKAProcess tbl_LbrTKAProcess = await db.tbl_LbrTKAProcess.FindAsync(id);
            db.tbl_LbrTKAProcess.Remove(tbl_LbrTKAProcess);
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
