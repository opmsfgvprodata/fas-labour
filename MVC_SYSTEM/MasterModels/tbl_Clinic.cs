namespace MVC_SYSTEM.MasterModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Clinic
    {
        [Key]
        public int fld_ID { get; set; }

        [StringLength(50)]
        public string fld_ClinicName { get; set; }

        [StringLength(50)]
        public string fld_District { get; set; }

        [StringLength(150)]
        public string fld_DoctorName { get; set; }

        public bool? fld_Deleted { get; set; }

        [StringLength(2)]
        public string fld_StateCode { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }
    }
}
