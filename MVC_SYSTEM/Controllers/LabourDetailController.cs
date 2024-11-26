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
using MVC_SYSTEM.App_LocalResources;
namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    public class LabourDetailController : Controller
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
        // GET: LabourDetail

        public async Task<ActionResult> Index(int? WilayahIDList, int? LadangIDList, string StatusList, string JnsPkjList, string FreeText)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            List<SelectListItem> fld_WilayahID = new List<SelectListItem>();
            List<SelectListItem> fld_LadangID = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "jnsPkj", "statusaktif" };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            int? GetWilayahID = 0;
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

                fld_LadangID = new SelectList(Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode + " - " + s.fld_LdgName }), "Value", "Text").ToList();
                fld_LadangID.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            }
            else
            {
                var GetWilayahData = Masterdb.tbl_Wilayah.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_ID == WilayahID && x.fld_Deleted == false).OrderBy(o => o.fld_WlyhName).ToList();
                fld_WilayahID = new SelectList(GetWilayahData, "fld_ID", "fld_WlyhName").ToList();

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

            List<SelectListItem> fld_WorkCtgry = new List<SelectListItem>();
            fld_WorkCtgry = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "jnsPkj" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            fld_WorkCtgry.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            List<SelectListItem> fld_ActiveStatus = new List<SelectListItem>();
            fld_ActiveStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "statusaktif" && x.fldDeleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }), "Value", "Text").ToList();
            fld_ActiveStatus.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));
            ViewBag.StatusList = fld_ActiveStatus;
            ViewBag.JnsPkjList = fld_WorkCtgry;
            ViewBag.WilayahIDList = fld_WilayahID;
            ViewBag.LadangIDList = fld_LadangID;

            //    if (WilayahID == 0 && LadangID == 0)
            //    {
            //        if (string.IsNullOrEmpty(FreeText))
            //        {
            //            if (WilayahIDList == 0 && LadangIDList == 0 && JnsPkjList == "0" && StatusList == "0")
            //            {
            //                LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID ).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
            //            }
            //            else if (WilayahIDList != 0 && LadangIDList == 0)
            //            {
            //                LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_ActiveStatus == StatusList && x.fld_WorkerType == JnsPkjList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
            //            }
            //            else
            //            {
            //                LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_ActiveStatus == StatusList && x.fld_WorkerType == JnsPkjList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
            //            }
            //        }
            //        else
            //        {
            //              LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WorkerType == JnsPkjList && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();

            //        }
            //    }
            //    else
            //    {
            //        if (string.IsNullOrEmpty(FreeText))
            //        {
            //            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_ActiveStatus == StatusList && x.fld_WorkerType == JnsPkjList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
            //        }
            //        else
            //        {
            //            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_ActiveStatus == StatusList && x.fld_WorkerType == JnsPkjList && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
            //        }
            //    }
            //    return View(LbrDataInfo);          //commented by wani 11.4.2020
            //}   


            if (WilayahID == 0 && LadangID == 0)
            {
                if (string.IsNullOrEmpty(FreeText))
                {
                    if (WilayahIDList == 0 && LadangIDList == 0)
                    {
                        if (JnsPkjList != (0).ToString())
                        {
                            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WorkerType == JnsPkjList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                        }
                        if (StatusList != (0).ToString())
                        {
                            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_ActiveStatus == StatusList).OrderBy(o => new { o.fld_LadangID, o.fld_WorkerName }).ToListAsync();
                        }
                        else
                        {
                            LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                        }
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0 && JnsPkjList == (0).ToString() && StatusList == (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    }
                    /*Edited By Shazana on 6/8*/    else if (WilayahIDList != 0 && LadangIDList == 0 && JnsPkjList != (0).ToString() && StatusList != (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_ActiveStatus == StatusList && x.fld_WorkerType == JnsPkjList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    }
                    /*Close Edited By Shazana on 6/8*/
                    else if (WilayahIDList != 0 && LadangIDList != 0 && JnsPkjList == (0).ToString() && StatusList == (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    }
                    else if (WilayahIDList != 0 && LadangIDList != 0 && JnsPkjList != (0).ToString() && StatusList == (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_WorkerType == JnsPkjList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    }
                    //Added by Shazana on 6/8
                    else if (WilayahIDList != 0 && LadangIDList != 0 && JnsPkjList == (0).ToString() && StatusList != (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_ActiveStatus == StatusList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    }
                    else if (WilayahIDList != 0 && LadangIDList != 0 && JnsPkjList == (0).ToString() && StatusList == (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    }
                    else if (WilayahIDList != 0 && LadangIDList == 0 && JnsPkjList == (0).ToString() && StatusList != (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList  && x.fld_ActiveStatus == StatusList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    }
                    //Close Added by Shazana on 6/8
                    else if (WilayahIDList != 0 && LadangIDList == 0 && JnsPkjList != (0).ToString() && StatusList == (0).ToString())
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_WorkerType == JnsPkjList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    }
                    else
                    {
                        LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_ActiveStatus == StatusList && x.fld_WorkerType == JnsPkjList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    }
                }
                else
                {
                    LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                }
            }
            else
            {
                //Added by Shazana on 20/4/2021
                if (StatusList == "0")
                {
                    StatusList = "";
                }
                if (JnsPkjList == "0")
                {
                    JnsPkjList = "";
                }
                //Close Added by Shazana on 20/4/2021

                if (string.IsNullOrEmpty(FreeText))
                {
                    //Commented by Shazana on 20/4/2021 
                    //LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_ActiveStatus == StatusList && x.fld_WorkerType == JnsPkjList).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    //Added by Shazana on 20/4/2021
                    LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_ActiveStatus.Contains(StatusList) && x.fld_WorkerType.Contains(JnsPkjList)).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();

                }
                else
                {
                    //Commented by Shazana on 20/4/2021 
                    //LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_ActiveStatus == StatusList && x.fld_WorkerType == JnsPkjList && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();
                    //Added by Shazana on 20/4/2021 
                    LbrDataInfo = await db.tbl_LbrDataInfo.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahIDList && x.fld_LadangID == LadangIDList && x.fld_ActiveStatus.Contains(StatusList) && x.fld_WorkerType.Contains(JnsPkjList) && (x.fld_WorkerIDNo.Contains(FreeText) || x.fld_WorkerName.Contains(FreeText) || x.fld_WorkerNo.Contains(FreeText))).OrderByDescending(o => new { o.fld_WorkerName }).ToListAsync();

                }
            }
            return View(LbrDataInfo);
        }     // modified by wani 11.4.2020

        public async Task<ActionResult> UpdateMenu(Guid? id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            tbl_LbrDataInfo tbl_LbrDataInfo = new tbl_LbrDataInfo();

            tbl_LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
            ViewBag.RoleUser = GetIdentity.getRoleID(GetUserID);//Added by Shazana on 20/4/2021
            return View(tbl_LbrDataInfo);
        }

        public async Task<ActionResult> TKTFullInformation(Guid? id)
        {
            ViewBag.LabourManagement = "class = active";
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
            List<SelectListItem> fld_BankCode = new List<SelectListItem>();
            List<SelectListItem> fld_PaymentMode = new List<SelectListItem>();//added by faeza 20.04.2021

            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
           
            tbl_LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);

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
            //fld_FeldaRelated = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_State).ToList();
            fld_BankCode = new SelectList(Masterdb.tbl_Bank.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_KodBank).Select(s => new SelectListItem { Value = s.fld_KodBank, Text = s.fld_NamaBank.ToUpper() }), "Value", "Text", tbl_LbrDataInfo.fld_BankCode).ToList();
            //added by faeza 20.04.2021
            fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();

            var GetExistingPassport = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 2 && x.fld_Deleted == false).FirstOrDefault();
            if (GetExistingPassport == null)
            { ViewBag.ExistingPassport = 0; }
            else
            { ViewBag.ExistingPassport = 1; }

            var GetExistingPermit = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 1 && x.fld_Deleted == false).FirstOrDefault();
            if (GetExistingPermit == null)
            { ViewBag.ExistingPermit = 0; }
            else
            { ViewBag.ExistingPermit = 1; }

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
            ViewBag.fld_BankCode = fld_BankCode;
            ViewBag.fld_PaymentMode = fld_PaymentMode;

            return View(tbl_LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TKTFullInformation(tbl_LbrDataInfo tbl_LbrDataInfo)
        {
            ViewBag.LabourManagement = "class = active";
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
            List<SelectListItem> fld_BankCode = new List<SelectListItem>();
            List<SelectListItem> fld_PaymentMode = new List<SelectListItem>(); //added by faeza 20.04.2021

            if (ModelState.IsValid)
            {
                LbrDataInfo = db.tbl_LbrDataInfo.Find(tbl_LbrDataInfo.fld_ID);
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
                LbrDataInfo.fld_WorkerAddress = tbl_LbrDataInfo.fld_WorkerAddress?.ToUpper();//edit by faeza 06.09.2021
                LbrDataInfo.fld_WorkerName = tbl_LbrDataInfo.fld_WorkerName?.ToUpper();//edit by faeza 06.09.2021
                LbrDataInfo.fld_WorkerType = tbl_LbrDataInfo.fld_WorkerType;
                LbrDataInfo.fld_WorkingStartDT = tbl_LbrDataInfo.fld_WorkingStartDT;
                LbrDataInfo.fld_ConfirmationDT = tbl_LbrDataInfo.fld_ConfirmationDT;
                LbrDataInfo.fld_ActiveStatus = tbl_LbrDataInfo.fld_ActiveStatus;
                LbrDataInfo.fld_WorkerIDNo = tbl_LbrDataInfo.fld_WorkerIDNo?.ToUpper();//edit by faeza 06.09.2021
                LbrDataInfo.fld_ActiveStatus = "1";
                LbrDataInfo.fld_InactiveReason = null;
                LbrDataInfo.fld_InactiveDT = null;
                LbrDataInfo.fld_BankCode = tbl_LbrDataInfo.fld_BankCode;
                LbrDataInfo.fld_BankAcc = tbl_LbrDataInfo.fld_BankAcc;
                LbrDataInfo.fld_PerkesoNo = tbl_LbrDataInfo.fld_PerkesoNo;
                ///added by faeza 20.04.2021
                LbrDataInfo.fld_PaymentMode = tbl_LbrDataInfo.fld_PaymentMode;
                LbrDataInfo.fld_Last4Pan = tbl_LbrDataInfo.fld_Last4Pan;
                db.Entry(LbrDataInfo).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Permit dan passport
                //Passport 2
                var GetExistingPassport = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 2 && x.fld_Deleted == false).FirstOrDefault();
                if (GetExistingPassport == null)
                { ViewBag.ExistingPassport = 0; }
                else
                {
                    ViewBag.ExistingPassport = 1;
                    GetExistingPassport.fld_PassportRenewalStartDate = tbl_LbrDataInfo.fld_PassportRenewalStartDate;
                    GetExistingPassport.fld_PassportRenewalStatus = tbl_LbrDataInfo.fld_PassportRenewalStatus;
                    GetExistingPassport.fld_ModifiedBy = GetUserID;
                    GetExistingPassport.fld_ModifiedDT = DT;
                    db.Entry(GetExistingPassport).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //Permit 1
                var GetExistingPermit = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 1 && x.fld_Deleted == false).FirstOrDefault();
                if (GetExistingPermit == null)
                { ViewBag.ExistingPermit = 0; }
                else
                {
                    ViewBag.ExistingPermit = 1;
                    GetExistingPermit.fld_PermitRenewalStatus = tbl_LbrDataInfo.fld_PermitRenewalStatus;
                    GetExistingPermit.fld_PermitRenewalStartDate = tbl_LbrDataInfo.fld_PermitRenewalStartDate;
                    GetExistingPermit.fld_ModifiedBy = GetUserID;
                    GetExistingPermit.fld_ModifiedDT = DT;
                    db.Entry(GetExistingPermit).State = EntityState.Modified;
                    db.SaveChanges();
                }

                bool SyncStatus = SyncToCheckRoll(LbrDataInfo.fld_ID);
                if (SyncStatus)
                {
                    ModelState.AddModelError("", "Update Successfully And Sync To Checkroll");
                    ViewBag.MsgColor = "color: green";
                }
                else
                {
                    ModelState.AddModelError("", "Update Successfully And Failed Sync To Checkroll");
                    ViewBag.MsgColor = "color: orange";
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
                //fld_FeldaRelated = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_State).ToList();
                fld_BankCode = new SelectList(Masterdb.tbl_Bank.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_KodBank).Select(s => new SelectListItem { Value = s.fld_KodBank, Text = s.fld_NamaBank.ToUpper() }), "Value", "Text", tbl_LbrDataInfo.fld_BankCode).ToList();
                //addedby faeza 20.04.2021
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
                ViewBag.fld_BankCode = fld_BankCode;
                ViewBag.fld_PaymentMode = fld_PaymentMode;

                return View(LbrDataInfo);

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
                //fld_FeldaRelated = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "negeri" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_State).ToList();
                fld_BankCode = new SelectList(Masterdb.tbl_Bank.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_KodBank).Select(s => new SelectListItem { Value = s.fld_KodBank, Text = s.fld_NamaBank.ToUpper() }), "Value", "Text", tbl_LbrDataInfo.fld_BankCode).ToList();
                //addedby faeza 20.04.2021
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
                ViewBag.fld_BankCode = fld_BankCode;
                ViewBag.fld_PaymentMode = fld_PaymentMode;

                return View(LbrDataInfo);
            }

        }

        public async Task<ActionResult> TKAFullInformation(Guid? id)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            tbl_LbrDataInfo tbl_LbrDataInfo = new tbl_LbrDataInfo();
            string[] WebConfigFilter = new string[] { "permitrenewalstatus", "passportrenewalstatus", "passportpermitstatus","jantina", "tarafKahwin", "bangsa", "agama", "krytnlist", "negeri", "jnsPkj", "designation", "statusaktif", "sbbTakAktif", "designation","roc", "paymentmode" };
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
            List<SelectListItem> fld_Roc = new List<SelectListItem>();
            List<SelectListItem> fld_BankCode = new List<SelectListItem>();
            List<SelectListItem> fld_PaymentMode = new List<SelectListItem>();//added by faeza 20.04.2021

            //Added by Shazana 29/3/2024
            List<SelectListItem> fld_PassportRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatusPermit = new List<SelectListItem>();
            List<SelectListItem> PermitRenewalStatus = new List<SelectListItem>();

            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            tbl_LbrDataInfo = await db.tbl_LbrDataInfo.FindAsync(id);
           
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
            fld_Roc = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "roc" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Roc).ToList();
            fld_BankCode = new SelectList(Masterdb.tbl_Bank.Where(x=>x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o=>o.fld_KodBank).Select(s=> new SelectListItem { Value = s.fld_KodBank, Text = s.fld_NamaBank.ToUpper()}), "Value", "Text", tbl_LbrDataInfo.fld_BankCode).ToList();
            fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();

            //Added by Shazana 29/3/2024
            PassportPermitStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PassportStatus).ToList();
            PassportPermitStatusPermit = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text").ToList();
            fld_Roc = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "roc" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Roc).ToList();
            fld_BankCode = new SelectList(Masterdb.tbl_Bank.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_KodBank).Select(s => new SelectListItem { Value = s.fld_KodBank, Text = s.fld_NamaBank.ToUpper() }), "Value", "Text", tbl_LbrDataInfo.fld_BankCode).ToList();

            var GetExistingPassport = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 2 && x.fld_Deleted == false).FirstOrDefault();
            if (GetExistingPassport == null)
            {
                ViewBag.ExistingPassport = 0;
                PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
            }
            else
            {
                ViewBag.ExistingPassport = 1;
                
                PassportRenewalStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PassportRenewalStatus).ToList();
                PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
            }

            var GetExistingPermit = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 1 && x.fld_Deleted == false).FirstOrDefault();
            if (GetExistingPermit == null)
            {
                ViewBag.ExistingPermit = 0;
                PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
            }
            else
            {
                ViewBag.ExistingPermit = 1;
                PermitRenewalStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "permitrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PermitRenewalStatus).ToList();
                PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
            }

            PassportPermitStatusPermit.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
            PassportPermitStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));


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
            ViewBag.fld_SupplierCode = fld_SupplierCode;
            ViewBag.fld_Roc = fld_Roc;
            ViewBag.fld_BankCode = fld_BankCode;
            ViewBag.fld_PaymentMode = fld_PaymentMode;

            //Added by Shazana 29/3/2024
            ViewBag.fld_PermitRenewalStatus = PermitRenewalStatus;
            ViewBag.fld_PermitStatus = PassportPermitStatusPermit;
            ViewBag.fld_PassportRenewalStatus = PassportRenewalStatus;
            ViewBag.fld_PassportStatus = PassportPermitStatus;
            ViewBag.fld_Roc = fld_Roc;
            ViewBag.fld_BankCode = fld_BankCode;



            return View(tbl_LbrDataInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TKAFullInformation(tbl_LbrDataInfo tbl_LbrDataInfo)
        {
            ViewBag.LabourManagement = "class = active";
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            DT = ChangeTimeZone.gettimezone();
            tbl_LbrDataInfo LbrDataInfo = new tbl_LbrDataInfo();

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
            List<SelectListItem> fld_Roc = new List<SelectListItem>();
            List<SelectListItem> fld_BankCode = new List<SelectListItem>();
            List<SelectListItem> fld_PaymentMode = new List<SelectListItem>();

            //Added by Shazana 29/3/2024
            List<SelectListItem> fld_PassportRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportRenewalStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatus = new List<SelectListItem>();
            List<SelectListItem> PassportPermitStatusPermit = new List<SelectListItem>();
            List<SelectListItem> PermitRenewalStatus = new List<SelectListItem>();

            string[] WebConfigFilter = new string[] { "jantina", "tarafKahwin", "bangsa", "agama", "krytnlist", "negeri", "jnsPkj", "designation", "statusaktif", "sbbTakAktif", "designation", "roc","paymentmode", "permitrenewalstatus", "passportrenewalstatus", "passportpermitstatus", };
            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            if (ModelState.IsValid)
            {

                LbrDataInfo = db.tbl_LbrDataInfo.Find(tbl_LbrDataInfo.fld_ID);
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
                LbrDataInfo.fld_WorkerAddress = tbl_LbrDataInfo.fld_WorkerAddress?.ToUpper();//edit by faeza 06.09.2021
                LbrDataInfo.fld_WorkerName = tbl_LbrDataInfo.fld_WorkerName?.ToUpper();//edit by faeza 06.09.2021
                LbrDataInfo.fld_WorkerType = tbl_LbrDataInfo.fld_WorkerType;
                LbrDataInfo.fld_WorkingStartDT = tbl_LbrDataInfo.fld_WorkingStartDT;
                LbrDataInfo.fld_ConfirmationDT = tbl_LbrDataInfo.fld_ConfirmationDT;
                LbrDataInfo.fld_ActiveStatus = tbl_LbrDataInfo.fld_ActiveStatus;
                LbrDataInfo.fld_PermitNo = tbl_LbrDataInfo.fld_PermitNo;
                LbrDataInfo.fld_PassportStartDT = tbl_LbrDataInfo.fld_PassportStartDT;  //Added by Shazana on 23/8
                LbrDataInfo.fld_PassportEndDT = tbl_LbrDataInfo.fld_PassportEndDT;
                LbrDataInfo.fld_PermitEndDT = tbl_LbrDataInfo.fld_PermitEndDT;
                LbrDataInfo.fld_WorkerIDNo = tbl_LbrDataInfo.fld_WorkerIDNo?.ToUpper();//edit by faeza 06.09.2021
                LbrDataInfo.fld_SupplierCode = tbl_LbrDataInfo.fld_SupplierCode;
                LbrDataInfo.fld_ArrivedDT = tbl_LbrDataInfo.fld_ArrivedDT;
                LbrDataInfo.fld_ActiveStatus = "1";
                LbrDataInfo.fld_InactiveReason = null;
                LbrDataInfo.fld_InactiveDT = null;
                LbrDataInfo.fld_Roc = tbl_LbrDataInfo.fld_Roc;
                LbrDataInfo.fld_BankCode = tbl_LbrDataInfo.fld_BankCode;
                LbrDataInfo.fld_BankAcc = tbl_LbrDataInfo.fld_BankAcc;
                LbrDataInfo.fld_PerkesoNo = tbl_LbrDataInfo.fld_PerkesoNo;
                LbrDataInfo.fld_PaymentMode = tbl_LbrDataInfo.fld_PaymentMode;
                LbrDataInfo.fld_Last4Pan = tbl_LbrDataInfo.fld_Last4Pan;

                //Added by Shazana 29/3/2024
                LbrDataInfo.fld_PassportStatus = tbl_LbrDataInfo.fld_PassportStatus;
                LbrDataInfo.fld_PassportRenewalStartDate = tbl_LbrDataInfo.fld_PassportRenewalStartDate;
                LbrDataInfo.fld_PassportRenewalStatus = tbl_LbrDataInfo.fld_PassportRenewalStatus;
                LbrDataInfo.fld_PermitRenewalStartDate = tbl_LbrDataInfo.fld_PermitRenewalStartDate;
                LbrDataInfo.fld_PermitStartDT = tbl_LbrDataInfo.fld_PermitStartDT;
                LbrDataInfo.fld_PermitStatus = tbl_LbrDataInfo.fld_PermitStatus;
                LbrDataInfo.fld_PermitRenewalStatus = tbl_LbrDataInfo.fld_PermitRenewalStatus;
                LbrDataInfo.fld_ContractStartDate = tbl_LbrDataInfo.fld_ContractStartDate;
                LbrDataInfo.fld_ContractExpiryDate = tbl_LbrDataInfo.fld_ContractExpiryDate;

                if (tbl_LbrDataInfo.fld_TmptLhr == null)
                {
                    tbl_LbrDataInfo.fld_TmptLhr = "-";
                }
                LbrDataInfo.fld_TmptLhr = tbl_LbrDataInfo.fld_TmptLhr.ToUpper();
                db.Entry(LbrDataInfo).State = EntityState.Modified;
                await db.SaveChangesAsync();


                //Permit dan passport
                //Passport 2
                var GetExistingPassport = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 2 && x.fld_Deleted == false).OrderByDescending(x=>x.fld_ID).FirstOrDefault();
                if (GetExistingPassport == null)
                { ViewBag.ExistingPassport = 0; }
                else
                {
                    ViewBag.ExistingPassport = 1;
                    GetExistingPassport.fld_PassportRenewalStartDate = tbl_LbrDataInfo.fld_PassportRenewalStartDate;
                    GetExistingPassport.fld_PassportRenewalStatus = tbl_LbrDataInfo.fld_PassportRenewalStatus;
                    GetExistingPassport.fld_PassportStatus = tbl_LbrDataInfo.fld_PassportStatus;
                    GetExistingPassport.fld_ModifiedBy = GetUserID;
                    GetExistingPassport.fld_ModifiedDT = DT;
                    db.Entry(GetExistingPassport).State = EntityState.Modified;
                    db.SaveChanges();
                    PassportRenewalStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PassportRenewalStatus).ToList();

                }

                //Permit 1
                var GetExistingPermit = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 1 && x.fld_Deleted == false).OrderByDescending(x=>x.fld_ID).FirstOrDefault();
                if (GetExistingPermit == null)
                { ViewBag.ExistingPermit = 0; }
                else
                {
                    ViewBag.ExistingPermit = 1;
                    GetExistingPermit.fld_PermitRenewalStatus = tbl_LbrDataInfo.fld_PermitRenewalStatus;
                    GetExistingPermit.fld_PermitRenewalStartDate = tbl_LbrDataInfo.fld_PermitRenewalStartDate;
                    GetExistingPermit.fld_PermitStatus = tbl_LbrDataInfo.fld_PermitStatus;
                    GetExistingPermit.fld_ModifiedBy = GetUserID;
                    GetExistingPermit.fld_ModifiedDT = DT;
                    db.Entry(GetExistingPermit).State = EntityState.Modified;
                    db.SaveChanges();
                }

                bool SyncStatus = SyncToCheckRoll(LbrDataInfo.fld_ID);
                if (SyncStatus)
                {
                    ModelState.AddModelError("", "Update Successfully And Sync To Checkroll");
                    ViewBag.MsgColor = "color: green";
                }
                else
                {
                    ModelState.AddModelError("", "Update Successfully And Failed Sync To Checkroll");
                    ViewBag.MsgColor = "color: orange";
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
                fld_Roc = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "roc" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Roc).ToList();
                fld_BankCode = new SelectList(Masterdb.tbl_Bank.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_KodBank).Select(s => new SelectListItem { Value = s.fld_KodBank, Text = s.fld_NamaBank.ToUpper() }), "Value", "Text", tbl_LbrDataInfo.fld_BankCode).ToList();
                //added by faeza 20.04.2021
                fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();

                //Added by Shazana 29/3/2024
                var GetExistingPassport1 = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 2 && x.fld_Deleted == false).FirstOrDefault();
                if (GetExistingPassport1 == null)
                {
                    ViewBag.ExistingPassport = 0;
                    PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
                }
                else
                {
                    ViewBag.ExistingPassport = 1;

                    PassportRenewalStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PassportRenewalStatus).ToList();
                    PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
                }

                var GetExistingPermit1 = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 1 && x.fld_Deleted == false).FirstOrDefault();
                if (GetExistingPermit1 == null)
                {
                    ViewBag.ExistingPermit = 0;
                    PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
                }
                else
                {
                    ViewBag.ExistingPermit = 1;
                    PermitRenewalStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "permitrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PermitRenewalStatus).ToList();
                    PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
                }
                PassportPermitStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PassportStatus).ToList();
                PassportPermitStatusPermit = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PermitStatus).ToList();

                PassportPermitStatusPermit.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
                PassportPermitStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));


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
                ViewBag.fld_SupplierCode = fld_SupplierCode;
                ViewBag.fld_Roc = fld_Roc;
                ViewBag.fld_BankCode = fld_BankCode;
                ViewBag.fld_PaymentMode = fld_PaymentMode;

                //Added by Shazana 29/3/2024
                ViewBag.fld_PassportRenewalStatus = PassportRenewalStatus;
                ViewBag.fld_PassportStatus = PassportPermitStatus;
                ViewBag.fld_PermitRenewalStatus = PermitRenewalStatus;
                ViewBag.fld_PermitStatus = PassportPermitStatusPermit;

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
                fld_Roc = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "roc" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Roc).ToList();
                fld_BankCode = new SelectList(Masterdb.tbl_Bank.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_KodBank).Select(s => new SelectListItem { Value = s.fld_KodBank, Text = s.fld_NamaBank.ToUpper() }), "Value", "Text", tbl_LbrDataInfo.fld_BankCode).ToList();
                //added by faeza 20.04.2021
                fld_PaymentMode = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "paymentmode" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PaymentMode).ToList();

                //Added by Shazana 29/3/2024
                //Added by Shazana 29/3/2024
                var GetExistingPassport1 = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 2 && x.fld_Deleted == false).FirstOrDefault();
                if (GetExistingPassport1 == null)
                {
                    ViewBag.ExistingPassport = 0;
                    PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
                }
                else
                {
                    ViewBag.ExistingPassport = 1;

                    PassportRenewalStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PassportRenewalStatus).ToList();
                    PassportRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
                }

                var GetExistingPermit1 = db.tbl_LbrPrmtPsprtUpdate.Where(x => x.fld_LbrRefID == tbl_LbrDataInfo.fld_ID && x.fld_PurposeIndicator == 1 && x.fld_Deleted == false).FirstOrDefault();
                if (GetExistingPermit1 == null)
                {
                    ViewBag.ExistingPermit = 0;
                    PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
                }
                else
                {
                    ViewBag.ExistingPermit = 1;
                    PermitRenewalStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "permitrenewalstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PermitRenewalStatus).ToList();
                    PermitRenewalStatus.Insert(0, (new SelectListItem { Text = GlobalResEstate.lblChoose, Value = "" }));
                }
                PassportPermitStatus = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PassportStatus).ToList();
                PassportPermitStatusPermit = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "passportpermitstatus" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_PermitStatus).ToList();
                fld_Roc = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "roc" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", tbl_LbrDataInfo.fld_Roc).ToList();
                fld_BankCode = new SelectList(Masterdb.tbl_Bank.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false).OrderBy(o => o.fld_KodBank).Select(s => new SelectListItem { Value = s.fld_KodBank, Text = s.fld_NamaBank.ToUpper() }), "Value", "Text", tbl_LbrDataInfo.fld_BankCode).ToList();

                //Added by Shazana 29/3/2024
                ViewBag.fld_PassportRenewalStatus = PassportRenewalStatus;
                ViewBag.fld_PassportStatus = PassportPermitStatus;
                ViewBag.fld_PermitRenewalStatus = PermitRenewalStatus;
                ViewBag.fld_PermitStatus = PassportPermitStatusPermit;

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
                ViewBag.fld_SupplierCode = fld_SupplierCode;
                ViewBag.fld_Roc = fld_Roc;
                ViewBag.fld_BankCode = fld_BankCode;
                ViewBag.fld_PaymentMode = fld_PaymentMode;

                return View(tbl_LbrDataInfo);
            }
        }

        public bool SyncToCheckRoll(Guid? ID)
        {
            bool Result = false;
            
            if (ID != Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                GetUserID = GetIdentity.ID(User.Identity.Name);
                GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
                Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
                db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);

                var EstateLabourData = db.tbl_LbrDataInfo.Find(ID);
                string GetBatchNo = "";
                if (EstateLabourData.fld_Nationality == "MA")
                {
                    GetBatchNo = db.tbl_LbrTKTProcess.Join(db.tbl_LbrRqst, j => j.fld_LbrRqstID, k => k.fld_ID, (j, k) => new { k.fld_BatchNo, j.fld_ID }).Where(x => x.fld_ID == EstateLabourData.fld_LbrProcessID).Select(s => s.fld_BatchNo).Distinct().FirstOrDefault();
                }
                else
                {
                    GetBatchNo = db.tbl_LbrTKAProcess.Join(db.tbl_LbrRqst, j => j.fld_LbrRqstID, k => k.fld_ID, (j, k) => new { k.fld_BatchNo, j.fld_ID }).Where(x => x.fld_ID == EstateLabourData.fld_LbrProcessID).Select(s => s.fld_BatchNo).Distinct().FirstOrDefault();
                }
                var DivisionID = Masterdb.tbl_Division.Where(x => x.fld_LadangID == EstateLabourData.fld_LadangID).Select(s => s.fld_ID).FirstOrDefault();
                if (SyncToCheckRollFunc(EstateLabourData.fld_NegaraID, EstateLabourData.fld_SyarikatID, EstateLabourData.fld_WilayahID, EstateLabourData.fld_LadangID, DivisionID, EstateLabourData, GetBatchNo, EstateLabourData.fld_WorkerNo))
                {
                    Result = true;
                }
                else
                {
                    Result = false;
                }
            }

            return Result;
        }

        public bool SyncToCheckRollFunc(int? NegaraID, int? SyarikatID, int? WilayahID, int? LadangID, int? DivisionID, tbl_LbrDataInfo tbl_LbrDataInfo, string GetBatchNo, string NoPkj)
        {
            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            bool Result = false;
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
            tbl_Pkjmast tbl_Pkjmast = new tbl_Pkjmast();
            try
            {
                tbl_Pkjmast = dbEstate.tbl_Pkjmast.Where(x => x.fld_Nopkj == NoPkj && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID).FirstOrDefault();

                tbl_Pkjmast.fld_Nopkj = tbl_LbrDataInfo.fld_WorkerNo;
                tbl_Pkjmast.fld_NegaraID = tbl_LbrDataInfo.fld_NegaraID;
                tbl_Pkjmast.fld_SyarikatID = tbl_LbrDataInfo.fld_SyarikatID;
                tbl_Pkjmast.fld_WilayahID = tbl_LbrDataInfo.fld_WilayahID;
                tbl_Pkjmast.fld_LadangID = tbl_LbrDataInfo.fld_LadangID;
                tbl_Pkjmast.fld_DivisionID = DivisionID;
                tbl_Pkjmast.fld_Nama = tbl_LbrDataInfo.fld_WorkerName;
                tbl_Pkjmast.fld_Kdrkyt = tbl_LbrDataInfo.fld_Nationality;
                tbl_Pkjmast.fld_Kdaktf = tbl_LbrDataInfo.fld_ActiveStatus;
                tbl_Pkjmast.fld_Nokp = tbl_LbrDataInfo.fld_WorkerIDNo;
                tbl_Pkjmast.fld_Neg = tbl_LbrDataInfo.fld_State;
                tbl_Pkjmast.fld_Trmlkj = tbl_LbrDataInfo.fld_WorkingStartDT;
                tbl_Pkjmast.fld_Trshjw = tbl_LbrDataInfo.fld_ConfirmationDT;
                tbl_Pkjmast.fld_Ktgpkj = tbl_LbrDataInfo.fld_WorkCtgry;
                tbl_Pkjmast.fld_Jenispekerja = tbl_LbrDataInfo.fld_WorkerType;
                tbl_Pkjmast.fld_Prmtno = tbl_LbrDataInfo.fld_PermitNo;
                tbl_Pkjmast.fld_T2prmt = tbl_LbrDataInfo.fld_PermitEndDT;
                tbl_Pkjmast.fld_T2pspt = tbl_LbrDataInfo.fld_PassportEndDT;
                tbl_Pkjmast.fld_Trlhr = tbl_LbrDataInfo.fld_BOD;
                tbl_Pkjmast.fld_StatusApproved = 1;
                tbl_Pkjmast.fld_Batch = GetBatchNo;
                tbl_Pkjmast.fld_LbrRefID = tbl_LbrDataInfo.fld_ID;

                tbl_Pkjmast.fld_Almt1 = tbl_LbrDataInfo.fld_WorkerAddress;
                tbl_Pkjmast.fld_Neg = tbl_LbrDataInfo.fld_State;
                tbl_Pkjmast.fld_Negara = tbl_LbrDataInfo.fld_Country;
                tbl_Pkjmast.fld_Poskod = tbl_LbrDataInfo.fld_Postcode;
                tbl_Pkjmast.fld_Notel = tbl_LbrDataInfo.fld_PhoneNo;
                tbl_Pkjmast.fld_Kdjnt = tbl_LbrDataInfo.fld_SexType;
                tbl_Pkjmast.fld_Kdbgsa = tbl_LbrDataInfo.fld_Nationality;
                tbl_Pkjmast.fld_Kdagma = tbl_LbrDataInfo.fld_Religion;
                tbl_Pkjmast.fld_Kdkwn = tbl_LbrDataInfo.fld_MarriedStatus;
                tbl_Pkjmast.fld_Trtakf = tbl_LbrDataInfo.fld_InactiveDT;
                tbl_Pkjmast.fld_Sbtakf = tbl_LbrDataInfo.fld_InactiveReason;
                tbl_Pkjmast.fld_Kodbkl = tbl_LbrDataInfo.fld_SupplierCode;
                tbl_Pkjmast.fld_Noperkeso = tbl_LbrDataInfo.fld_PerkesoNo;
                tbl_Pkjmast.fld_Kdbank = tbl_LbrDataInfo.fld_BankCode;
                tbl_Pkjmast.fld_NoAkaun = tbl_LbrDataInfo.fld_BankAcc;
                tbl_Pkjmast.fld_Psptno = tbl_LbrDataInfo.fld_WorkerIDNo;
                //added by faeza 20.04.2021
                tbl_Pkjmast.fld_PaymentMode = tbl_LbrDataInfo.fld_PaymentMode;
                tbl_Pkjmast.fld_Last4Pan = tbl_LbrDataInfo.fld_Last4Pan;


                dbEstate.Entry(tbl_Pkjmast).State = EntityState.Modified;
                dbEstate.SaveChanges();
                Result = true;
            }
            catch (Exception ex)
            {
                Result = false;
            }
            finally
            {
                dbEstate.Dispose();
            }

            return Result;
        }

        public ActionResult _LabourRelationshipDetail(Guid LabourID)
        {
            ViewBag.fld_LbrRefID = LabourID;
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            tbl_LbrRelationInfo LbrRelationInfo = new tbl_LbrRelationInfo();
            List<SelectListItem> fld_Relationship = new List<SelectListItem>();
            string[] WebConfigFilter = new string[] { "relationship" };
            string Relationship = "";

            var GetDropdownList = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();
            LbrRelationInfo = db.tbl_LbrRelationInfo.Where(x => x.fld_LbrRefID == LabourID).FirstOrDefault();
            if (LbrRelationInfo == null)
            {
                Relationship = "0";
            }
            else
            {
                Relationship = LbrRelationInfo.fld_Relationship;
            }
            fld_Relationship = new SelectList(GetDropdownList.Where(x => x.fldOptConfFlag1 == "relationship" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).OrderBy(o => o.fldOptConfValue).Select(s => new SelectListItem { Value = s.fldOptConfValue, Text = s.fldOptConfDesc.ToUpper() }).Distinct(), "Value", "Text", Relationship).ToList();
            ViewBag.fld_Relationship = fld_Relationship;
            return View(LbrRelationInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _LabourRelationshipDetail(tbl_LbrRelationInfo LbrRelationInfo)
        {
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
            db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);
            string Msg = "";
            string Status = "";

            var CheckExisting = db.tbl_LbrRelationInfo.Where(x => x.fld_LbrRefID == LbrRelationInfo.fld_LbrRefID).FirstOrDefault();
            if (CheckExisting == null)
            {
                tbl_LbrRelationInfo tbl_LbrRelationInfo = new tbl_LbrRelationInfo();
                tbl_LbrRelationInfo.fld_LbrRefID = LbrRelationInfo.fld_LbrRefID;
                tbl_LbrRelationInfo.fld_Name = LbrRelationInfo.fld_Name;
                tbl_LbrRelationInfo.fld_Address = LbrRelationInfo.fld_Address;
                tbl_LbrRelationInfo.fld_PhoneNo = LbrRelationInfo.fld_PhoneNo;
                tbl_LbrRelationInfo.fld_Relationship = LbrRelationInfo.fld_Relationship;
                db.tbl_LbrRelationInfo.Add(tbl_LbrRelationInfo);
            }
            else
            {
                CheckExisting.fld_Name = LbrRelationInfo.fld_Name;
                CheckExisting.fld_Address = LbrRelationInfo.fld_Address;
                CheckExisting.fld_PhoneNo = LbrRelationInfo.fld_PhoneNo;
                CheckExisting.fld_Relationship = LbrRelationInfo.fld_Relationship;
                db.Entry(CheckExisting).State = EntityState.Modified;
            }
            try
            {
                db.SaveChanges();
                Msg = "Successfully updated";
                Status = "success";
            }
            catch(Exception ex)
            {
                Msg = "Failed to update";
                Status = "danger";
            }
            

            return Json(new { Msg, Status });
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