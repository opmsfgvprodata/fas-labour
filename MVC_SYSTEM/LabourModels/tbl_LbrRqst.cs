namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrRqst
    {
        [Key]
        public long fld_ID { get; set; }

        [StringLength(20)]
        [Display(Name = "Batch No")]
        public string fld_BatchNo { get; set; }

        [Display(Name = "No of Labour Request")]
        public short? fld_LbrReqQty { get; set; }

        [Display(Name = "Approved No of Labour Request")]
        public short? fld_AppReqQty { get; set; }

        [Required]
        [Display(Name = "No of TKA Request")]
        public short? fld_TKAQty { get; set; }

        [Required]
        [Display(Name = "No of TKT Request")]
        public short? fld_TKTQty { get; set; }

        public short? fld_Month { get; set; }

        [Display(Name = "Year")]
        public short? fld_Year { get; set; }

        [Display(Name = "Approval Status")]
        public short? fld_ApprovedStatus { get; set; }

        [Display(Name = "Approval By")]
        public int? fld_ApprovedBy { get; set; }

        [Display(Name = "Date Time Approval")]
        public DateTime? fld_ApprovedDT { get; set; }

        [Display(Name = "Process Status")]
        public short? fld_ProcessStatus { get; set; }

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
