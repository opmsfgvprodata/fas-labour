using Itenso.TimePeriod;
using MVC_SYSTEM.Attributes;
using MVC_SYSTEM.Class;
using MVC_SYSTEM.EstateModels;
using MVC_SYSTEM.LabourModels;
using MVC_SYSTEM.MasterModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_SYSTEM.Controllers
{
    [AccessDeniedAuthorizeAttribute(Roles = "Super Power Admin,Super Admin,Admin 1,Admin 2,Admin 3,Super Power User,Super User,Normal User")]
    public class AjaxRequestController : Controller
    {
        private MVC_SYSTEM_MasterModels Masterdb = new MVC_SYSTEM_MasterModels();
        private MVC_SYSTEM_Models db = new MVC_SYSTEM_Models();
        private GetIdentity GetIdentity = new GetIdentity();
        private GetNSWL GetNSWL = new GetNSWL();
        private Connection Connection = new Connection();
        private GetLadang GetLadang = new GetLadang();
        string Host, Catalog, UserID, Pass = "";
        string Purpose = "LABOUR";
        int? NegaraID, SyarikatID, WilayahID, LadangID, GetUserID = 0;

        public JsonResult GetLadangList(int WilayahIDParam)
        {
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            List<SelectListItem> LadangIDReturn = new List<SelectListItem>();
            
            var GetLadangList = Masterdb.tbl_Ladang.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WlyhID == WilayahIDParam && x.fld_Deleted == false).ToList();
            LadangIDReturn = new SelectList(GetLadangList.Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_LdgCode.Trim() + " - " + s.fld_LdgName.Trim() }), "Value", "Text").ToList();
            LadangIDReturn.Insert(0, (new SelectListItem { Text = "All", Value = "0" }));

            return Json(LadangIDReturn);
        }

        public JsonResult GetWorkerNo(Guid? ID)
        {
            string nopkjnew = "";

            if (ID != Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                GetUserID = GetIdentity.ID(User.Identity.Name);
                GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
                Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
                db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);

                var EstateID = db.tbl_LbrDataInfo.Find(ID);
                if (EstateID.fld_WorkerNo == null || EstateID.fld_WorkerNo == "")
                {
                    string kuota = "P";
                    string warganegara = EstateID.fld_Nationality;
                    string kodldg = GetLadang.GetLadangCode(EstateID.fld_LadangID.Value);
                    string thnKemasukan = DateTime.Now.ToString("yy");

                    int kod = 0;

                    if (warganegara != "0")
                    {
                        if (warganegara == "ID")
                        {
                            warganegara = warganegara.Substring(1, 1);
                        }
                        else
                        {
                            warganegara = warganegara.Substring(0, 1);
                        }
                        nopkjnew = kuota + warganegara + kodldg + thnKemasukan;
                        var getpkj = GetWorkerFromEstateData(EstateID.fld_NegaraID, EstateID.fld_SyarikatID, EstateID.fld_WilayahID, EstateID.fld_LadangID, nopkjnew);

                        if (getpkj != null)
                        {
                            kod = Convert.ToInt32(getpkj.Substring(getpkj.Length - 3));
                        }
                        else
                        {
                            kod = 0;
                        }
                        kod = kod + 1;
                        nopkjnew = nopkjnew + kod.ToString("000");
                    }
                    else
                    {
                        nopkjnew = "";
                    }
                }
                else
                {
                    nopkjnew = EstateID.fld_WorkerNo;
                }
            }
            return Json(nopkjnew);
        }

        public JsonResult GetWorkerNo2(Guid? ID)
        {
            string nopkjnew = "";

            if (ID != Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                GetUserID = GetIdentity.ID(User.Identity.Name);
                GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
                Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
                db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);

                var EstateID = db.vw_LbrTrnsferData.Find(ID);
                if (string.IsNullOrEmpty(EstateID.fld_NewWorkerNo))
                {
                    string kuota = "P";
                    string warganegara = EstateID.fld_Nationality;
                    string kodldg = GetLadang.GetLadangCode(EstateID.fld_LadangIDTo.Value);
                    string thnKemasukan = DateTime.Now.ToString("yy");

                    int kod = 0;

                    if (warganegara != "0")
                    {
                        if (warganegara == "ID")
                        {
                            warganegara = warganegara.Substring(1, 1);
                        }
                        else
                        {
                            warganegara = warganegara.Substring(0, 1);
                        }
                        nopkjnew = kuota + warganegara + kodldg + thnKemasukan;
                        var getpkj = GetWorkerFromEstateData(EstateID.fld_NegaraIDTo, EstateID.fld_SyarikatIDTo, EstateID.fld_WilayahIDTo, EstateID.fld_LadangIDTo, nopkjnew);

                        if (getpkj != null)
                        {
                            kod = Convert.ToInt32(getpkj.Substring(getpkj.Length - 3));
                        }
                        else
                        {
                            kod = 0;
                        }
                        kod = kod + 1;
                        nopkjnew = nopkjnew + kod.ToString("000");
                    }
                    else
                    {
                        nopkjnew = "";
                    }
                }
                else
                {
                    nopkjnew = EstateID.fld_NewWorkerNo;
                }
            }
            return Json(nopkjnew);
        }

        public string GetWorkerFromEstateData(int? NegaraID, int? SyarikatID, int? WilayahID, int? LadangID, string nopkjnew)
        {
            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);

            var getpkj = dbEstate.tbl_Pkjmast.Where(x => x.fld_Nopkj.Contains(nopkjnew) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID).Select(s => s.fld_Nopkj.Trim()).Distinct().OrderByDescending(s => s).FirstOrDefault();
            
            dbEstate.Dispose();

            return getpkj;
        }

        public JsonResult TransferToCheckRoll(Guid? ID, string CostCenter)
        {
            string msg = "";
            string statusmsg = "";
            if (ID != Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                GetUserID = GetIdentity.ID(User.Identity.Name);
                GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
                Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
                db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);

                var EstateLabourData = db.tbl_LbrDataInfo.Find(ID);
                if (!string.IsNullOrEmpty(EstateLabourData.fld_WorkerNo) && EstateLabourData.fld_TransferToChckrollStatus == null)
                {
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
                    if (TransferToCheckRollFunc(EstateLabourData.fld_NegaraID, EstateLabourData.fld_SyarikatID, EstateLabourData.fld_WilayahID, EstateLabourData.fld_LadangID, DivisionID, EstateLabourData, GetBatchNo, CostCenter))
                    {
                        EstateLabourData.fld_TransferToChckrollStatus = true;
                        db.Entry(EstateLabourData).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = "Transfer successfully";
                        statusmsg = "success";
                    }
                    else
                    {
                        msg = "Transfer failure";
                        statusmsg = "warning";
                    }
                }
                else
                {
                    if (EstateLabourData.fld_TransferToChckrollStatus == true)
                    {
                        msg = "Data already transfered";
                        statusmsg = "warning";
                    }
                    else
                    {
                        msg = "Please generate worker no then save";
                        statusmsg = "warning";
                    }
                }
            }
            else
            {
                msg = "Please save the data before transfer";
                statusmsg = "warning";
            }

            return Json(new { msg, statusmsg });
        }

        public JsonResult TransferToCheckRoll2(Guid? ID, string CostCenter)
        {
            string msg = "";
            string statusmsg = "";
            if (ID != Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                GetUserID = GetIdentity.ID(User.Identity.Name);
                GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
                Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose);
                db = MVC_SYSTEM_Models.ConnectToSqlServer(Host, Catalog, UserID, Pass);

                var GetTransferData = db.tbl_LbrTrnsferHistory.Find(ID);

                var EstateLabourData = db.tbl_LbrDataInfo.Find(GetTransferData.fld_LbrRefID);
                if (!string.IsNullOrEmpty(GetTransferData.fld_NewWorkerNo) && EstateLabourData.fld_TransferToChckrollWorkerTransferStatus == false)
                {
                    string GetBatchNo = "";
                    if (EstateLabourData.fld_Nationality == "MA")
                    {
                        GetBatchNo = db.tbl_LbrTKTProcess.Join(db.tbl_LbrRqst, j => j.fld_LbrRqstID, k => k.fld_ID, (j, k) => new { k.fld_BatchNo, j.fld_ID }).Where(x => x.fld_ID == EstateLabourData.fld_LbrProcessID).Select(s => s.fld_BatchNo).Distinct().FirstOrDefault();
                    }
                    else
                    {
                        GetBatchNo = db.tbl_LbrTKAProcess.Join(db.tbl_LbrRqst, j => j.fld_LbrRqstID, k => k.fld_ID, (j, k) => new { k.fld_BatchNo, j.fld_ID }).Where(x => x.fld_ID == EstateLabourData.fld_LbrProcessID).Select(s => s.fld_BatchNo).Distinct().FirstOrDefault();
                    }
                    var DivisionID = Masterdb.tbl_Division.Where(x => x.fld_LadangID == GetTransferData.fld_LadangIDTo).Select(s => s.fld_ID).FirstOrDefault();
                    if (TransferToCheckRollFunc2(GetTransferData.fld_NegaraIDTo, GetTransferData.fld_SyarikatIDTo, GetTransferData.fld_WilayahIDTo, GetTransferData.fld_LadangIDTo, DivisionID, EstateLabourData, GetBatchNo, GetTransferData.fld_NewWorkerNo, CostCenter))
                    {
                        EstateLabourData.fld_TransferToChckrollWorkerTransferStatus = true;
                        EstateLabourData.fld_WorkerNo = GetTransferData.fld_NewWorkerNo;
                        EstateLabourData.fld_NegaraID = GetTransferData.fld_NegaraIDTo;
                        EstateLabourData.fld_SyarikatID = GetTransferData.fld_SyarikatIDTo;
                        EstateLabourData.fld_WilayahID = GetTransferData.fld_WilayahIDTo;
                        EstateLabourData.fld_LadangID = GetTransferData.fld_LadangIDTo;
                        db.Entry(EstateLabourData).State = EntityState.Modified;
                        db.SaveChanges();

                        GetTransferData.fld_SuccessTransferd = true;
                        db.Entry(GetTransferData).State = EntityState.Modified;
                        db.SaveChanges();

                        msg = "Transfer successfully";
                        statusmsg = "success";
                    }
                    else
                    {
                        msg = "Transfer failure";
                        statusmsg = "warning";
                    }
                }
                else
                {
                    if (EstateLabourData.fld_TransferToChckrollWorkerTransferStatus == true)
                    {
                        msg = "Data already transfered";
                        statusmsg = "warning";
                    }
                    else
                    {
                        msg = "Please generate worker no then save";
                        statusmsg = "warning";
                    }
                }
            }
            else
            {
                msg = "Please save the data before transfer";
                statusmsg = "warning";
            }

            return Json(new { msg, statusmsg });
        }

        public bool TransferToCheckRollFunc(int? NegaraID, int? SyarikatID, int? WilayahID, int? LadangID, int? DivisionID, tbl_LbrDataInfo tbl_LbrDataInfo, string GetBatchNo, string CostCenter)
        {
            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            bool Result = false;
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
            tbl_Pkjmast tbl_Pkjmast = new tbl_Pkjmast();

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
            //add by faeza 20.04.2021
            tbl_Pkjmast.fld_PaymentMode = tbl_LbrDataInfo.fld_PaymentMode;
            tbl_Pkjmast.fld_Last4Pan = tbl_LbrDataInfo.fld_Last4Pan;

            tbl_Pkjmast.fld_KodSAPPekerja = CostCenter;

            try
            {
                dbEstate.tbl_Pkjmast.Add(tbl_Pkjmast);
                dbEstate.SaveChanges();
                GetOtherRelatedDataPjkmast(dbEstate, NegaraID.Value, SyarikatID.Value, WilayahID.Value, LadangID.Value, tbl_LbrDataInfo.fld_WorkerNo);
                Result = true;
            }
            catch(Exception ex)
            {
                Result = false;
            }
            finally
            {
                dbEstate.Dispose();
            }
            
            return Result;
        }

        public bool TransferToCheckRollFunc2(int? NegaraID, int? SyarikatID, int? WilayahID, int? LadangID, int? DivisionID, tbl_LbrDataInfo tbl_LbrDataInfo, string GetBatchNo, string WorkerNo, string CostCenter)
        {
            string Host1, Catalog1, UserID1, Pass1 = "";
            string Host2, Catalog2, UserID2, Pass2 = "";
            string Purpose2 = "CHECKROLL";
            bool Result = false;
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);

            MVC_SYSTEM_EstateModels dbEstate2 = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host2, out Catalog2, out UserID2, out Pass2, tbl_LbrDataInfo.fld_WilayahID.Value, tbl_LbrDataInfo.fld_SyarikatID.Value, tbl_LbrDataInfo.fld_NegaraID.Value, Purpose2);
            dbEstate2 = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host2, Catalog2, UserID2, Pass2);
            tbl_Pkjmast tbl_Pkjmast = new tbl_Pkjmast();
            tbl_Pkjmast tbl_Pkjmast2 = new tbl_Pkjmast();

            tbl_Pkjmast.fld_Nopkj = WorkerNo;
            tbl_Pkjmast.fld_NegaraID = NegaraID;
            tbl_Pkjmast.fld_SyarikatID = SyarikatID;
            tbl_Pkjmast.fld_WilayahID = WilayahID;
            tbl_Pkjmast.fld_LadangID = LadangID;
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
            tbl_Pkjmast.fld_KodSAPPekerja = CostCenter;
            //add by faeza 28.05.2021
            tbl_Pkjmast.fld_Notel = tbl_LbrDataInfo.fld_PhoneNo;
            tbl_Pkjmast.fld_PaymentMode = tbl_LbrDataInfo.fld_PaymentMode;
            tbl_Pkjmast.fld_Last4Pan = tbl_LbrDataInfo.fld_Last4Pan;
            //add by faeza 11.06.2021
            tbl_Pkjmast.fld_Noperkeso = tbl_LbrDataInfo.fld_PerkesoNo;

            tbl_Pkjmast2 = dbEstate2.tbl_Pkjmast.Where(x => x.fld_Nopkj == tbl_LbrDataInfo.fld_WorkerNo && x.fld_NegaraID == tbl_LbrDataInfo.fld_NegaraID && x.fld_SyarikatID == tbl_LbrDataInfo.fld_SyarikatID && x.fld_WilayahID == tbl_LbrDataInfo.fld_WilayahID && x.fld_LadangID == tbl_LbrDataInfo.fld_LadangID).FirstOrDefault();
            tbl_Pkjmast2.fld_Kdaktf = "2";
            tbl_Pkjmast2.fld_Sbtakf = "01";

            try
            {
                dbEstate2.Entry(tbl_Pkjmast2).State = EntityState.Modified;
                dbEstate2.SaveChanges();

                dbEstate.tbl_Pkjmast.Add(tbl_Pkjmast);
                dbEstate.SaveChanges();
                GetOtherRelatedDataPjkmast2(dbEstate, dbEstate2, NegaraID.Value, SyarikatID.Value, WilayahID.Value, LadangID.Value, WorkerNo, tbl_LbrDataInfo.fld_NegaraID.Value, tbl_LbrDataInfo.fld_SyarikatID.Value, tbl_LbrDataInfo.fld_WilayahID.Value, tbl_LbrDataInfo.fld_LadangID.Value, tbl_LbrDataInfo.fld_WorkerNo);
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

        public bool GetOtherRelatedDataPjkmast(MVC_SYSTEM_EstateModels dbr, int NegaraID, int SyarikatID, int WilayahID, int LadangID, string NoPkj)
        {
            bool Result = false;
            int year = DateTime.Now.Year;
            DateTime lastDay = new DateTime(year, 12, 31);
            string[] WebConfigFilter = new string[] { "kodCutiTahunan", "kodCutiSakit", "kodCutiAm", "kodCarumanSocso", "kodCarumanKwsp" };
            var tblOptionConfigsWebs = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            try
            {
                var app2 = dbr.tbl_Pkjmast.Where(x => x.fld_Nopkj == NoPkj && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID).FirstOrDefault();
                if (app2 != null)
                {
                    app2.fld_Kdaktf = "1";
                    app2.fld_StatusAkaun = "1";
                    app2.fld_StatusApproved = 1;
                    dbr.SaveChanges();
                }

                //calculate annual leave
                DateDiff dateDiff = new DateDiff(Convert.ToDateTime(app2.fld_Trmlkj).AddDays(-1), lastDay);
                var kodCutiTahunan = tblOptionConfigsWebs.SingleOrDefault(x => x.fldOptConfFlag1 == "kodCutiTahunan" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false);
                var cutiTahunanPkj = Masterdb.tbl_CutiMaintenance.Where(x => x.fld_JenisCuti == kodCutiTahunan.fldOptConfValue && x.fld_Deleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_LowerLimit <= dateDiff.Months && x.fld_UpperLimit >= dateDiff.Months).Select(s => s.fld_PeruntukkanCuti).FirstOrDefault();

                if (!dbr.tbl_CutiPeruntukan.Any(x => x.fld_NoPkj == NoPkj && x.fld_KodCuti == kodCutiTahunan.fldOptConfValue && x.fld_Tahun == year && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Deleted == false))
                {
                    tbl_CutiPeruntukan CutiPeruntukanTahunan = new tbl_CutiPeruntukan();
                    CutiPeruntukanTahunan.fld_NoPkj = NoPkj;
                    CutiPeruntukanTahunan.fld_JumlahCuti = cutiTahunanPkj;
                    CutiPeruntukanTahunan.fld_KodCuti = kodCutiTahunan.fldOptConfValue;
                    CutiPeruntukanTahunan.fld_JumlahCutiDiambil = 0;
                    CutiPeruntukanTahunan.fld_Tahun = Convert.ToInt16(year);
                    CutiPeruntukanTahunan.fld_NegaraID = NegaraID;
                    CutiPeruntukanTahunan.fld_SyarikatID = SyarikatID;
                    CutiPeruntukanTahunan.fld_WilayahID = WilayahID;
                    CutiPeruntukanTahunan.fld_LadangID = LadangID;
                    CutiPeruntukanTahunan.fld_Deleted = false;
                    dbr.tbl_CutiPeruntukan.Add(CutiPeruntukanTahunan);
                    dbr.SaveChanges();
                }

                //calculate sick leave
                var kodCutiSakit = tblOptionConfigsWebs.SingleOrDefault(x => x.fldOptConfFlag1 == "kodCutiSakit" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false);
                var cutiSakitPkj = Masterdb.tbl_CutiMaintenance.Where(x => x.fld_JenisCuti == kodCutiSakit.fldOptConfValue && x.fld_Deleted == false && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_LowerLimit <= dateDiff.Years && x.fld_UpperLimit >= dateDiff.Years).Select(s => s.fld_PeruntukkanCuti).Single();

                if (!dbr.tbl_CutiPeruntukan.Any(x => x.fld_NoPkj == NoPkj && x.fld_KodCuti == kodCutiSakit.fldOptConfValue && x.fld_Tahun == year && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Deleted == false))
                {
                    tbl_CutiPeruntukan CutiPeruntukanSakit = new tbl_CutiPeruntukan();
                    CutiPeruntukanSakit.fld_NoPkj = NoPkj;
                    CutiPeruntukanSakit.fld_JumlahCuti = cutiSakitPkj;
                    CutiPeruntukanSakit.fld_KodCuti = kodCutiSakit.fldOptConfValue;
                    CutiPeruntukanSakit.fld_JumlahCutiDiambil = 0;
                    CutiPeruntukanSakit.fld_Tahun = Convert.ToInt16(year);
                    CutiPeruntukanSakit.fld_NegaraID = NegaraID;
                    CutiPeruntukanSakit.fld_SyarikatID = SyarikatID;
                    CutiPeruntukanSakit.fld_WilayahID = WilayahID;
                    CutiPeruntukanSakit.fld_LadangID = LadangID;
                    CutiPeruntukanSakit.fld_Deleted = false;
                    dbr.tbl_CutiPeruntukan.Add(CutiPeruntukanSakit);
                    dbr.SaveChanges();
                }

                var kodCutiUmum = tblOptionConfigsWebs.SingleOrDefault(x => x.fldOptConfFlag1 == "kodCutiAm" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false);

                if (!dbr.tbl_CutiPeruntukan.Any(x => x.fld_NoPkj == NoPkj && x.fld_KodCuti == kodCutiUmum.fldOptConfValue && x.fld_Tahun == year && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Deleted == false))
                {
                    tbl_CutiPeruntukan CutiPeruntukanUmum = new tbl_CutiPeruntukan();
                    CutiPeruntukanUmum.fld_NoPkj = NoPkj;
                    CutiPeruntukanUmum.fld_JumlahCuti = 11;
                    CutiPeruntukanUmum.fld_KodCuti = kodCutiUmum.fldOptConfValue;
                    CutiPeruntukanUmum.fld_JumlahCutiDiambil = 0;
                    CutiPeruntukanUmum.fld_Tahun = Convert.ToInt16(year);
                    CutiPeruntukanUmum.fld_NegaraID = NegaraID;
                    CutiPeruntukanUmum.fld_SyarikatID = SyarikatID;
                    CutiPeruntukanUmum.fld_WilayahID = WilayahID;
                    CutiPeruntukanUmum.fld_LadangID = LadangID;
                    CutiPeruntukanUmum.fld_Deleted = false;
                    dbr.tbl_CutiPeruntukan.Add(CutiPeruntukanUmum);
                    dbr.SaveChanges();
                }

                //kwsp & socso
                if (app2.fld_Kdrkyt != "MA")
                {
                    DateDiff umurPekerja = new DateDiff(Convert.ToDateTime(app2.fld_Trlhr).AddDays(-1), lastDay);

                    if (app2 != null)
                    {
                        app2.fld_StatusKwspSocso = "2";
                        dbr.SaveChanges();
                    }

                    var activeContributionCategoryData = Masterdb.tbl_CarumanTambahan.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false && x.fld_Default == true && x.fld_Warganegara == 2).ToList();

                    foreach (var activeContribution in activeContributionCategoryData)
                    {
                        var activeSubContributionCategoryData = Masterdb.tbl_SubCarumanTambahan.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false && x.fld_KodCaruman == activeContribution.fld_KodCaruman);
                        foreach (var activeSubContribution in activeSubContributionCategoryData)
                        {
                            if (activeSubContribution.fld_UmurLower <= umurPekerja.Years && activeSubContribution.fld_UmurUpper >= umurPekerja.Years)
                            {
                                if (!dbr.tbl_PkjCarumanTambahan.Any(x => x.fld_Nopkj == NoPkj && x.fld_KodCaruman == activeContribution.fld_KodCaruman && x.fld_KodSubCaruman == activeSubContribution.fld_KodSubCaruman && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Deleted == false))
                                {
                                    tbl_PkjCarumanTambahan pkjCarumanTambahan = new tbl_PkjCarumanTambahan();
                                    pkjCarumanTambahan.fld_Nopkj = NoPkj;
                                    pkjCarumanTambahan.fld_KodCaruman = activeContribution.fld_KodCaruman;
                                    pkjCarumanTambahan.fld_KodSubCaruman = activeSubContribution.fld_KodSubCaruman;
                                    pkjCarumanTambahan.fld_NegaraID = NegaraID;
                                    pkjCarumanTambahan.fld_SyarikatID = SyarikatID;
                                    pkjCarumanTambahan.fld_WilayahID = WilayahID;
                                    pkjCarumanTambahan.fld_LadangID = LadangID;
                                    pkjCarumanTambahan.fld_Deleted = false;
                                    dbr.tbl_PkjCarumanTambahan.Add(pkjCarumanTambahan);
                                    dbr.SaveChanges();
                                }
                            }
                        }
                    }
                }
                else if (app2.fld_Kdrkyt == "MA")
                {
                    DateDiff umurPekerja = new DateDiff(Convert.ToDateTime(app2.fld_Trlhr).AddDays(-1), lastDay);
                    var jnsSocso = tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "kodCarumanSocso" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).Select(s => s.fldOptConfValue).FirstOrDefault();
                    var kodSocso = Masterdb.tbl_JenisCaruman.Where(x => x.fld_UmurLower <= umurPekerja.Years && x.fld_UmurUpper >= umurPekerja.Years && x.fld_JenisCaruman == jnsSocso && x.fld_Default == true).Select(s => s.fld_KodCaruman).FirstOrDefault();
                    var jnsKwsp = tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "kodCarumanKwsp" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).Select(s => s.fldOptConfValue).FirstOrDefault();
                    var kodKwsp = Masterdb.tbl_JenisCaruman.Where(x => x.fld_UmurLower <= umurPekerja.Years && x.fld_UmurUpper >= umurPekerja.Years && x.fld_JenisCaruman == jnsKwsp && x.fld_Default == true).Select(s => s.fld_KodCaruman).FirstOrDefault();

                    if (app2 != null)
                    {
                        app2.fld_KodSocso = kodSocso;
                        app2.fld_KodKWSP = kodKwsp;
                        app2.fld_StatusKwspSocso = "1";
                        dbr.SaveChanges();
                    }

                    //sip
                    var activeContributionCategoryData = Masterdb.tbl_CarumanTambahan.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false && x.fld_Default == true && x.fld_Warganegara == 1).ToList();

                    foreach (var activeContribution in activeContributionCategoryData)
                    {
                        var activeSubContributionCategoryData = Masterdb.tbl_SubCarumanTambahan.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false && x.fld_KodCaruman == activeContribution.fld_KodCaruman);
                        foreach (var activeSubContribution in activeSubContributionCategoryData)
                        {
                            if (activeSubContribution.fld_UmurLower <= umurPekerja.Years && activeSubContribution.fld_UmurUpper >= umurPekerja.Years)
                            {
                                if (!dbr.tbl_PkjCarumanTambahan.Any(x => x.fld_Nopkj == NoPkj && x.fld_KodCaruman == activeContribution.fld_KodCaruman && x.fld_KodSubCaruman == activeSubContribution.fld_KodSubCaruman && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Deleted == false))
                                {
                                    tbl_PkjCarumanTambahan pkjCarumanTambahan = new tbl_PkjCarumanTambahan();
                                    pkjCarumanTambahan.fld_Nopkj = NoPkj;
                                    pkjCarumanTambahan.fld_KodCaruman = activeContribution.fld_KodCaruman;
                                    pkjCarumanTambahan.fld_KodSubCaruman = activeSubContribution.fld_KodSubCaruman;
                                    pkjCarumanTambahan.fld_NegaraID = NegaraID;
                                    pkjCarumanTambahan.fld_SyarikatID = SyarikatID;
                                    pkjCarumanTambahan.fld_WilayahID = WilayahID;
                                    pkjCarumanTambahan.fld_LadangID = LadangID;
                                    pkjCarumanTambahan.fld_Deleted = false;
                                    dbr.tbl_PkjCarumanTambahan.Add(pkjCarumanTambahan);
                                    dbr.SaveChanges();
                                }
                            }
                        }
                    }
                }

                Result = true;
            }
            catch (Exception ex)
            {
                Result = false;
            }

            return Result;
        }

        public bool GetOtherRelatedDataPjkmast2(MVC_SYSTEM_EstateModels dbr, MVC_SYSTEM_EstateModels dbr2, int NegaraID, int SyarikatID, int WilayahID, int LadangID, string NoPkj, int NegaraID2, int SyarikatID2, int WilayahID2, int LadangID2, string NoPkj2)
        {
            bool Result = false;
            int year = DateTime.Now.Year;
            DateTime lastDay = new DateTime(year, 12, 31);
            string[] WebConfigFilter = new string[] { "kodCutiTahunan", "kodCutiSakit", "kodCutiAm", "kodCarumanSocso", "kodCarumanKwsp" };
            var tblOptionConfigsWebs = Masterdb.tblOptionConfigsWebs.Where(x => WebConfigFilter.Contains(x.fldOptConfFlag1) && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).ToList();

            try
            {
                var app2 = dbr.tbl_Pkjmast.Where(x => x.fld_Nopkj == NoPkj && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID).FirstOrDefault();
                if (app2 != null)
                {
                    app2.fld_Kdaktf = "1";
                    app2.fld_StatusAkaun = "1";
                    app2.fld_StatusApproved = 1;
                    dbr.SaveChanges();
                }

                //update cuti
                var CutiTransfer = dbr2.tbl_CutiPeruntukan.Where(x => x.fld_NoPkj == NoPkj2 && x.fld_Tahun == year && x.fld_NegaraID == NegaraID2 && x.fld_SyarikatID == SyarikatID2 && x.fld_WilayahID == WilayahID2 && x.fld_LadangID == LadangID2 && x.fld_Deleted == false).ToList();
                if (CutiTransfer.Count() != 0)
                {
                    foreach (var item in CutiTransfer)
                    {
                        tbl_CutiPeruntukan tblLeaves = new tbl_CutiPeruntukan();
                        tblLeaves.fld_KodCuti = item.fld_KodCuti;
                        tblLeaves.fld_NoPkj = NoPkj;
                        tblLeaves.fld_Tahun = item.fld_Tahun;
                        tblLeaves.fld_JumlahCuti = item.fld_JumlahCuti;
                        tblLeaves.fld_JumlahCutiDiambil = item.fld_JumlahCutiDiambil;
                        tblLeaves.fld_NegaraID = NegaraID;
                        tblLeaves.fld_SyarikatID = SyarikatID;
                        tblLeaves.fld_WilayahID = WilayahID;
                        tblLeaves.fld_LadangID = LadangID;
                        tblLeaves.fld_Deleted = false;
                        dbr.tbl_CutiPeruntukan.Add(tblLeaves);
                        dbr.SaveChanges();
                    }
                }
                
                //kwsp & socso
                if (app2.fld_Kdrkyt != "MA")
                {
                    DateDiff umurPekerja = new DateDiff(Convert.ToDateTime(app2.fld_Trlhr).AddDays(-1), lastDay);

                    if (app2 != null)
                    {
                        app2.fld_StatusKwspSocso = "2";
                        dbr.SaveChanges();
                    }

                    var activeContributionCategoryData = Masterdb.tbl_CarumanTambahan.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false && x.fld_Default == true && x.fld_Warganegara == 2).ToList();

                    foreach (var activeContribution in activeContributionCategoryData)
                    {
                        var activeSubContributionCategoryData = Masterdb.tbl_SubCarumanTambahan.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false && x.fld_KodCaruman == activeContribution.fld_KodCaruman);
                        foreach (var activeSubContribution in activeSubContributionCategoryData)
                        {
                            if (activeSubContribution.fld_UmurLower <= umurPekerja.Years && activeSubContribution.fld_UmurUpper >= umurPekerja.Years)
                            {
                                if (!dbr.tbl_PkjCarumanTambahan.Any(x => x.fld_Nopkj == NoPkj && x.fld_KodCaruman == activeContribution.fld_KodCaruman && x.fld_KodSubCaruman == activeSubContribution.fld_KodSubCaruman && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Deleted == false))
                                {
                                    tbl_PkjCarumanTambahan pkjCarumanTambahan = new tbl_PkjCarumanTambahan();
                                    pkjCarumanTambahan.fld_Nopkj = NoPkj;
                                    pkjCarumanTambahan.fld_KodCaruman = activeContribution.fld_KodCaruman;
                                    pkjCarumanTambahan.fld_KodSubCaruman = activeSubContribution.fld_KodSubCaruman;
                                    pkjCarumanTambahan.fld_NegaraID = NegaraID;
                                    pkjCarumanTambahan.fld_SyarikatID = SyarikatID;
                                    pkjCarumanTambahan.fld_WilayahID = WilayahID;
                                    pkjCarumanTambahan.fld_LadangID = LadangID;
                                    pkjCarumanTambahan.fld_Deleted = false;
                                    dbr.tbl_PkjCarumanTambahan.Add(pkjCarumanTambahan);
                                    dbr.SaveChanges();
                                }
                            }
                        }
                    }
                }
                else if (app2.fld_Kdrkyt == "MA")
                {
                    DateDiff umurPekerja = new DateDiff(Convert.ToDateTime(app2.fld_Trlhr).AddDays(-1), lastDay);
                    var jnsSocso = tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "kodCarumanSocso" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).Select(s => s.fldOptConfValue).FirstOrDefault();
                    var kodSocso = Masterdb.tbl_JenisCaruman.Where(x => x.fld_UmurLower <= umurPekerja.Years && x.fld_UmurUpper >= umurPekerja.Years && x.fld_JenisCaruman == jnsSocso && x.fld_Default == true).Select(s => s.fld_KodCaruman).FirstOrDefault();
                    var jnsKwsp = tblOptionConfigsWebs.Where(x => x.fldOptConfFlag1 == "kodCarumanKwsp" && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fldDeleted == false).Select(s => s.fldOptConfValue).FirstOrDefault();
                    var kodKwsp = Masterdb.tbl_JenisCaruman.Where(x => x.fld_UmurLower <= umurPekerja.Years && x.fld_UmurUpper >= umurPekerja.Years && x.fld_JenisCaruman == jnsKwsp && x.fld_Default == true).Select(s => s.fld_KodCaruman).FirstOrDefault();

                    if (app2 != null)
                    {
                        app2.fld_KodSocso = kodSocso;
                        app2.fld_KodKWSP = kodKwsp;
                        app2.fld_StatusKwspSocso = "1";
                        dbr.SaveChanges();
                    }

                    //sip
                    var activeContributionCategoryData = Masterdb.tbl_CarumanTambahan.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false && x.fld_Default == true && x.fld_Warganegara == 1).ToList();

                    foreach (var activeContribution in activeContributionCategoryData)
                    {
                        var activeSubContributionCategoryData = Masterdb.tbl_SubCarumanTambahan.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_Deleted == false && x.fld_KodCaruman == activeContribution.fld_KodCaruman);
                        foreach (var activeSubContribution in activeSubContributionCategoryData)
                        {
                            if (activeSubContribution.fld_UmurLower <= umurPekerja.Years && activeSubContribution.fld_UmurUpper >= umurPekerja.Years)
                            {
                                if (!dbr.tbl_PkjCarumanTambahan.Any(x => x.fld_Nopkj == NoPkj && x.fld_KodCaruman == activeContribution.fld_KodCaruman && x.fld_KodSubCaruman == activeSubContribution.fld_KodSubCaruman && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID && x.fld_Deleted == false))
                                {
                                    tbl_PkjCarumanTambahan pkjCarumanTambahan = new tbl_PkjCarumanTambahan();
                                    pkjCarumanTambahan.fld_Nopkj = NoPkj;
                                    pkjCarumanTambahan.fld_KodCaruman = activeContribution.fld_KodCaruman;
                                    pkjCarumanTambahan.fld_KodSubCaruman = activeSubContribution.fld_KodSubCaruman;
                                    pkjCarumanTambahan.fld_NegaraID = NegaraID;
                                    pkjCarumanTambahan.fld_SyarikatID = SyarikatID;
                                    pkjCarumanTambahan.fld_WilayahID = WilayahID;
                                    pkjCarumanTambahan.fld_LadangID = LadangID;
                                    pkjCarumanTambahan.fld_Deleted = false;
                                    dbr.tbl_PkjCarumanTambahan.Add(pkjCarumanTambahan);
                                    dbr.SaveChanges();
                                }
                            }
                        }
                    }
                }

                Result = true;
            }
            catch (Exception ex)
            {
                Result = false;
            }

            return Result;
        }

        public JsonResult SyncToCheckRoll(Guid? ID, string CostCenter)
        {
            string msg = "";
            string statusmsg = "";
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
                if (SyncToCheckRollFunc(EstateLabourData.fld_NegaraID, EstateLabourData.fld_SyarikatID, EstateLabourData.fld_WilayahID, EstateLabourData.fld_LadangID, DivisionID, EstateLabourData, GetBatchNo, EstateLabourData.fld_WorkerNo, CostCenter))
                {
                    msg = "Synced successfully";
                    statusmsg = "success";
                }
                else
                {
                    msg = "Synced failure";
                    statusmsg = "warning";
                }
            }

            return Json(new { msg, statusmsg });
        }

        public bool SyncToCheckRollFunc(int? NegaraID, int? SyarikatID, int? WilayahID, int? LadangID, int? DivisionID, tbl_LbrDataInfo tbl_LbrDataInfo, string GetBatchNo, string NoPkj, string CostCenter)
        {
            string Host1, Catalog1, UserID1, Pass1 = "";
            string Purpose2 = "CHECKROLL";
            bool Result = false;
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            Connection.GetConnection(out Host1, out Catalog1, out UserID1, out Pass1, WilayahID.Value, SyarikatID.Value, NegaraID.Value, Purpose2);
            dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host1, Catalog1, UserID1, Pass1);
            tbl_Pkjmast tbl_Pkjmast = new tbl_Pkjmast();

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
            tbl_Pkjmast.fld_KodSAPPekerja = CostCenter;
            //add by faeza 20.04.2021
            tbl_Pkjmast.fld_PaymentMode = tbl_LbrDataInfo.fld_PaymentMode;
            tbl_Pkjmast.fld_Last4Pan = tbl_LbrDataInfo.fld_Last4Pan;

            try
            {
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

        public JsonResult GetClinicList (string StateCode)
        {
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            List<SelectListItem> ClinicList = new List<SelectListItem>();

            var GetClinicList = Masterdb.tbl_Clinic.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_StateCode == StateCode && x.fld_Deleted == false).OrderBy(o => o.fld_ClinicName).ToList();
            ClinicList = new SelectList(GetClinicList.Select(s => new SelectListItem { Value = s.fld_ID.ToString(), Text = s.fld_ClinicName + " - " + s.fld_District }), "Value", "Text").ToList();
            string DoctorName = GetClinicList.Select(s => s.fld_DoctorName).Take(1).FirstOrDefault();
            return Json(new { ClinicList, DoctorName });
        }

        public JsonResult GetClinicInfo(int ClinicID)
        {
            GetUserID = GetIdentity.ID(User.Identity.Name);
            GetNSWL.GetData(out NegaraID, out SyarikatID, out WilayahID, out LadangID, GetUserID, User.Identity.Name);
            List<SelectListItem> ClinicList = new List<SelectListItem>();

            var GetClinicList = Masterdb.tbl_Clinic.Where(x => x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_ID == ClinicID && x.fld_Deleted == false).FirstOrDefault();
            string DoctorName = GetClinicList.fld_DoctorName;
            return Json(new { DoctorName });
        }
    }
}