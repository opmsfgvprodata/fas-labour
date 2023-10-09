namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrFlightRequest
    {
        [Key]
        public int fld_ID { get; set; }

        public Guid? fld_LbrRefID { get; set; }
        
        public int? fld_DestinationCode { get; set; }

        [StringLength(3)]
        public string fld_FlightCode { get; set; }

        [Column(TypeName = "date")]
        [Required]
        public DateTime? fld_RequestDT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_RequestDT2 { get; set; }

        [StringLength(2)]
        public string fld_ReasonRequestCode { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        public int? fld_WilayahID { get; set; }

        public int? fld_LadangID { get; set; }

        public int? fld_CreatedBy { get; set; }

        public DateTime? fld_CreatedDT { get; set; }

        [Display(Name = "Flight Request Status")]
        public bool? fld_ApprovedStatus { get; set; }

        public int? fld_ApprovedBy { get; set; }

        public DateTime? fld_ApprovedDT { get; set; }

        public bool? fld_Deleted { get; set; }

        public int? fld_DeletedBy { get; set; }

        public DateTime? fld_DeletedDT { get; set; }

        [StringLength(50)]
        public string fld_NotificationCode { get; set; }
    }
}
