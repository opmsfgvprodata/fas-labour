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
    public class LabourApprovedRequestController : Controller
    {
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private ChangeTimeZone ChangeTimeZone = new ChangeTimeZone();
        private GetConfig GetConfig = new GetConfig();
        private List<tbl_LbrRqst> LbrRqst = new List<tbl_LbrRqst>();
        private GeneralFunc GeneralFunc = new GeneralFunc();
        DateTime DT = new DateTime();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;

        // GET: LabourApprovedRequest
        public async Task<ActionResult> Index(short? Year, int? WilayahIDList, int? LadangIDList)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            int YearRange = 0;
            int CurrentYear = 0;
            int? GetWilayahID = 0;
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
            fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            if (WilayahID == 0 && LadangID == 0)
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();
                fld_WilayahID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));



                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();

                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == GetWilayahID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

            }
            else
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_ID == WilayahID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
          

                var GetTopWilayahID = GetWilayahData.Take(1).FirstOrDefault();
                if (WilayahIDList != null)
                {
                    GetWilayahID = WilayahIDList;
                }
                else
                {
                    GetWilayahID = GetTopWilayahID.fld_ID;
                }

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == WilayahID && x.fld_ID == LadangID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
            }
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;

            if (WilayahID == 0 && LadangID == 0)
                if (string.IsNullOrEmpty(""))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {
                        var GetApprovalTKT = db.tbl_LbrTKTProcess.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID  && x.fld_SuccessStatus == true).Select(s => s.fld_LbrRqstID).Distinct().ToList();
                        var GetApprovalTKA = db.tbl_LbrTKAProcess.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID  && x.fld_SuccessStatus == true).Select(s => s.fld_LbrRqstID).Distinct().ToList();
                        LbrRqst = await db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID  && x.fld_Year == Year && (GetApprovalTKT.Contains(x.fld_ID) || GetApprovalTKA.Contains(x.fld_ID)) && x.fld_Deleted == false).OrderByDescending(o => new { o.fld_ApprovedStatus, o.fld_Month, o.fld_CreatedDT }).ToListAsync();
                        ViewBag.GetApprovalTKTCount = GetApprovalTKT.Count;
                        ViewBag.GetApprovalTKACount = GetApprovalTKA.Count;
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0)
                    {
                        var GetApprovalTKT = db.tbl_LbrTKTProcess.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_SuccessStatus == true).Select(s => s.fld_LbrRqstID).Distinct().ToList();
                        var GetApprovalTKA = db.tbl_LbrTKAProcess.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_SuccessStatus == true).Select(s => s.fld_LbrRqstID).Distinct().ToList();
                        LbrRqst = await db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_Year == Year && (GetApprovalTKT.Contains(x.fld_ID) || GetApprovalTKA.Contains(x.fld_ID)) && x.fld_Deleted == false).OrderByDescending(o => new { o.fld_ApprovedStatus, o.fld_Month, o.fld_CreatedDT }).ToListAsync();
                        ViewBag.GetApprovalTKTCount = GetApprovalTKT.Count;
                        ViewBag.GetApprovalTKACount = GetApprovalTKA.Count;
                    }
                    else
                    {
                        var GetApprovalTKT = db.tbl_LbrTKTProcess.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_SuccessStatus == true).Select(s => s.fld_LbrRqstID).Distinct().ToList();
                        var GetApprovalTKA = db.tbl_LbrTKAProcess.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_SuccessStatus == true).Select(s => s.fld_LbrRqstID).Distinct().ToList();
                        LbrRqst = await db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Year == Year && (GetApprovalTKT.Contains(x.fld_ID) || GetApprovalTKA.Contains(x.fld_ID)) && x.fld_Deleted == false).OrderByDescending(o => new { o.fld_ApprovedStatus, o.fld_Month, o.fld_CreatedDT }).ToListAsync();
                        ViewBag.GetApprovalTKTCount = GetApprovalTKT.Count;
                        ViewBag.GetApprovalTKACount = GetApprovalTKA.Count;
                    }
            }
            else
            {
                    var GetApprovalTKT = db.tbl_LbrTKTProcess.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_SuccessStatus == true).Select(s => s.fld_LbrRqstID).Distinct().ToList();
                    var GetApprovalTKA = db.tbl_LbrTKAProcess.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_SuccessStatus == true).Select(s => s.fld_LbrRqstID).Distinct().ToList();
                    LbrRqst = await db.tbl_LbrRqst.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_Year == Year && (GetApprovalTKT.Contains(x.fld_ID) || GetApprovalTKA.Contains(x.fld_ID)) && x.fld_Deleted == false).OrderByDescending(o => new { o.fld_ApprovedStatus, o.fld_Month, o.fld_CreatedDT }).ToListAsync();
                    ViewBag.GetApprovalTKTCount = GetApprovalTKT.Count;
                    ViewBag.GetApprovalTKACount = GetApprovalTKA.Count;
                }
            //Commented by Shazana on 17/8
            //return View(LbrRqst.OrderByDescending(x => x.fld_BatchNo));
            //Added by Shazana on 17/8
            return View(LbrRqst.OrderByDescending(x => x.fld_BatchNo));
        }

        public async Task<ActionResult> TKTInformation(string BatchNo)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            var GetStatusBatchNo = db.tbl_LbrRqst.Where(x => x.fld_BatchNo == BatchNo && x.fld_ApprovedStatus == 1 && x.fld_ProcessStatus != 99).FirstOrDefault();
            var batchno = GetStatusBatchNo == null ? 0 : GetStatusBatchNo.fld_AppReqQty;

            //Commented by Shazana on 17/8  
            //return View(await db.tbl_LbrTKTProcess.OrderBy(o => new {o.fld_NoIC,o.fld_Nama}).ToListAsync());

            //Added By Shazana on 17/8
            //Commented by Shazana on 31/3/2021
            //var fld_idBatchNo = db.tbl_LbrRqst.Where(x => x.fld_BatchNo == BatchNo).Select(s=> s.fld_ID).FirstOrDefault();

            //Added by Shazana 31/3/2021
            var fld_idBatchNoolatest = (from t in db.tbl_LbrRqst
                                        where t.fld_BatchNo == BatchNo
                                        orderby t.fld_ID descending
                                        select t).Take(1).FirstOrDefault();
            return View(await db.tbl_LbrTKTProcess.Where(x => x.fld_LbrRqstID == fld_idBatchNoolatest.fld_ID).OrderBy(o => new { o.fld_NoIC, o.fld_Nama }).ToListAsync());
            //Close Added by Shazana 31/3/2021
            //Commented by Shazana on 31/3/2021
            //return View(await db.tbl_LbrTKTProcess.Where(x => x.fld_LbrRqstID == fld_idBatchNo).OrderBy(o => new { o.fld_NoIC, o.fld_Nama }).ToListAsync());

        }

        public async Task<ActionResult> TKTOfferLetterCreate(Guid? id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);

            var GetTKTInfo = await db.tbl_LbrTKTProcess.FindAsync(id);

            return View(GetTKTInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TKTOfferLetterCreate(tbl_LbrTKTOfrLtr tbl_LbrTKTOfrLtr)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            var GetLabourDetails = db.tbl_LbrTKTOfrLtr.Where(x=>x.fld_LbrRefID == tbl_LbrTKTOfrLtr.fld_LbrRefID).ToList();
            if (GetLabourDetails.Count == 0)
            {
                if (ModelState.IsValid)
                {
                    tbl_LbrTKTOfrLtr LbrTKTOfrLtr = new tbl_LbrTKTOfrLtr();
                    LbrTKTOfrLtr.fld_LbrRefID = tbl_LbrTKTOfrLtr.fld_LbrRefID;
                    LbrTKTOfrLtr.fld_LtrAdd = tbl_LbrTKTOfrLtr.fld_LtrAdd;
                    LbrTKTOfrLtr.fld_DailyPayRate = tbl_LbrTKTOfrLtr.fld_DailyPayRate;
                    LbrTKTOfrLtr.fld_WorkingSrtDT = tbl_LbrTKTOfrLtr.fld_WorkingSrtDT;
                    LbrTKTOfrLtr.fld_CreatedBy = GetUserID;
                    LbrTKTOfrLtr.fld_CreatedDT = DT;

                    db.tbl_LbrTKTOfrLtr.Add(LbrTKTOfrLtr);
                    db.SaveChanges();
                    ModelState.AddModelError("", "Update Successfully");
                    ViewBag.MsgColor = "color: green";
                }
                else
                {
                    ModelState.AddModelError("", "Please Fulfill * Remark");
                    ViewBag.MsgColor = "color: red";
                }
            }
            var GetTKTInfo = await db.tbl_LbrTKTProcess.FindAsync(tbl_LbrTKTOfrLtr.fld_LbrRefID);
            return View(GetTKTInfo);
        }

        public ActionResult _TKTOfferLetterDetail(Guid LabourID)
        {
            return View(db.vw_LbrTKTOffrLetter.Where(x => x.fld_LbrRefID == LabourID).ToList());
        }

        public ActionResult TKTOfferLetter(int id)
        {
            return View(db.vw_LbrTKTOffrLetter.Where(x => x.fld_ID == id).FirstOrDefault());
        }

        public async Task<ActionResult> TKTFullInformation(Guid? id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            tbl_LbrDataInfo tbl_LbrDataInfo = new tbl_LbrDataInfo();
            string[] WebConfigFilter = new string[] { "jantina", "tarafKahwin", "bangsa", "agama", "krytnlist", "negeri", "jnsPkj", "designation", "statusaktif", "sbbTakAktif", "designation", "paymentmode" };//modified by faeza 20.04.2021
            List<SelectListItem> fld_State = new List<SelectListItem>();
            List<SelectListItem> fld_Country = new List<SelectListItem>();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_Race = new List<SelectListItem>();
            List<SelectListItem> fld_Religion = new List<SelectListItem>();
            List<SelectListItem> fld_Nationality = new List<SelectListItem>();
            List<SelectListItem> fld_MarriedStatus = new List<SelectListItem>();
            List<SelectListItem> fld_WorkerType = new List<SelectListItem>();
            List<SelectListItem> fld_WorkCtgry = new List<SelectListItem>();
            List<SelectListItem> fld_FeldaRelated = new List<SelectListItem>();
            List<SelectListItem> fld_ActiveStatus = new List<SelectListItem>();
            List<SelectListItem> fld_InactiveReason = new List<SelectListItem>();
            List<SelectListItem> fld_PaymentMode = new List<SelectListItem>();//add by faeza 20.04.2021

            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            var GetDetailFromProcess = db.tbl_LbrTKTProcess.Find(id);
            var GetBatchNo = db.tbl_LbrRqst.Where(x => x.fld_ID == GetDetailFromProcess.fld_LbrRqstID).Select(s => s.fld_BatchNo).FirstOrDefault();
            tbl_LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_LbrProcessID == GetDetailFromProcess.fld_ID && x.fld_Nationality == "MA").FirstOrDefaultAsync();
            if (tbl_LbrDataInfo == null)
            {
                tbl_LbrDataInfo = new tbl_LbrDataInfo();
                tbl_LbrDataInfo.fld_WorkerIDNo = GetDetailFromProcess.fld_NoIC;
                tbl_LbrDataInfo.fld_WorkerName = GetDetailFromProcess.fld_Nama;
                tbl_LbrDataInfo.fld_BOD = GetDetailFromProcess.fld_BOD;
                tbl_LbrDataInfo.fld_Age = GetDetailFromProcess.fld_Age;
                tbl_LbrDataInfo.fld_PhoneNo = GetDetailFromProcess.fld_PhoneNo;
                tbl_LbrDataInfo.fld_LbrProcessID = GetDetailFromProcess.fld_ID;
            }

            fld_State = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_State).ToList();
            fld_Country = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue == "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Country).ToList();
            fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_SexType).ToList();
            fld_Race = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "bangsa" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Race).ToList();
            fld_Religion = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "agama" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Religion).ToList();
            fld_Nationality = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue == "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Nationality).ToList();
            fld_MarriedStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "tarafKahwin" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_MarriedStatus).ToList();
            fld_WorkerType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jnsPkj" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkerType).ToList();
            fld_ActiveStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_ActiveStatus).ToList();
            fld_InactiveReason = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "sbbTakAktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_InactiveReason).ToList();
            fld_WorkCtgry = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "designation" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkCtgry).ToList();
            fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();//added by faeza 20.04.2021

            ViewBag.fld_State = fld_State;
            ViewBag.fld_Country = fld_Country;
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_Race = fld_Race;
            ViewBag.fld_Religion = fld_Religion;
            ViewBag.fld_Nationality = fld_Nationality;
            ViewBag.fld_MarriedStatus = fld_MarriedStatus;
            ViewBag.fld_WorkerType = fld_WorkerType;
            ViewBag.fld_FeldaRelated = fld_FeldaRelated;
            ViewBag.fld_ActiveStatus = fld_ActiveStatus;
            ViewBag.fld_InactiveReason = fld_InactiveReason;
            ViewBag.fld_WorkCtgry = fld_WorkCtgry;
            ViewBag.fld_PaymentMode = fld_PaymentMode;//added by faeza 20.04.2021
            ViewBag.GetBatchNo = GetBatchNo;

            ViewBag.ProccessID = GetDetailFromProcess.fld_ID;
            return View(tbl_LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TKTFullInformation(tbl_LbrDataInfo tbl_LbrDataInfo)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            tbl_LbrDataInfo LbrDataInfo = new tbl_LbrDataInfo();

            string[] WebConfigFilter = new string[] { "jantina", "tarafKahwin", "bangsa", "agama", "krytnlist", "negeri", "jnsPkj", "designation", "statusaktif", "sbbTakAktif", "designation", "paymentmode" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            List<SelectListItem> fld_State = new List<SelectListItem>();
            List<SelectListItem> fld_Country = new List<SelectListItem>();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_Race = new List<SelectListItem>();
            List<SelectListItem> fld_Religion = new List<SelectListItem>();
            List<SelectListItem> fld_Nationality = new List<SelectListItem>();
            List<SelectListItem> fld_MarriedStatus = new List<SelectListItem>();
            List<SelectListItem> fld_WorkerType = new List<SelectListItem>();
            List<SelectListItem> fld_WorkCtgry = new List<SelectListItem>();
            List<SelectListItem> fld_FeldaRelated = new List<SelectListItem>();
            List<SelectListItem> fld_ActiveStatus = new List<SelectListItem>();
            List<SelectListItem> fld_InactiveReason = new List<SelectListItem>();
            List<SelectListItem> fld_PaymentMode = new List<SelectListItem>();//added by faeza 20.04.2021

            //try 
            //{
            LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_LbrProcessID == tbl_LbrDataInfo.fld_LbrProcessID && x.fld_Nationality == "MA").FirstOrDefault();
            //var noworker = db.tbl_LbrDataInfo.Where(x => x.fld_LbrProcessID == tbl_LbrDataInfo.fld_LbrProcessID && x.fld_Nationality == "MA").Select(s => s.fld_WorkerNo).FirstOrDefault();
            var GetLbrRqstDetails = db.tbl_LbrTKTProcess.Find(tbl_LbrDataInfo.fld_LbrProcessID);
            var GetBatchNo = db.tbl_LbrRqst.Where(x => x.fld_ID == GetLbrRqstDetails.fld_LbrRqstID).Select(s => s.fld_BatchNo).FirstOrDefault();

            if (ModelState.IsValid)
            {
                if (LbrDataInfo == null)
                {
                    tbl_LbrDataInfo.fld_NegaraID = NegaraID;
                    tbl_LbrDataInfo.fld_SyarikatID = SyarikatID;
                    tbl_LbrDataInfo.fld_WilayahID = GetLbrRqstDetails.fld_WilayahID;
                    tbl_LbrDataInfo.fld_LadangID = GetLbrRqstDetails.fld_LadangID;
                    tbl_LbrDataInfo.fld_CreatedBy = GetUserID;
                    tbl_LbrDataInfo.fld_CreatedDT = DT;
                    tbl_LbrDataInfo.fld_ModifiedBy = GetUserID;
                    tbl_LbrDataInfo.fld_ModifiedDT = DT;
                    tbl_LbrDataInfo.fld_DivisionID = null;
                    tbl_LbrDataInfo.fld_FeldaRelated = "0";
                    tbl_LbrDataInfo.fld_ActiveStatus = "1";
                    tbl_LbrDataInfo.fld_InactiveReason = null;
                    tbl_LbrDataInfo.fld_InactiveDT = null;
                    tbl_LbrDataInfo.fld_TransferToChckrollStatus = null;
                    db.tbl_LbrDataInfo.Add(tbl_LbrDataInfo);
                    await db.SaveChangesAsync();
                    LbrDataInfo = new tbl_LbrDataInfo();
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_LbrProcessID == tbl_LbrDataInfo.fld_LbrProcessID && x.fld_Nationality == "MA").FirstOrDefault();
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Find(tbl_LbrDataInfo.fld_ID);
                    ModelState.AddModelError("", "Update Successfully");
                    ViewBag.MsgColor = "color: green";

                }
                else
                {
                    LbrDataInfo.fld_BOD = tbl_LbrDataInfo.fld_BOD;
                    LbrDataInfo.fld_Age = tbl_LbrDataInfo.fld_Age;
                    LbrDataInfo.fld_Country = tbl_LbrDataInfo.fld_Country;
                    LbrDataInfo.fld_MarriedStatus = tbl_LbrDataInfo.fld_MarriedStatus;
                    LbrDataInfo.fld_ModifiedBy = GetUserID;
                    LbrDataInfo.fld_ModifiedDT = DT;
                    LbrDataInfo.fld_Notes = tbl_LbrDataInfo.fld_Notes;
                    LbrDataInfo.fld_PhoneNo = tbl_LbrDataInfo.fld_PhoneNo;
                    LbrDataInfo.fld_Nationality = tbl_LbrDataInfo.fld_Nationality;
                    LbrDataInfo.fld_Postcode = tbl_LbrDataInfo.fld_Postcode;
                    LbrDataInfo.fld_Race = tbl_LbrDataInfo.fld_Race;
                    LbrDataInfo.fld_Religion = tbl_LbrDataInfo.fld_Religion;
                    LbrDataInfo.fld_SexType = tbl_LbrDataInfo.fld_SexType;
                    LbrDataInfo.fld_State = tbl_LbrDataInfo.fld_State;
                    LbrDataInfo.fld_WorkCtgry = tbl_LbrDataInfo.fld_WorkCtgry;
                    LbrDataInfo.fld_WorkerAddress = tbl_LbrDataInfo.fld_WorkerAddress;
                    LbrDataInfo.fld_WorkerName = tbl_LbrDataInfo.fld_WorkerName.ToUpper();
                    LbrDataInfo.fld_WorkerType = tbl_LbrDataInfo.fld_WorkerType;
                    LbrDataInfo.fld_WorkingStartDT = tbl_LbrDataInfo.fld_WorkingStartDT;
                    LbrDataInfo.fld_ConfirmationDT = tbl_LbrDataInfo.fld_ConfirmationDT;
                    LbrDataInfo.fld_ActiveStatus = tbl_LbrDataInfo.fld_ActiveStatus;
                    LbrDataInfo.fld_WorkerIDNo = tbl_LbrDataInfo.fld_WorkerIDNo;
                    //LbrDataInfo.fld_WorkerNo = noworker;
                    LbrDataInfo.fld_WorkerNo = tbl_LbrDataInfo.fld_WorkerNo;
                    LbrDataInfo.fld_ActiveStatus = "1";
                    LbrDataInfo.fld_InactiveReason = null;
                    LbrDataInfo.fld_InactiveDT = null;
                    LbrDataInfo.fld_PaymentMode = tbl_LbrDataInfo.fld_PaymentMode;//added by faeza 20.04.2021
                    LbrDataInfo.fld_Last4Pan = tbl_LbrDataInfo.fld_Last4Pan;//added by faeza 20.04.2021
                    db.Entry(LbrDataInfo).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    LbrDataInfo = new tbl_LbrDataInfo();
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_LbrProcessID == tbl_LbrDataInfo.fld_LbrProcessID && x.fld_Nationality == "MA").FirstOrDefault();
                    ModelState.AddModelError("", "Update Successfully");
                    ViewBag.MsgColor = "color: green";
                }

                fld_State = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_State).ToList();
                fld_Country = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue == "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Country).ToList();
                fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_SexType).ToList();
                fld_Race = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "bangsa" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Race).ToList();
                fld_Religion = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "agama" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Religion).ToList();
                fld_Nationality = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue == "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Nationality).ToList();
                fld_MarriedStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "tarafKahwin" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_MarriedStatus).ToList();
                fld_WorkerType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jnsPkj" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkerType).ToList();
                fld_ActiveStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_ActiveStatus).ToList();
                fld_InactiveReason = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "sbbTakAktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_InactiveReason).ToList();
                fld_WorkCtgry = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "designation" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkCtgry).ToList();
                fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();//added by faeza 20.04.2021

                ViewBag.fld_State = fld_State;
                ViewBag.fld_Country = fld_Country;
                ViewBag.fld_SexType = fld_SexType;
                ViewBag.fld_Race = fld_Race;
                ViewBag.fld_Religion = fld_Religion;
                ViewBag.fld_Nationality = fld_Nationality;
                ViewBag.fld_MarriedStatus = fld_MarriedStatus;
                ViewBag.fld_WorkerType = fld_WorkerType;
                ViewBag.fld_FeldaRelated = fld_FeldaRelated;
                ViewBag.fld_ActiveStatus = fld_ActiveStatus;
                ViewBag.fld_InactiveReason = fld_InactiveReason;
                ViewBag.fld_WorkCtgry = fld_WorkCtgry;
                ViewBag.fld_PaymentMode = fld_PaymentMode;//added by faeza 20.04.2021
                ViewBag.GetBatchNo = GetBatchNo;

                return View(tbl_LbrDataInfo);
            }
            else
            {
                fld_State = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_State).ToList();
                fld_Country = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue == "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Country).ToList();
                fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_SexType).ToList();
                fld_Race = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "bangsa" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Race).ToList();
                fld_Religion = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "agama" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Religion).ToList();
                fld_Nationality = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue == "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Nationality).ToList();
                fld_MarriedStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "tarafKahwin" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_MarriedStatus).ToList();
                fld_WorkerType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jnsPkj" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkerType).ToList();
                fld_ActiveStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_ActiveStatus).ToList();
                fld_InactiveReason = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "sbbTakAktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_InactiveReason).ToList();
                fld_WorkCtgry = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "designation" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkCtgry).ToList();
                fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();//added by faeza 20.04.2021

                ViewBag.fld_State = fld_State;
                ViewBag.fld_Country = fld_Country;
                ViewBag.fld_SexType = fld_SexType;
                ViewBag.fld_Race = fld_Race;
                ViewBag.fld_Religion = fld_Religion;
                ViewBag.fld_Nationality = fld_Nationality;
                ViewBag.fld_MarriedStatus = fld_MarriedStatus;
                ViewBag.fld_WorkerType = fld_WorkerType;
                ViewBag.fld_FeldaRelated = fld_FeldaRelated;
                ViewBag.fld_ActiveStatus = fld_ActiveStatus;
                ViewBag.fld_InactiveReason = fld_InactiveReason;
                ViewBag.fld_WorkCtgry = fld_WorkCtgry;
                ViewBag.fld_PaymentMode = fld_PaymentMode;//added by faeza 20.04.2021
                ViewBag.GetBatchNo = GetBatchNo;

                return View(tbl_LbrDataInfo);
            }
            //}
            //catch (Exception ex)
            //{
            //    geterror.catcherro(ex.Message, ex.StackTrace, ex.Source, ex.TargetSite.ToString());
            //}
            //return View(tbl_LbrDataInfo);

        }

        public async Task<ActionResult> TKAInformation(string BatchNo)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            var GetStatusBatchNo = db.tbl_LbrRqst.Where(x => x.fld_BatchNo == BatchNo && x.fld_ApprovedStatus == 1 && x.fld_ProcessStatus != 99).FirstOrDefault();
            var batchno = GetStatusBatchNo == null ? 0 : GetStatusBatchNo.fld_AppReqQty;
            //Commented by Shazana on 17/8
            //return View(await db.tbl_LbrTKAProcess.ToListAsync());

            //Added By Shazana on 17/8
            var fld_idBatchNo = db.tbl_LbrRqst.Where(x => x.fld_BatchNo == BatchNo).Select(x => x.fld_ID).FirstOrDefault();
            return View(await db.tbl_LbrTKAProcess.Where(x => x.fld_LbrRqstID == fld_idBatchNo).ToListAsync());
        }
        
        public async Task<ActionResult> TKAFullInformation(Guid? id)
        {
            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            tbl_LbrDataInfo tbl_LbrDataInfo = new tbl_LbrDataInfo();
            string[] WebConfigFilter = new string[] { "jantina", "tarafKahwin", "bangsa", "agama", "krytnlist", "negeri", "jnsPkj", "designation", "statusaktif", "sbbTakAktif", "designation", "paymentmode" };
            
            List<SelectListItem> fld_State = new List<SelectListItem>();
            List<SelectListItem> fld_Country = new List<SelectListItem>();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_Race = new List<SelectListItem>();
            List<SelectListItem> fld_Religion = new List<SelectListItem>();
            List<SelectListItem> fld_Nationality = new List<SelectListItem>();
            List<SelectListItem> fld_MarriedStatus = new List<SelectListItem>();
            List<SelectListItem> fld_WorkerType = new List<SelectListItem>();
            List<SelectListItem> fld_WorkCtgry = new List<SelectListItem>();
            List<SelectListItem> fld_FeldaRelated = new List<SelectListItem>();
            List<SelectListItem> fld_ActiveStatus = new List<SelectListItem>();
            List<SelectListItem> fld_InactiveReason = new List<SelectListItem>();
            List<SelectListItem> fld_SupplierCode = new List<SelectListItem>();
            List<SelectListItem> fld_PaymentMode = new List<SelectListItem>(); //added by faeza 20.04.2021

            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            var GetDetailFromProcess = db.tbl_LbrTKAProcess.Find(id);
            var GetBatchNo = db.tbl_LbrRqst.Where(x => x.fld_ID == GetDetailFromProcess.fld_LbrRqstID).Select(s => s.fld_BatchNo).FirstOrDefault();
            tbl_LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_LbrProcessID == GetDetailFromProcess.fld_ID && x.fld_Nationality != "MA").FirstOrDefaultAsync();
            if (tbl_LbrDataInfo == null)
            {
                tbl_LbrDataInfo = new tbl_LbrDataInfo();
                tbl_LbrDataInfo.fld_WorkerIDNo = GetDetailFromProcess.fld_NoPassport;
                tbl_LbrDataInfo.fld_WorkerName = GetDetailFromProcess.fld_Nama;
                tbl_LbrDataInfo.fld_BOD = GetDetailFromProcess.fld_BOD;
                tbl_LbrDataInfo.fld_Age = GetDetailFromProcess.fld_Age;
                tbl_LbrDataInfo.fld_ArrivedDT = GetDetailFromProcess.fld_ArrivedDT;
                tbl_LbrDataInfo.fld_LbrProcessID = GetDetailFromProcess.fld_ID;
            }

            fld_State = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_State).ToList();
            fld_Country = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue != "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Country).ToList();
            fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_SexType).ToList();
            fld_Race = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "bangsa" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Race).ToList();
            fld_Religion = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "agama" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Religion).ToList();
            fld_Nationality = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue != "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Nationality).ToList();
            fld_MarriedStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "tarafKahwin" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_MarriedStatus).ToList();
            fld_WorkerType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jnsPkj" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkerType).ToList();
            fld_ActiveStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_ActiveStatus).ToList();
            fld_InactiveReason = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "sbbTakAktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_InactiveReason).ToList();
            fld_WorkCtgry = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "designation" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkCtgry).ToList();
            fld_SupplierCode = new SelectList(Masterdb.tbl_Pembekal.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).OrderBy(o => o.fld_KodPbkl).Select(s => new SelectListItem { Value = s.fld_KodPbkl.ToUpper(), Text = s.fld_NamaPbkl.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_SupplierCode).ToList();
            //added by faeza 20.04.2021
            fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();

            ViewBag.fld_State = fld_State;
            ViewBag.fld_Country = fld_Country;
            ViewBag.fld_SexType = fld_SexType;
            ViewBag.fld_Race = fld_Race;
            ViewBag.fld_Religion = fld_Religion;
            ViewBag.fld_Nationality = fld_Nationality;
            ViewBag.fld_MarriedStatus = fld_MarriedStatus;
            ViewBag.fld_WorkerType = fld_WorkerType;
            ViewBag.fld_FeldaRelated = fld_FeldaRelated;
            ViewBag.fld_ActiveStatus = fld_ActiveStatus;
            ViewBag.fld_InactiveReason = fld_InactiveReason;
            ViewBag.fld_WorkCtgry = fld_WorkCtgry;
            ViewBag.GetBatchNo = GetBatchNo;
            ViewBag.fld_SupplierCode = fld_SupplierCode;
            ViewBag.fld_PaymentMode = fld_PaymentMode;//added by faeza 20.04.2021

            ViewBag.ProccessID = GetDetailFromProcess.fld_ID;
            return View(tbl_LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TKAFullInformation(tbl_LbrDataInfo tbl_LbrDataInfo)
        {

            ViewBag.LabourRequest = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();

            tbl_LbrDataInfo LbrDataInfo = new tbl_LbrDataInfo();
            string[] WebConfigFilter = new string[] { "jantina", "tarafKahwin", "bangsa", "agama", "krytnlist", "negeri", "jnsPkj", "designation", "statusaktif", "sbbTakAktif", "designation", "paymentmode" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            List<SelectListItem> fld_State = new List<SelectListItem>();
            List<SelectListItem> fld_Country = new List<SelectListItem>();
            List<SelectListItem> fld_SexType = new List<SelectListItem>();
            List<SelectListItem> fld_Race = new List<SelectListItem>();
            List<SelectListItem> fld_Religion = new List<SelectListItem>();
            List<SelectListItem> fld_Nationality = new List<SelectListItem>();
            List<SelectListItem> fld_MarriedStatus = new List<SelectListItem>();
            List<SelectListItem> fld_WorkerType = new List<SelectListItem>();
            List<SelectListItem> fld_WorkCtgry = new List<SelectListItem>();
            List<SelectListItem> fld_FeldaRelated = new List<SelectListItem>();
            List<SelectListItem> fld_ActiveStatus = new List<SelectListItem>();
            List<SelectListItem> fld_InactiveReason = new List<SelectListItem>();
            List<SelectListItem> fld_SupplierCode = new List<SelectListItem>();
            List<SelectListItem> fld_PaymentMode = new List<SelectListItem>();//added by faeza 20.04.2021             

            LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_LbrProcessID == tbl_LbrDataInfo.fld_LbrProcessID && x.fld_Nationality != "MA").FirstOrDefault();
            var GetLbrRqstDetails = db.tbl_LbrTKAProcess.Find(tbl_LbrDataInfo.fld_LbrProcessID);
            var GetBatchNo = db.tbl_LbrRqst.Where(x => x.fld_ID == GetLbrRqstDetails.fld_LbrRqstID).Select(s => s.fld_BatchNo).FirstOrDefault();

            if (ModelState.IsValid)
            {
                if (LbrDataInfo == null)
                {
                    tbl_LbrDataInfo.fld_NegaraID = NegaraID;
                    tbl_LbrDataInfo.fld_SyarikatID = SyarikatID;
                    tbl_LbrDataInfo.fld_WilayahID = GetLbrRqstDetails.fld_WilayahID;
                    tbl_LbrDataInfo.fld_LadangID = GetLbrRqstDetails.fld_LadangID;
                    tbl_LbrDataInfo.fld_CreatedBy = GetUserID;
                    tbl_LbrDataInfo.fld_CreatedDT = DT;
                    tbl_LbrDataInfo.fld_ModifiedBy = GetUserID;
                    tbl_LbrDataInfo.fld_ModifiedDT = DT;
                    tbl_LbrDataInfo.fld_DivisionID = null;
                    tbl_LbrDataInfo.fld_FeldaRelated = "0";
                    tbl_LbrDataInfo.fld_ActiveStatus = "1";
                    tbl_LbrDataInfo.fld_InactiveReason = null;
                    tbl_LbrDataInfo.fld_InactiveDT = null;
                    tbl_LbrDataInfo.fld_TransferToChckrollStatus = null;
                    db.tbl_LbrDataInfo.Add(tbl_LbrDataInfo);
                    await db.SaveChangesAsync();
                    LbrDataInfo = new tbl_LbrDataInfo();
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_LbrProcessID == tbl_LbrDataInfo.fld_LbrProcessID && x.fld_Nationality == "MA").FirstOrDefault();
                    tbl_LbrDataInfo = db.tbl_LbrDataInfo.Find(tbl_LbrDataInfo.fld_ID);
                    ModelState.AddModelError("", "Update Successfully");
                    ViewBag.MsgColor = "color: green";

                }
                else
                {
                    LbrDataInfo.fld_BOD = tbl_LbrDataInfo.fld_BOD;
                    LbrDataInfo.fld_Age = tbl_LbrDataInfo.fld_Age;
                    LbrDataInfo.fld_Country = tbl_LbrDataInfo.fld_Country;
                    LbrDataInfo.fld_MarriedStatus = tbl_LbrDataInfo.fld_MarriedStatus;
                    LbrDataInfo.fld_ModifiedBy = GetUserID;
                    LbrDataInfo.fld_ModifiedDT = DT;
                    LbrDataInfo.fld_Notes = tbl_LbrDataInfo.fld_Notes;
                    LbrDataInfo.fld_PhoneNo = tbl_LbrDataInfo.fld_PhoneNo;
                    LbrDataInfo.fld_Nationality = tbl_LbrDataInfo.fld_Nationality;
                    LbrDataInfo.fld_Postcode = tbl_LbrDataInfo.fld_Postcode;
                    LbrDataInfo.fld_Race = tbl_LbrDataInfo.fld_Race;
                    LbrDataInfo.fld_Religion = tbl_LbrDataInfo.fld_Religion;
                    LbrDataInfo.fld_SexType = tbl_LbrDataInfo.fld_SexType;
                    LbrDataInfo.fld_State = tbl_LbrDataInfo.fld_State;
                    LbrDataInfo.fld_WorkCtgry = tbl_LbrDataInfo.fld_WorkCtgry;
                    LbrDataInfo.fld_WorkerAddress = tbl_LbrDataInfo.fld_WorkerAddress;
                    LbrDataInfo.fld_WorkerName = tbl_LbrDataInfo.fld_WorkerName.ToUpper();
                    LbrDataInfo.fld_WorkerType = tbl_LbrDataInfo.fld_WorkerType;
                    LbrDataInfo.fld_WorkingStartDT = tbl_LbrDataInfo.fld_WorkingStartDT;
                    LbrDataInfo.fld_ConfirmationDT = tbl_LbrDataInfo.fld_ConfirmationDT;
                    LbrDataInfo.fld_ActiveStatus = tbl_LbrDataInfo.fld_ActiveStatus;
                    LbrDataInfo.fld_PermitNo = tbl_LbrDataInfo.fld_PermitNo;
                    LbrDataInfo.fld_PassportEndDT = tbl_LbrDataInfo.fld_PassportEndDT;
                    LbrDataInfo.fld_PermitEndDT = tbl_LbrDataInfo.fld_PermitEndDT;
                    LbrDataInfo.fld_WorkerIDNo = tbl_LbrDataInfo.fld_WorkerIDNo;
                    LbrDataInfo.fld_WorkerNo = tbl_LbrDataInfo.fld_WorkerNo;
                    LbrDataInfo.fld_SupplierCode = tbl_LbrDataInfo.fld_SupplierCode;
                    LbrDataInfo.fld_ArrivedDT = tbl_LbrDataInfo.fld_ArrivedDT;
                    LbrDataInfo.fld_ActiveStatus = "1";
                    LbrDataInfo.fld_InactiveReason = null;
                    LbrDataInfo.fld_InactiveDT = null;
                    LbrDataInfo.fld_PaymentMode = tbl_LbrDataInfo.fld_PaymentMode; //added by faeza 20.04.2021
                    LbrDataInfo.fld_Last4Pan = tbl_LbrDataInfo.fld_Last4Pan; //added by Faeza 20.04.2021
                    db.Entry(LbrDataInfo).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    LbrDataInfo = new tbl_LbrDataInfo();
                    LbrDataInfo = db.tbl_LbrDataInfo.Where(x => x.fld_LbrProcessID == tbl_LbrDataInfo.fld_LbrProcessID && x.fld_Nationality == "MA").FirstOrDefault();
                    ModelState.AddModelError("", "Update Successfully");
                    ViewBag.MsgColor = "color: green";
                }

                fld_State = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_State).ToList();
                fld_Country = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue != "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Country).ToList();
                fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_SexType).ToList();
                fld_Race = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "bangsa" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Race).ToList();
                fld_Religion = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "agama" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Religion).ToList();
                fld_Nationality = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue != "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Nationality).ToList();
                fld_MarriedStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "tarafKahwin" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_MarriedStatus).ToList();
                fld_WorkerType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jnsPkj" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkerType).ToList();
                fld_ActiveStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_ActiveStatus).ToList();
                fld_InactiveReason = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "sbbTakAktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_InactiveReason).ToList();
                fld_WorkCtgry = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "designation" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkCtgry).ToList();
                fld_SupplierCode = new SelectList(Masterdb.tbl_Pembekal.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).OrderBy(o => o.fld_KodPbkl).Select(s => new SelectListItem { Value = s.fld_KodPbkl.ToUpper(), Text = s.fld_NamaPbkl.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_SupplierCode).ToList();
                //added by faeza 20.04.2021
                fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();

                ViewBag.fld_State = fld_State;
                ViewBag.fld_Country = fld_Country;
                ViewBag.fld_SexType = fld_SexType;
                ViewBag.fld_Race = fld_Race;
                ViewBag.fld_Religion = fld_Religion;
                ViewBag.fld_Nationality = fld_Nationality;
                ViewBag.fld_MarriedStatus = fld_MarriedStatus;
                ViewBag.fld_WorkerType = fld_WorkerType;
                ViewBag.fld_FeldaRelated = fld_FeldaRelated;
                ViewBag.fld_ActiveStatus = fld_ActiveStatus;
                ViewBag.fld_InactiveReason = fld_InactiveReason;
                ViewBag.fld_WorkCtgry = fld_WorkCtgry;
                ViewBag.GetBatchNo = GetBatchNo;
                ViewBag.fld_SupplierCode = fld_SupplierCode;
                ViewBag.fld_PaymentMode = fld_PaymentMode;//added by faeza 20.04.2021

                return View(tbl_LbrDataInfo);
            }
            else
            {
                fld_State = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_State).ToList();
                fld_Country = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue != "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Country).ToList();
                fld_SexType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jantina" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_SexType).ToList();
                fld_Race = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "bangsa" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Race).ToList();
                fld_Religion = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "agama" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Religion).ToList();
                fld_Nationality = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "krytnlist" && x.fldOptConfValue != "MA" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Nationality).ToList();
                fld_MarriedStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "tarafKahwin" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_MarriedStatus).ToList();
                fld_WorkerType = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jnsPkj" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkerType).ToList();
                fld_ActiveStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_ActiveStatus).ToList();
                fld_InactiveReason = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "sbbTakAktif" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_InactiveReason).ToList();
                fld_WorkCtgry = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "designation" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_WorkCtgry).ToList();
                fld_SupplierCode = new SelectList(Masterdb.tbl_Pembekal.Where(x => x.fld_SyarikatID == SyarikatID && x.fld_NegaraID == NegaraID && x.fld_Deleted == false).OrderBy(o => o.fld_KodPbkl).Select(s => new SelectListItem { Value = s.fld_KodPbkl.ToUpper(), Text = s.fld_NamaPbkl.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_SupplierCode).ToList();
                //added by faeza 20.04.2021
                fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();

                ViewBag.fld_State = fld_State;
                ViewBag.fld_Country = fld_Country;
                ViewBag.fld_SexType = fld_SexType;
                ViewBag.fld_Race = fld_Race;
                ViewBag.fld_Religion = fld_Religion;
                ViewBag.fld_Nationality = fld_Nationality;
                ViewBag.fld_MarriedStatus = fld_MarriedStatus;
                ViewBag.fld_WorkerType = fld_WorkerType;
                ViewBag.fld_FeldaRelated = fld_FeldaRelated;
                ViewBag.fld_ActiveStatus = fld_ActiveStatus;
                ViewBag.fld_InactiveReason = fld_InactiveReason;
                ViewBag.fld_WorkCtgry = fld_WorkCtgry;
                ViewBag.GetBatchNo = GetBatchNo;
                ViewBag.fld_SupplierCode = fld_SupplierCode;
                ViewBag.fld_PaymentMode = fld_PaymentMode;//added by faeza 20.04.2021

                return View(tbl_LbrDataInfo);
            }
        }

        // GET: LabourApprovedRequest/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrRqst tbl_LbrRqst = await db.tbl_LbrRqst.FindAsync(id);
            if (tbl_LbrRqst == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrRqst);
        }

        // GET: LabourApprovedRequest/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LabourApprovedRequest/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(tbl_LbrRqst tbl_LbrRqst)
        {
            if (ModelState.IsValid)
            {
                db.tbl_LbrRqst.Add(tbl_LbrRqst);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(tbl_LbrRqst);
        }

        // GET: LabourApprovedRequest/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrRqst tbl_LbrRqst = await db.tbl_LbrRqst.FindAsync(id);
            if (tbl_LbrRqst == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrRqst);
        }

        // POST: LabourApprovedRequest/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(tbl_LbrRqst tbl_LbrRqst)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_LbrRqst).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(tbl_LbrRqst);
        }

        // GET: LabourApprovedRequest/Delete/5
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_LbrRqst tbl_LbrRqst = await db.tbl_LbrRqst.FindAsync(id);
            if (tbl_LbrRqst == null)
            {
                return HttpNotFound();
            }
            return View(tbl_LbrRqst);
        }

        // POST: LabourApprovedRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            tbl_LbrRqst tbl_LbrRqst = await db.tbl_LbrRqst.FindAsync(id);
            db.tbl_LbrRqst.Remove(tbl_LbrRqst);
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
