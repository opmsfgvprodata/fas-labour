using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_SYSTEM.CustomModels
{
    public class CustMod_RumKeperluanPekerjaRpt
    {
        [Key]
        public int fld_ID { get; set; }

        public string fld_EstateName { get; set; }

        public decimal fld_EstateSize { get; set; }

        public int fld_Need { get; set; }

        public string fld_RatioPerHa { get; set; }

        public int fld_TotalLastMonth { get; set; }

        public int fld_Keperluan { get; set; }

        public int fld_Indon { get; set; }

        public int fld_Bangla { get; set; }

        public int fld_India { get; set; }

        public int fld_Nepal { get; set; }

        public int fld_TotalTKA { get; set; }

        public int fld_POL { get; set; }

        public int fld_TotalTK { get; set; }

        public Decimal fld_Kedudukan { get; set; }

        public int fld_KekuranganSemasa { get; set; }

        public int fld_PeratusKekuranganSemasa { get; set; }

        public int fld_Agihan { get; set; }

        public int fld_WorkerBalanceBI { get; set; }

        public int fld_TotalTKI { get; set; }

        public int fld_TotalTKB { get; set; }

        public int fld_TotalTKD { get; set; }

        public int fld_TotalTKN { get; set; }

        public int fld_TotalPOL { get; set; }

        public int fld_TotalWorker { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        public int? fld_WilayahID { get; set; }

        public int? fld_LadangID { get; set; }

        public string fld_KodSAPPekerja { get; set; }
    }
}