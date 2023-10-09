using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_SYSTEM.CustomModels
{
    public class CustMod_MasterInfo
    {

        public long id { get; set; }
        //  public string costcenter { get; set; }
        public string nopkj { get; set; }
        public string namepkj { get; set; }

        public string gender { get; set; }

        public string nationality { get; set; }
        public string status { get; set; }

        public string kwsp { get; set; }

        public string socso { get; set; }
        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? tarikhMasuk { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? passportTamat { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PermitTamat { get; set; }
        public short? roc { get; set; }

        public string agensi { get; set; }
        public string permitNo { get; set; }

        public int? fld_NegaraID { get; set; }
        public int? fld_SyarikatID { get; set; }
        public int? fld_WilayahID { get; set; }
        public int? fld_LadangID { get; set; }

        public string costcenter { get; set; }
    }
}