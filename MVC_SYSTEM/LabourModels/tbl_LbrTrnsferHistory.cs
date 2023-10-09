namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrTrnsferHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid fld_ID { get; set; }

        public Guid? fld_LbrRefID { get; set; }

        [StringLength(50)]
        public string fld_WorkerTransferCode { get; set; }

        [StringLength(20)]
        [Display(Name = "Original Worker No")]
        public string fld_OldWorkerNo { get; set; }

        public int? fld_NegaraIDFrom { get; set; }

        public int? fld_SyarikatIDFrom { get; set; }
        
        [Display(Name = "Original Region")]
        public int? fld_WilayahIDFrom { get; set; }

        [Display(Name = "Original Estate")]
        public int? fld_LadangIDFrom { get; set; }

        [StringLength(20)]
        [Display(Name = "Original Worker No")]
        public string fld_NewWorkerNo { get; set; }

        public int? fld_NegaraIDTo { get; set; }

        public int? fld_SyarikatIDTo { get; set; }

        [Display(Name = "Transfered Region")]
        public int? fld_WilayahIDTo { get; set; }

        [Display(Name = "Transfered Estate")]
        public int? fld_LadangIDTo { get; set; }

        public bool? fld_SuccessTransferd { get; set; }

        public int? fld_CreatedBy { get; set; }

        public DateTime? fld_CreatedDT { get; set; }
    }
}
