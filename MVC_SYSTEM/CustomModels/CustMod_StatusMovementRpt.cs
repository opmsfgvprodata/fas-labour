using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_SYSTEM.CustomModels
{
    public class CustMod_StatusMovementRpt
    {
        [Key]
        public int fld_ID { get; set; }

        public string fld_EstateName { get; set; }

        public decimal fld_EstateSize { get; set; }

        public int fld_Need { get; set; }

        public string fld_RatioPerHa { get; set; }

        public int fld_TotalLastMonth { get; set; }

        public int fld_TotalAbscondedBI { get; set; }

        public int fld_TotalAbscondedHK { get; set; }

        public int fld_TotalCOMBI { get; set; }

        public int fld_TotalCOMHK { get; set; }

        public int fld_WorkerBalance { get; set; }

        public int fld_NewWorkerBI { get; set; }

        public int fld_NewWorkerHK { get; set; }

        public int fld_TransferIn { get; set; }

        public int fld_TransferOut { get; set; }

        public int fld_WorkerBalanceBI { get; set; }

        public int fld_TotalTKI { get; set; }

        public int fld_TotalTKB { get; set; }

        public int fld_TotalTKD { get; set; }

        public int fld_TotalTKN { get; set; }

        public int fld_TotalPOL { get; set; }

        public int fld_TotalWorker { get; set; }

        public int fld_Approximately { get; set; }

        public int fld_ApproximatelyRatio { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        public int? fld_WilayahID { get; set; }

        public int? fld_LadangID { get; set; }

        public string fld_KodSAPPekerja { get; set; }
    }
}