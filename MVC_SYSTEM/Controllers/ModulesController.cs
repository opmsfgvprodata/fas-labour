using MVC_SYSTEM.Attributes;
using MVC_SYSTEM.Class;
using MVC_SYSTEM.MasterModels;
using MVC_SYSTEM.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    public class ModulesController : Controller
    {
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private MVC_SYSTEM_MasterModels db = new MVC_SYSTEM_MasterModels();
        private EncryptDecrypt Encrypt = new EncryptDecrypt();
        private ChangeTimeZone timezone = new ChangeTimeZone();
        // GET: Modules
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ModulesSelection()
        {
            int? getuserid = GetIdentity.ID(User.Identity.Name);
            int? NegaraID, SyarikatID, WilayahID, LadangID = 0;
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, getuserid, User.Identity.Name);
            string RedirectLink = "";
            var ModulesUrl = db.tbl_ModulesUrl.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).ToList();

            var user = db.tblUsers.Where(u => u.fldUserID == getuserid).SingleOrDefault();
            var getestateselection = db.tbl_EstateSelection.Where(x => x.fld_UserID == getuserid).FirstOrDefault();
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, getuserid, User.Identity.Name);
            var routeurl = db.tbl_EstateSelection.Where(x => x.fld_UserID == getuserid).Select(s => s.fld_HQUrl).FirstOrDefault();
            string passwordencrypt = Encrypt.Encrypt(user.fldUserPassword);
            string usernameencrypt = Encrypt.Encrypt(user.fldUserName);
            int day = timezone.gettimezone().Day;
            int month = timezone.gettimezone().Month;
            int year = timezone.gettimezone().Year;
            string code = day.ToString() + month.ToString() + year.ToString();
            code = Encrypt.Encrypt(code);

            Response.Cookies.Clear();
            FormsAuthentication.SetAuthCookie(String.Empty, false);
            FormsAuthentication.SignOut();

            string UrlLevel = "";

           if (WilayahID == 0 && LadangID == 0)
            {
                UrlLevel = "HQ";
            }
            else
            {
                UrlLevel = "ESTATE";
            }

            RedirectLink = ModulesUrl.Where(x => x.fld_Module == "Checkroll" && x.fld_LevelAccess == UrlLevel).Select(s => s.fld_Url).FirstOrDefault();
            string modules = "yes";

            RedirectLink = RedirectLink + "/IntegrationLogin?TokenID=" + usernameencrypt + "&PassID=" + passwordencrypt + "&Code=" + code + "&Modules=" + modules;

            return Redirect(RedirectLink);
        }
    }
}