using MVC_SYSTEM.Attributes;
using MVC_SYSTEM.Class;
using MVC_SYSTEM.LabourModels;
using MVC_SYSTEM.MasterModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3")]
    public class LabourQuotaMenuListController : Controller
    {
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID = 0;
        // GET: LabourQuotaMenuList
        public async Task<ActionResult> Index()
        {
            ViewBag.LabourQuota = "class = active";
            int? getuserid = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, getuserid, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            return View(await Masterdb.tblMenuLists.Where(x=>x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Flag == "LabourQuotaList" && x.fldDeleted == false).ToListAsync());
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