namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vw_LbrTKTOffrLetter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int fld_ID { get; set; }

        public Guid? fld_LbrRefID { get; set; }

        [StringLength(50)]
        public string fld_Nama { get; set; }

        [StringLength(15)]
        public string fld_NoIC { get; set; }

        [StringLength(15)]
        public string fld_PhoneNo { get; set; }

        [StringLength(1)]
        public string fld_SexType { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_BOD { get; set; }

        public short? fld_Age { get; set; }

        public long? fld_LbrRqstID { get; set; }

        public short? fld_ProcessStatus { get; set; }

        public bool? fld_SuccessStatus { get; set; }

        public short? fld_UnsuccessReason { get; set; }

        [StringLength(100)]
        public string fld_Notes { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        public int? fld_WilayahID { get; set; }

        public int? fld_LadangID { get; set; }

        public int? fld_CreatedBy { get; set; }

        public DateTime? fld_CreatedDT { get; set; }

        public int? fld_ModifiedBy { get; set; }

        public DateTime? fld_ModifiedDT { get; set; }

        [StringLength(200)]
        public string fld_LtrAdd { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_WorkingSrtDT { get; set; }

        public decimal? fld_DailyPayRate { get; set; }
    }
}
