namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrTKTOfrLtr
    {
        [Key]
        public int fld_ID { get; set; }

        public Guid? fld_LbrRefID { get; set; }

        [StringLength(200)]
        [Required]
        public string fld_LtrAdd { get; set; }

        [Column(TypeName = "date")]
        [Required]
        public DateTime? fld_WorkingSrtDT { get; set; }

        [Required]
        public decimal? fld_DailyPayRate { get; set; }

        public int? fld_CreatedBy { get; set; }

        public DateTime? fld_CreatedDT { get; set; }
    }
}
