using MVC_SYSTEM.CustomModels;
using MVC_SYSTEM.EstateModels;
using MVC_SYSTEM.MasterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_SYSTEM.Class
{
    public class GetLadang
    {
        private MVC_SYSTEM_MasterModels db = new MVC_SYSTEM_MasterModels();
        private Connection Connection = new Connection();

        public string GetLadangName(int ladangid, int wlyhID)
        {
            string LadangName = db.tbl_Ladang.Where(x => x.fld_ID == ladangid && x.fld_WlyhID == wlyhID).Select(s => s.fld_LdgName).FirstOrDefault();
            return LadangName;
        }

        public string GetLadangCode(int ladangid)
        {
            string LadangCode = db.tbl_Ladang.Where(x => x.fld_ID == ladangid).Select(s => s.fld_LdgCode).FirstOrDefault();
            return LadangCode;
        }

        public void GetLadangAcc(out string NoAcc, out string NoGL, out string NoCIT, int? ldgid, int? wlyhid)
        {
            var account = db.tbl_Ladang.Where(x => x.fld_WlyhID == wlyhid && x.fld_ID == ldgid).FirstOrDefault();
            NoAcc = account.fld_NoAcc;
            NoGL = account.fld_NoGL;
            NoCIT = account.fld_NoCIT;
        }

        public string GetLadangNegeriCode(int? ladangid)
        {
            string LadangNegeriCode = db.tbl_Ladang.Where(x => x.fld_ID == ladangid).Select(s => s.fld_KodNegeri).FirstOrDefault();
            return LadangNegeriCode;
        }

        public  List<CustMod_AreaForEstate> GetEstateAreaByLevel(int? SyarikatID, int? NegaraID)
        {
            string Purpose = "CHECKROLL";
            MVC_SYSTEM_EstateModels dbEstate = new MVC_SYSTEM_EstateModels();
            var GetListEstate = db.vw_NSWL.Where(x => x.fld_Deleted_L == false).ToList();
            var GetRegionList = GetListEstate.Select(s => s.fld_WilayahID).Distinct().ToList();
            List<CustMod_AreaForEstate> CustMod_AreaForEstate = new List<CustMod_AreaForEstate>();
            int ID = 1;
            foreach (var WilayahID in GetRegionList)
            {
                string Host, Catalog, UserID, Pass = "";
                Connection.GetConnection(out Host, out Catalog, out UserID, out Pass, WilayahID, SyarikatID, NegaraID, Purpose);
                dbEstate = MVC_SYSTEM_EstateModels.ConnectToSqlServer(Host, Catalog, UserID, Pass);

                var GetListEstateID = GetListEstate.Where(x => x.fld_WilayahID == WilayahID).Select(s => s.fld_LadangID).ToList();
                var GetLevelData = dbEstate.tbl_PktUtama.Where(x => GetListEstateID.Contains(x.fld_LadangID.Value) && x.fld_Deleted == false).ToList();
                var GetListEstate2 = GetListEstate.Where(x => x.fld_WilayahID == WilayahID).ToList();
                foreach (var Estate in GetListEstate2)
                {
                    var GetAreaSize = GetLevelData.Where(x => x.fld_LadangID == Estate.fld_LadangID).Sum(s => s.fld_LsPktUtama);
                    CustMod_AreaForEstate.Add(new CustMod_AreaForEstate() { fld_ID = ID, fld_SizeArea = GetAreaSize.Value, fld_NegaraID = Estate.fld_NegaraID, fld_SyarikatID = Estate.fld_SyarikatID, fld_WilayahID = Estate.fld_WilayahID, fld_LadangID = Estate.fld_LadangID });
                    ID++;
                }
            }
            return CustMod_AreaForEstate;
        }
    }
}