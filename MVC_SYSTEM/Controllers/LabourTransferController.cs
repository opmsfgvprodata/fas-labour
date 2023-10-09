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
using MVC_SYSTEM.Attributes;
using MVC_SYSTEM.EstateModels;
using MVC_SYSTEM.log;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
    public class LabourTransferController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private GetConfig GetConfig = new GetConfig();
        private GeneralFunc GeneralFunc = new GeneralFunc();
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;
        private errorlog geterror = new errorlog();

        // GET: LabourTransfer
        public async Task<ActionResult> Index(Guid? id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            tbl_LbrDataInfo LbrDataInfo = new tbl_LbrDataInfo();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            string Code = "";

            LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            Code = LbrDataInfo.fld_WorkerNo + "_" + DT.Day.ToString() + DT.Month.ToString() + DT.Year.ToString() + DT.Hour.ToString() + DT.Minute.ToString() + DT.Second.ToString();

            var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
            fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName", LbrDataInfo.fld_WilayahID).ToList();
            fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == LbrDataInfo.fld_WilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text", LbrDataInfo.fld_LadangID).ToList();

            ViewBag.fld_WilayahIDTo = fld_WilayahID;
            ViewBag.fld_LadangIDTo = fld_LadangID;
            ViewBag.Code = Code;

            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(tbl_LbrDataInfo tbl_LbrDataInfo, string TransCode, int fld_WilayahIDTo, int fld_LadangIDTo)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            tbl_LbrDataInfo LbrDataInfo = new tbl_LbrDataInfo();
            tbl_LbrTrnsferHistory LbrTrnsferHistory = new tbl_LbrTrnsferHistory();
            string Code = "";
            Code = LbrDataInfo.fld_WorkerNo + "_" + DT.Day.ToString() + DT.Month.ToString() + DT.Year.ToString() + DT.Hour.ToString() + DT.Minute.ToString() + DT.Second.ToString();

            LbrDataInfo = db.tbl_LbrDataInfo.Find(tbl_LbrDataInfo.fld_ID);
            List<SelectListItem> fld_WilayahID2 = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID2 = new List<SelectListItem>();

            var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
            try
            {

                if (LbrDataInfo.fld_LadangID != fld_LadangIDTo)
                {
                    LbrDataInfo.fld_WorkerTransferCode = TransCode;
                    LbrDataInfo.fld_TransferToChckrollWorkerTransferStatus = false;
                    //modified by faeza 30.05.2021
                    if (LbrDataInfo.fld_PaymentMode == null)
                    {
                        ModelState.AddModelError("", "Update Unsuccessfully. Please check Worker Personal Detail");
                        ViewBag.MsgColor = "color: red";
                    }
                    else
                    {
                        db.Entry(LbrDataInfo).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        LbrTrnsferHistory.fld_LbrRefID = LbrDataInfo.fld_ID;
                        LbrTrnsferHistory.fld_WorkerTransferCode = TransCode;
                        LbrTrnsferHistory.fld_OldWorkerNo = LbrDataInfo.fld_WorkerNo;
                        LbrTrnsferHistory.fld_NegaraIDFrom = LbrDataInfo.fld_NegaraID;
                        LbrTrnsferHistory.fld_SyarikatIDFrom = LbrDataInfo.fld_SyarikatID;
                        LbrTrnsferHistory.fld_WilayahIDFrom = LbrDataInfo.fld_WilayahID;
                        LbrTrnsferHistory.fld_LadangIDFrom = LbrDataInfo.fld_LadangID;
                        LbrTrnsferHistory.fld_NegaraIDTo = NegaraID;
                        LbrTrnsferHistory.fld_SyarikatIDTo = SyarikatID;
                        LbrTrnsferHistory.fld_WilayahIDTo = fld_WilayahIDTo;
                        LbrTrnsferHistory.fld_LadangIDTo = fld_LadangIDTo;
                        LbrTrnsferHistory.fld_CreatedBy = GetUserID;
                        LbrTrnsferHistory.fld_CreatedDT = DT;
                        db.tbl_LbrTrnsferHistory.Add(LbrTrnsferHistory);
                        db.SaveChanges();

                        ModelState.AddModelError("", "Update Successfully");
                        ViewBag.MsgColor = "color: green";
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Transfer to same estate");
                    ViewBag.MsgColor = "color: orange";
                }

                fld_WilayahID2 = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName", LbrDataInfo.fld_WilayahID).ToList();
                fld_LadangID2 = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == fld_WilayahIDTo).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text", LbrDataInfo.fld_LadangID).ToList();

                ViewBag.fld_WilayahIDTo = fld_WilayahID2;
                ViewBag.fld_LadangIDTo = fld_LadangID2;
                ViewBag.Code = Code;

            }
            catch (Exception ex)
            {
                geterror.catcherro(ex.Message, ex.StackTrace, ex.Source, ex.TargetSite.ToString());
                //return View(LbrDataInfo);
            }
            return View(LbrDataInfo);

        }

        public ActionResult _LabourTransferDetail(string WorkerTransferCode)
        {
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            return View(db.vw_LbrTrnsferData.Where(x => x.fld_NegaraIDFrom == NegaraID && x.fld_SyarikatIDFrom == SyarikatID && x.fld_WorkerTransferCode == WorkerTransferCode).FirstOrDefault());
        }

        public async Task<ActionResult> TransferLabourDetail(Guid? id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            vw_LbrTrnsferData LbrTrnsferData = new vw_LbrTrnsferData();

            LbrTrnsferData = await db.vw_LbrTrnsferData.FindAsync(id);

            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, LbrTrnsferData.fld_WilayahIDTo, LbrTrnsferData.fld_SyarikatIDTo, LbrTrnsferData.fld_NegaraIDTo, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);

            List<SelectListItem> fld_CostCenter = new List<SelectListItem>();
            var GetExistingWorker = dbEstate.tbl_Pkjmast.Where(x => x.fld_Nopkj == LbrTrnsferData.fld_NewWorkerNo && x.fld_NegaraID == LbrTrnsferData.fld_NegaraIDTo && x.fld_SyarikatID == LbrTrnsferData.fld_SyarikatIDTo && x.fld_WilayahID == LbrTrnsferData.fld_WilayahIDTo && x.fld_LadangID == LbrTrnsferData.fld_LadangIDTo).FirstOrDefault();
            var CC = GetExistingWorker == null ? "" : GetExistingWorker.fld_KodSAPPekerja;
            fld_CostCenter = new SelectList(Masterdb.tbl_SAPCCPUP.Where(x => x.fld_NegaraID == LbrTrnsferData.fld_NegaraIDTo && x.fld_SyarikatID == LbrTrnsferData.fld_SyarikatIDTo && x.fld_WilayahID == LbrTrnsferData.fld_WilayahIDTo && x.fld_LadangID == LbrTrnsferData.fld_LadangIDTo && x.fld_Deleted == false).OrderBy(o => o.fld_CostCenter).Select(s => new SelectListItem { Value = s.fld_CostCenter, Text = s.fld_CostCenter + " - " + s.fld_CostCenterDesc }), "Value", "Text", CC).ToList();
            var GetExistingWorker2 = GetExistingWorker == null ? 0 : 1;
            ViewBag.GetExistingWorker = GetExistingWorker2;
            ViewBag.fld_CostCenter = fld_CostCenter;

            return View(LbrTrnsferData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TransferLabourDetail(tbl_LbrTrnsferHistory tbl_LbrTrnsferHistory)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            tbl_LbrTrnsferHistory LbrTrnsferHistory = new tbl_LbrTrnsferHistory();


            LbrTrnsferHistory = db.tbl_LbrTrnsferHistory.Find(tbl_LbrTrnsferHistory.fld_ID);
            LbrTrnsferHistory.fld_NewWorkerNo = tbl_LbrTrnsferHistory.fld_NewWorkerNo;
            db.Entry(LbrTrnsferHistory).State = EntityState.Modified;
            await db.SaveChangesAsync();
            ModelState.AddModelError("", "Update Successfully");
            ViewBag.MsgColor = "color: green";

            vw_LbrTrnsferData LbrTrnsferData = new vw_LbrTrnsferData();

            LbrTrnsferData = await db.vw_LbrTrnsferData.FindAsync(tbl_LbrTrnsferHistory.fld_ID);

            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, LbrTrnsferData.fld_WilayahIDTo, LbrTrnsferData.fld_SyarikatIDTo, LbrTrnsferData.fld_NegaraIDTo, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
           
            List<SelectListItem> fld_CostCenter = new List<SelectListItem>();
            var GetExistingWorker = dbEstate.tbl_Pkjmast.Where(x => x.fld_Nopkj == LbrTrnsferData.fld_NewWorkerNo && x.fld_NegaraID == LbrTrnsferData.fld_NegaraIDTo && x.fld_SyarikatID == LbrTrnsferData.fld_SyarikatIDTo && x.fld_WilayahID == LbrTrnsferData.fld_WilayahIDTo && x.fld_LadangID == LbrTrnsferData.fld_LadangIDTo).FirstOrDefault();
            var CC = GetExistingWorker == null ? "" : GetExistingWorker.fld_KodSAPPekerja;
            fld_CostCenter = new SelectList(Masterdb.tbl_SAPCCPUP.Where(x => x.fld_NegaraID == LbrTrnsferData.fld_NegaraIDTo && x.fld_SyarikatID == LbrTrnsferData.fld_SyarikatIDTo && x.fld_WilayahID == LbrTrnsferData.fld_WilayahIDTo && x.fld_LadangID == LbrTrnsferData.fld_LadangIDTo && x.fld_Deleted == false).OrderBy(o => o.fld_CostCenter).Select(s => new SelectListItem { Value = s.fld_CostCenter, Text = s.fld_CostCenter + " - " + s.fld_CostCenterDesc }), "Value", "Text", CC).ToList();
            var GetExistingWorker2 = GetExistingWorker == null ? 0 : 1;
            ViewBag.GetExistingWorker = GetExistingWorker2;
            ViewBag.fld_CostCenter = fld_CostCenter;

            return View(LbrTrnsferData);
        }
    }
}