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
using MVC_SYSTEM.EstateModels;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    public class LabourTicketRequestController : Controller
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

        // GET: LabourTicketRequest
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TicketRequestRegister(Guid? id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_FlightCode = new List<SelectListItem>();
            List<SelectListItem> fld_DestinationCode = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "flighttype", "sbbTakAktif" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            var LbrDataInfo = db.tbl_LbrDataInfo.Find(id);

            var GetWayFlight = GetDropdownList.Where(x => x.fldOptConfValue.Contains(LbrDataInfo.fld_InactiveReason) && x.fldOptConfFlag1 == "sbbTakAktif").FirstOrDefault();
            if (GetWayFlight.fldOptConfFlag3 != "noflight")
            {
                if (GetWayFlight.fldOptConfFlag3 == "flightoneway")
                {
                    fld_FlightCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "flighttype" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldOptConfValue == "F01" && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
                }
                else
                {
                    fld_FlightCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "flighttype" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
                }

                var GetDesti = Masterdb.tbl_FlightDestination.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).ToList();
                fld_DestinationCode = new SelectList(GetDesti.OrderBy(o => o.fld_DestinationName).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_DestinationName }), "Value", "Text").ToList();

                var CheckExistingFlightRequest = db.tbl_LbrFlightRequest.Where(x => x.fld_LbrRefID == id && ((x.fld_ApprovedStatus == true && x.fld_Deleted == false) || (x.fld_ApprovedStatus == null && x.fld_Deleted == false))).Count();
                if (CheckExistingFlightRequest > 0)
                {
                    ViewBag.DisableAll = "Yes";
                }

                ViewBag.fld_FlightCode = fld_FlightCode;
                ViewBag.fld_DestinationCode = fld_DestinationCode;

                return View(LbrDataInfo);
            }
            else
            {
                /*  Edited by nana on 29/7 */
                return RedirectToAction("UpdateMenu", "LabourDetail", new { id = LbrDataInfo.fld_ID });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TicketRequestRegister(tbl_LbrFlightRequest tbl_FlightRequest)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            var GetExistingFlightRequest = db.tbl_LbrFlightRequest.Where(x => x.fld_LbrRefID == tbl_FlightRequest.fld_LbrRefID && ((x.fld_ApprovedStatus == null && x.fld_Deleted == false) || (x.fld_ApprovedStatus == true && x.fld_Deleted == false))).Count();
            var GetLabourDetails = db.tbl_LbrDataInfo.Find(tbl_FlightRequest.fld_LbrRefID);
            if (GetExistingFlightRequest == 0)
            {
                if (ModelState.IsValid)
                {
                    string Code = "";
                    Code = GetLabourDetails.fld_WorkerNo + "_" + DT.Day.ToString() + DT.Month.ToString() + DT.Year.ToString() + DT.Hour.ToString() + DT.Minute.ToString() + DT.Second.ToString();

                    if ((tbl_FlightRequest.fld_FlightCode == "F02" && !string.IsNullOrEmpty(tbl_FlightRequest.fld_RequestDT2.ToString())) || (tbl_FlightRequest.fld_FlightCode == "F01" && !string.IsNullOrEmpty(tbl_FlightRequest.fld_RequestDT.ToString())))
                    {
                        tbl_LbrFlightRequest FlightRequest = new tbl_LbrFlightRequest();
                        FlightRequest.fld_LbrRefID = tbl_FlightRequest.fld_LbrRefID;
                        FlightRequest.fld_DestinationCode = tbl_FlightRequest.fld_DestinationCode;
                        FlightRequest.fld_FlightCode = tbl_FlightRequest.fld_FlightCode;
                        FlightRequest.fld_ReasonRequestCode = GetLabourDetails.fld_InactiveReason;
                        FlightRequest.fld_CreatedBy = GetUserID;
                        FlightRequest.fld_CreatedDT = DT;
                        FlightRequest.fld_RequestDT = tbl_FlightRequest.fld_RequestDT;
                        FlightRequest.fld_RequestDT2 = tbl_FlightRequest.fld_RequestDT2;
                        FlightRequest.fld_NegaraID = GetLabourDetails.fld_NegaraID;
                        FlightRequest.fld_SyarikatID = GetLabourDetails.fld_SyarikatID;
                        FlightRequest.fld_WilayahID = GetLabourDetails.fld_WilayahID;
                        FlightRequest.fld_LadangID = GetLabourDetails.fld_LadangID;
                        FlightRequest.fld_ApprovedStatus = null;
                        FlightRequest.fld_Deleted = false;
                        FlightRequest.fld_NotificationCode = Code;
                        db.tbl_LbrFlightRequest.Add(FlightRequest);
                        db.SaveChanges();

                        tbl_LbrNotificationApproval tbl_NotificationApproval = new tbl_LbrNotificationApproval();
                        tbl_NotificationApproval.fld_ApprovalSection = "FLIGHT TICKET APPROVAL";
                        tbl_NotificationApproval.fld_ApprovalLink = Url.Action("ApproveFlightRequest", "LabourTicketRequest", new { id = FlightRequest.fld_ID }, this.Request.Url.Scheme);
                        tbl_NotificationApproval.fld_ApproveAction = false;
                        tbl_NotificationApproval.fld_NotificationCode = Code;
                        tbl_NotificationApproval.fld_OpenAction = false;
                        tbl_NotificationApproval.fld_NegaraID = GetLabourDetails.fld_NegaraID;
                        tbl_NotificationApproval.fld_SyarikatID = GetLabourDetails.fld_SyarikatID;
                        tbl_NotificationApproval.fld_WilayahID = GetLabourDetails.fld_WilayahID;
                        tbl_NotificationApproval.fld_LadangID = GetLabourDetails.fld_LadangID;
                        tbl_NotificationApproval.fld_CreatedDT = DT;
                        tbl_NotificationApproval.fld_CreatedBy = GetUserID;
                        db.tbl_LbrNotificationApproval.Add(tbl_NotificationApproval);
                        db.SaveChanges();


                        ModelState.AddModelError("", "Request Successfully");
                        ViewBag.MsgColor = "color: green";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please Fulfill * Remark");
                        ViewBag.MsgColor = "color: red";
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please Fulfill * Remark");
                    ViewBag.MsgColor = "color: red";
                }
            }
            else
            {
                ModelState.AddModelError("", "Flight Already Requested");
                ViewBag.MsgColor = "color: orange";
            }

            List<SelectListItem> fld_FlightCode = new List<SelectListItem>();
            List<SelectListItem> fld_DestinationCode = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "flighttype", "sbbTakAktif" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            var LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(tbl_FlightRequest.fld_LbrRefID);

            var GetWayFlight = GetDropdownList.Where(x => x.fldOptConfValue.Contains(LbrDataInfo.fld_InactiveReason) && x.fldOptConfFlag1 == "sbbTakAktif").FirstOrDefault();

            if (GetWayFlight.fldOptConfFlag3 == "flightoneway")
            {
                fld_FlightCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "flighttype" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldOptConfValue == "F01" && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            }
            else
            {
                fld_FlightCode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "flighttype" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text").ToList();
            }

            var GetDesti = Masterdb.tbl_FlightDestination.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).ToList();
            fld_DestinationCode = new SelectList(GetDesti.OrderBy(o => o.fld_DestinationName).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_DestinationName }), "Value", "Text").ToList();

            var CheckExistingFlightRequest = db.tbl_LbrFlightRequest.Where(x => x.fld_LbrRefID == tbl_FlightRequest.fld_LbrRefID && ((x.fld_ApprovedStatus == true && x.fld_Deleted == false) || (x.fld_ApprovedStatus == null && x.fld_Deleted == false))).Count();
            if (CheckExistingFlightRequest > 0)
            {
                ViewBag.DisableAll = "Yes";
            }

            ViewBag.fld_FlightCode = fld_FlightCode;
            ViewBag.fld_DestinationCode = fld_DestinationCode;

            return View(LbrDataInfo);
        }

        public ActionResult _LabourFlightRequestDetail(Guid LabourID)
        {
            return View(db.vw_LbrFlightRequest.Where(x => x.fld_LbrRefID == LabourID).ToList());
        }

        // GET: HQLabourQuota/Delete/5
        public async Task<ActionResult> CancelFlightRequest(int id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vw_LbrFlightRequest vw_LbrFlightRequest = await db.vw_LbrFlightRequest.FindAsync(id);
            if (vw_LbrFlightRequest == null)
            {
                return HttpNotFound();
            }

            return View(vw_LbrFlightRequest);
        }

        // POST: HQLabourQuota/Delete/5
        [HttpPost, ActionName("CancelFlightRequest")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelFlightRequestConfirmed(int id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            tbl_LbrFlightRequest tbl_LbrFlightRequest = await db.tbl_LbrFlightRequest.FindAsync(id);
            tbl_LbrFlightRequest.fld_Deleted = true;
            tbl_LbrFlightRequest.fld_DeletedDT = DT;
            tbl_LbrFlightRequest.fld_DeletedBy = GetUserID;
            db.Entry(tbl_LbrFlightRequest).State = EntityState.Modified;
            await db.SaveChangesAsync();

            var GetLabourNotification = db.tbl_LbrNotificationApproval.Where(x => x.fld_NotificationCode == tbl_LbrFlightRequest.fld_NotificationCode).FirstOrDefault();
            GetLabourNotification.fld_OpenAction = true;
            GetLabourNotification.fld_ApproveAction = true;
            db.Entry(GetLabourNotification).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("TicketRequestRegister", new { id = tbl_LbrFlightRequest.fld_LbrRefID });
        }

        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User")]
        public async Task<ActionResult> ApproveFlightRequest(int id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);

            string[] WebConfigFilter = new string[] { "approvedstatus" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vw_LbrFlightRequest vw_LbrFlightRequest = await db.vw_LbrFlightRequest.FindAsync(id);
            if (vw_LbrFlightRequest == null)
            {
                return HttpNotFound();
            }
            List<SelectListItem> fld_ApprovedStatus = new List<SelectListItem>();
            fld_ApprovedStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "approvedstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text").ToList();
            ViewBag.fld_ApprovedStatus = fld_ApprovedStatus;

            return View(vw_LbrFlightRequest);
        }

        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveFlightRequest(vw_LbrFlightRequest vw_LbrFlightRequest)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            tbl_LbrFlightRequest tbl_LbrFlightRequest = await db.tbl_LbrFlightRequest.FindAsync(vw_LbrFlightRequest.fld_ID);
            tbl_LbrFlightRequest.fld_ApprovedStatus = vw_LbrFlightRequest.fld_ApprovedStatus;
            tbl_LbrFlightRequest.fld_ApprovedDT = DT;
            tbl_LbrFlightRequest.fld_ApprovedBy = GetUserID;
            db.Entry(tbl_LbrFlightRequest).State = EntityState.Modified;
            await db.SaveChangesAsync();

            var GetLabourNotification = db.tbl_LbrNotificationApproval.Where(x => x.fld_NotificationCode == tbl_LbrFlightRequest.fld_NotificationCode).FirstOrDefault();
            GetLabourNotification.fld_OpenAction = true;
            GetLabourNotification.fld_ApproveAction = true;
            db.Entry(GetLabourNotification).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("TicketRequestRegister", new { id = tbl_LbrFlightRequest.fld_LbrRefID });
        }

        [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
        public ActionResult AddFlightDestination(Guid? ID)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);

            Session["ID"] = ID;

            return View();
        }
    }
}