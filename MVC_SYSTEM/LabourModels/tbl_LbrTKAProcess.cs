namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrTKAProcess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid fld_ID { get; set; }

        //modified by faeza 15.10.2021 - change length 50 to 100
        [StringLength(100)]
        [Required]
        [Display(Name = "Name")]
        public string fld_Nama { get; set; }

        [StringLength(15)]
        [Required]
        [Display(Name = "Passport No")]
        public string fld_NoPassport { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Required]
        [Display(Name = "Date of Birth")]
        public DateTime? fld_BOD { get; set; }

        [Display(Name = "Age")]
        public short? fld_Age { get; set; }

        [StringLength(50)]
        [Required]
        [Display(Name = "Original Batch")]
        public string fld_OriginBatchNo { get; set; }

        [Required]
        [Display(Name = "Queue No")]
        public short? fld_QueueNo { get; set; }

        [StringLength(1)]
        [Display(Name = "Gender")]
        public string fld_SexType { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date of Arrived")]
        public DateTime? fld_ArrivedDT { get; set; }

        public long? fld_LbrRqstID { get; set; }

        [Display(Name = "Process Status")]
        public short? fld_ProcessStatus { get; set; }

        [Display(Name = "Success Status")]
        public bool? fld_SuccessStatus { get; set; }

        [Display(Name = "Unsuccess Reason")]
        public short? fld_UnsuccessReason { get; set; }

        [StringLength(100)]
        [Display(Name = "Notes")]
        public string fld_Notes { get; set; }

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
    }
}
