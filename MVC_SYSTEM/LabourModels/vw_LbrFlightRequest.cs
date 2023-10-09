namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vw_LbrFlightRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int fld_ID { get; set; }

        public Guid? fld_LbrRefID { get; set; }

        [StringLength(20)]
        [Display(Name = "Worker ID")]
        public string fld_WorkerIDNo { get; set; }

        [StringLength(20)]
        public string fld_PermitNo { get; set; }

        [StringLength(20)]
        [Display(Name = "Worker No")]
        public string fld_WorkerNo { get; set; }

        [StringLength(100)]
        [Display(Name = "Worker Name")]
        public string fld_WorkerName { get; set; }

        [StringLength(100)]
        public string fld_WorkerAddress { get; set; }

        [StringLength(10)]
        public string fld_Postcode { get; set; }

        [StringLength(10)]
        public string fld_State { get; set; }

        [StringLength(10)]
        public string fld_Country { get; set; }

        [StringLength(15)]
        public string fld_PhoneNo { get; set; }

        [StringLength(1)]
        public string fld_SexType { get; set; }

        [StringLength(2)]
        public string fld_Race { get; set; }

        [StringLength(1)]
        public string fld_Religion { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_BOD { get; set; }

        public short? fld_Age { get; set; }

        [StringLength(2)]
        public string fld_Nationality { get; set; }

        [StringLength(1)]
        public string fld_MarriedStatus { get; set; }

        [StringLength(1)]
        public string fld_FeldaRelated { get; set; }

        [StringLength(1)]
        public string fld_ActiveStatus { get; set; }

        [StringLength(2)]
        public string fld_InactiveReason { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_InactiveDT { get; set; }

        public int? fld_DestinationCode { get; set; }

        [StringLength(3)]
        [Display(Name = "Flight Type")]
        public string fld_FlightCode { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "Departure Date")]
        public DateTime? fld_RequestDT { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "Come Back Date")]
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

        [StringLength(50)]
        public string fld_NotificationCode { get; set; }

        public DateTime? fld_DeletedDT { get; set; }
    }
}
