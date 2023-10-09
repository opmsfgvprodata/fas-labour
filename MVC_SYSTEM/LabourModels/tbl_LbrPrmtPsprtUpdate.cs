namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrPrmtPsprtUpdate
    {
        [Key]
        public long fld_ID { get; set; }

        public Guid? fld_LbrRefID { get; set; }

        [StringLength(20)]
        [Required]
        public string fld_NewPrmtPsprtNo { get; set; }

        [Column(TypeName = "date")]
        [Required]
        public DateTime? fld_NewPrmtPsrtEndDT { get; set; }

        [StringLength(20)]
        public string fld_OldPrmtPsprtNo { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_OldPrmtPsrtEndDT { get; set; }

        public short? fld_PurposeIndicator { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        public int? fld_WilayahID { get; set; }

        public int? fld_LadangID { get; set; }

        public int? fld_CreatedBy { get; set; }

        public DateTime? fld_CreatedDT { get; set; }

        public bool? fld_Deleted { get; set; }

        public int? fld_DeletedBy { get; set; }

        public DateTime? fld_DeletedDT { get; set; }
    }
}
