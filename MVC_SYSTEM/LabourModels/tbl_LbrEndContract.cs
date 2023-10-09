namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrEndContract
    {
        [Key]
        public int fld_ID { get; set; }

        public Guid? fld_LbrRefID { get; set; }

        [Column(TypeName = "date")]
        [Required]
        public DateTime? fld_ReturnDT { get; set; }

        [StringLength(2)]
        public string fld_ContractStatus { get; set; }

        [StringLength(100)]
        [Required]
        public string fld_Remark { get; set; }

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
