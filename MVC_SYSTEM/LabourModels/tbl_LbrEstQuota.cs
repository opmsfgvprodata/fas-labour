namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrEstQuota
    {
        [Key]
        public int fld_ID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        [Display(Name = "Estate Quota")]
        public short fld_LbrEstQuota { get; set; }

        [Display(Name = "Year")]
        public short fld_Year { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        [Display(Name = "Region")]
        public int? fld_WilayahID { get; set; }

        [Display(Name = "Estate")]
        public int? fld_LadangID { get; set; }

        [Display(Name = "Created By")]
        public int? fld_CreatedBy { get; set; }

        [Display(Name = "Date Time Create")]
        public DateTime? fld_CreatedDT { get; set; }

        [Display(Name = "Modified By")]
        public int? fld_ModifiedBy { get; set; }

        [Display(Name = "Date Time Modified")]
        public DateTime? fld_ModifiedDT { get; set; }

        public bool? fld_Deleted { get; set; }
    }
}
