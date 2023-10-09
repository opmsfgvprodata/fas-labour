//using MVC_SYSTEM.AuthModels;
using MVC_SYSTEM.MasterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_SYSTEM.Class
{
    public class GetNSWL
    {
        private MVC_SYSTEM_MasterModels db = new MVC_SYSTEM_MasterModels();
        //private MVC_SYSTEM_Auth db2 = new MVC_SYSTEM_Auth();
        GetIdentity getidentity = new GetIdentity();
        public void GetData(out int ? NegaraID, out int ? SyarikatID, out int?  WilayahID, out int ? LadangID, int ? userid, string username)
        {
            NegaraID = 0;
            SyarikatID = 0;
            WilayahID = 0;
            LadangID = 0;

            if (getidentity.SuperPowerAdmin(username) || getidentity.SuperAdmin(username) || getidentity.Admin1(username) || getidentity.Admin2(username))
            {
                var getcountycompany = db.tbl_EstateSelection.Where(x => x.fld_UserID == userid).FirstOrDefault();
                if (getcountycompany != null)
                {
                    NegaraID = getcountycompany.fld_NegaraID;
                    SyarikatID = getcountycompany.fld_SyarikatID;
                    WilayahID = 0;
                    LadangID = 0;
                }
                else
                {
                    var GetSuperAdminDetail = db.tblUsers.Where(x => x.fldUserID == userid).FirstOrDefault();
                    NegaraID = GetSuperAdminDetail.fldNegaraID;
                    SyarikatID = GetSuperAdminDetail.fldSyarikatID;
                    WilayahID = 0;
                    LadangID = 0;
                }
            }
            else if (getidentity.SuperPowerUser(username))
            {
                var getcountycompany = db.tblUsers.Where(x => x.fldUserID == userid).FirstOrDefault();
                NegaraID = getcountycompany.fldNegaraID;
                SyarikatID = getcountycompany.fldSyarikatID;
                WilayahID = getcountycompany.fldWilayahID;
                LadangID = getcountycompany.fldLadangID;
            }
            else if (getidentity.SuperUser(username) || getidentity.NormalUser(username))
            {
                var getcountycompany = db.tblUsers.Where(x => x.fldUserID == userid).FirstOrDefault();
                NegaraID = getcountycompany.fldNegaraID;
                SyarikatID = getcountycompany.fldSyarikatID;
                WilayahID = getcountycompany.fldWilayahID;
                LadangID = getcountycompany.fldLadangID;
            }
        }

        public int? GetDivisionSelection(int? userid, int? NegaraID, int? SyarikatID, int? WilayahID, int? LadangID)
        {
            int? DivisionID = 0;

            var GetDivision = db.tbl_EstateDivisionSelection.Where(x => x.fld_UserID == userid && x.fld_NegaraID == NegaraID && x.fld_SyarikatID == SyarikatID && x.fld_WilayahID == WilayahID && x.fld_LadangID == LadangID).Select(s => s.fld_DivisionID).FirstOrDefault();

            if (GetDivision > 0)
            {
                DivisionID = GetDivision;
            }
            else
            {
                DivisionID = 0;
            }

            return DivisionID;
        }

        public vw_NSWL GetLadangDetail(int LadangID)
        {
            vw_NSWL NSWL = new vw_NSWL();

            NSWL = db.vw_NSWL.Where(x => x.fld_LadangID == LadangID).FirstOrDefault();

            db.Dispose();

            return NSWL;
        }

        public List<vw_NSWL> GetLadang()
        {
            List<vw_NSWL> NSWL = new List<vw_NSWL>();

            NSWL = db.vw_NSWL.Where(x => x.fld_Deleted_L == false).ToList();

            db.Dispose();

            return NSWL;
        }

        public vw_NSWL GetLadangDetail(string kdprmhnan, string kdldg)
        {
            vw_NSWL NSWL = new vw_NSWL();

            NSWL = db.vw_NSWL.Where(x => x.fld_LdgCode == kdldg && x.fld_RequestCode == kdprmhnan).FirstOrDefault();

            db.Dispose();

            return NSWL;
        }
    }
}