namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class vw_LbrPrmtPsprtUpdate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long fld_ID { get; set; }

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

        [StringLength(100)]
        public string fld_Notes { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_WorkingStartDT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_ConfirmationDT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_PassportStartDT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_PassportEndDT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_PermitStartDT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fld_PermitEndDT { get; set; }

        [StringLength(2)]
        public string fld_WorkerType { get; set; }

        [StringLength(2)]
        public string fld_WorkCtgry { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        public int? fld_WilayahID { get; set; }

        public int? fld_LadangID { get; set; }

        public int? fld_DivisionID { get; set; }

        public Guid? fld_LbrProcessID { get; set; }

        public bool? fld_TransferToChckrollStatus { get; set; }

        public bool? fld_TransferToChckrollWorkerTransferStatus { get; set; }

        [StringLength(50)]
        public string fld_WorkerTransferCode { get; set; }

        [StringLength(3)]
        public string fld_SupplierCode { get; set; }

        [StringLength(20)]
        [Display(Name = "New Passport/Permit No")]
        public string fld_NewPrmtPsprtNo { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "Expired Date")]
        public DateTime? fld_NewPrmtPsrtEndDT { get; set; }

        [StringLength(20)]
        [Display(Name = "Old Passport/Permit No")]
        public string fld_OldPrmtPsprtNo { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "Expired Date")]
        public DateTime? fld_OldPrmtPsrtEndDT { get; set; }

        public short? fld_PurposeIndicator { get; set; }

        public int? fld_CreatedBy { get; set; }

        public DateTime? fld_CreatedDT { get; set; }

        public bool? fld_Deleted { get; set; }

        [StringLength(50)]
        public string fld_TmptLhr { get; set; }
    }
}
