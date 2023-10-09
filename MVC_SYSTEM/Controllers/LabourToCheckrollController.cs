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

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    public class LabourToCheckrollController : Controller
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

        // GET: LobourToCheckroll
        public async Task<ActionResult> Index(Guid? id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            tbl_LbrDataInfo LbrDataInfo = new tbl_LbrDataInfo();

            LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);

            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, LbrDataInfo.fld_WilayahID, LbrDataInfo.fld_SyarikatID, LbrDataInfo.fld_NegaraID, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
            
            List<SelectListItem> fld_CostCenter = new List<SelectListItem>();
            var GetExistingWorker = dbEstate.tbl_Pkjmast.Where(x => x.fld_Nokp == LbrDataInfo.fld_WorkerIDNo && x.fld_NegaraID == LbrDataInfo.fld_NegaraID && x.fld_SyarikatID == LbrDataInfo.fld_SyarikatID && x.fld_WilayahID == LbrDataInfo.fld_WilayahID && x.fld_LadangID == LbrDataInfo.fld_LadangID).FirstOrDefault();
            var GetExistingWorker2 = GetExistingWorker == null ? 0 : 1;
            var CC = GetExistingWorker == null ? "" : GetExistingWorker.fld_KodSAPPekerja;
            fld_CostCenter = new SelectList(Masterdb.tbl_SAPCCPUP.Where(x => x.fld_NegaraID == LbrDataInfo.fld_NegaraID && x.fld_SyarikatID == LbrDataInfo.fld_SyarikatID && x.fld_WilayahID == LbrDataInfo.fld_WilayahID && x.fld_LadangID == LbrDataInfo.fld_LadangID && x.fld_Deleted == false).OrderBy(o => o.fld_CostCenter).Select(s => new SelectListItem { Value = s.fld_CostCenter, Text = s.fld_CostCenter + " - " + s.fld_CostCenterDesc }), "Value", "Text", CC).ToList();
            ViewBag.GetExistingWorker = GetExistingWorker2;
            ViewBag.fld_CostCenter = fld_CostCenter;
            return View(LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(tbl_LbrDataInfo tbl_LbrDataInfo)
        {
            ViewBag.LabourManagement = "class = active";
            string Host1, Catalog1, UserID1, Pass1 = "";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            tbl_LbrDataInfo LbrDataInfo = new tbl_LbrDataInfo();

            string Purpose2 = "CHECKROLL";
            LbrDataInfo = db.tbl_LbrDataInfo.Find(tbl_LbrDataInfo.fld_ID);
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, LbrDataInfo.fld_WilayahID, LbrDataInfo.fld_SyarikatID, LbrDataInfo.fld_NegaraID, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
            LbrDataInfo.fld_WorkerNo = tbl_LbrDataInfo.fld_WorkerNo;
            db.Entry(LbrDataInfo).State = EntityState.Modified;
            await db.SaveChangesAsync();
            ModelState.AddModelError("", "Update Successfully");
            ViewBag.MsgColor = "color: green";

            List<SelectListItem> fld_CostCenter = new List<SelectListItem>();
            var GetExistingWorker = dbEstate.tbl_Pkjmast.Where(x => x.fld_Nokp == LbrDataInfo.fld_WorkerIDNo && x.fld_NegaraID == LbrDataInfo.fld_NegaraID && x.fld_SyarikatID == LbrDataInfo.fld_SyarikatID && x.fld_WilayahID == LbrDataInfo.fld_WilayahID && x.fld_LadangID == LbrDataInfo.fld_LadangID).FirstOrDefault();
            var GetExistingWorker2 = GetExistingWorker == null ? 0 : 1;
            var CC = GetExistingWorker == null ? "" : GetExistingWorker.fld_KodSAPPekerja;
            fld_CostCenter = new SelectList(Masterdb.tbl_SAPCCPUP.Where(x => x.fld_NegaraID == LbrDataInfo.fld_NegaraID && x.fld_SyarikatID == LbrDataInfo.fld_SyarikatID && x.fld_WilayahID == LbrDataInfo.fld_WilayahID && x.fld_LadangID == LbrDataInfo.fld_LadangID && x.fld_Deleted == false).OrderBy(o => o.fld_CostCenter).Select(s => new SelectListItem { Value = s.fld_CostCenter, Text = s.fld_CostCenter + " - " + s.fld_CostCenterDesc }), "Value", "Text", CC).ToList();
            ViewBag.GetExistingWorker = GetExistingWorker2;
            ViewBag.fld_CostCenter = fld_CostCenter;

            return View(LbrDataInfo);
        }
    }
}