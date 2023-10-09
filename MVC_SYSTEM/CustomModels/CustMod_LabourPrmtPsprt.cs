using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MVC_SYSTEM.CustomModels
{
    public class CustMod_LabourPrmtPsprt
    {
        public Guid fld_ID { get; set; }

        [StringLength(20)]
        [Display(Name = "Worker ID")]
        public string fld_WorkerIDNo { get; set; }

        [StringLength(20)]
        [Display(Name = "Permit No")]
        public string fld_PermitNo { get; set; }

        [StringLength(20)]
        [Display(Name = "Worker No")]
        public string fld_WorkerNo { get; set; }

        [StringLength(50)]
        [Display(Name = "Worker Name")]
        public string fld_WorkerName { get; set; }

        [StringLength(100)]
        [Display(Name = "Worker Address")]
        public string fld_WorkerAddress { get; set; }

        [StringLength(10)]
        [Display(Name = "Postcode")]
        public string fld_Postcode { get; set; }

        [StringLength(10)]
        [Display(Name = "State")]
        public string fld_State { get; set; }

        [StringLength(10)]
        [Display(Name = "Country")]
        public string fld_Country { get; set; }

        [StringLength(15)]
        [Display(Name = "Phone No")]
        public string fld_PhoneNo { get; set; }

        [StringLength(1)]
        [Display(Name = "Gender")]
        public string fld_SexType { get; set; }

        [StringLength(2)]
        [Display(Name = "Race")]
        public string fld_Race { get; set; }

        [StringLength(1)]
        [Display(Name = "Religion")]
        public string fld_Religion { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Birth of Date")]
        public DateTime? fld_BOD { get; set; }

        [Display(Name = "Age")]
        public short? fld_Age { get; set; }

        [StringLength(2)]
        [Display(Name = "Nationality")]
        public string fld_Nationality { get; set; }

        [StringLength(1)]
        [Display(Name = "Marital Status")]
        public string fld_MarriedStatus { get; set; }

        [StringLength(1)]
        public string fld_FeldaRelated { get; set; }

        [StringLength(1)]
        [Display(Name = "Active Status")]
        public string fld_ActiveStatus { get; set; }

        [StringLength(2)]
        [Display(Name = "Inactive Reason")]
        public string fld_InactiveReason { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Time Inactive")]
        public DateTime? fld_InactiveDT { get; set; }

        public bool? fld_OnLeaveStatus { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? fld_OnLeaveLastDT { get; set; }

        [StringLength(100)]
        [Display(Name = "Notes")]
        public string fld_Notes { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date of Working Started")]
        public DateTime? fld_WorkingStartDT { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date of Confirmation")]
        public DateTime? fld_ConfirmationDT { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date of Passport End")]
        public DateTime? fld_PassportEndDT { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date of Permit End")]
        public DateTime? fld_PermitEndDT { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Arrived")]
        public DateTime? fld_ArrivedDT { get; set; }

        [StringLength(2)]
        [Display(Name = "Worker Type")]
        public string fld_WorkerType { get; set; }

        [StringLength(2)]
        [Display(Name = "Worker Category")]
        public string fld_WorkCtgry { get; set; }

        [StringLength(50)]
        [Display(Name = "Birth Place")]
        public string fld_TmptLhr { get; set; }

        [Display(Name = "ROC")]
        public short? fld_Roc { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        [Display(Name = "Region")]
        public int? fld_WilayahID { get; set; }

        [Display(Name = "Estate")]
        public int? fld_LadangID { get; set; }

      //  public int? fld_DivisionID { get; set; }

      //  public Guid? fld_LbrProcessID { get; set; }

        public bool? fld_TransferToChckrollStatus { get; set; }

        public bool? fld_TransferToChckrollWorkerTransferStatus { get; set; }

        [StringLength(50)]
        public string fld_WorkerTransferCode { get; set; }

        [StringLength(3)]
        [Display(Name = "Supplier")]
        public string fld_SupplierCode { get; set; }

        [StringLength(5)]
        [Display(Name = "Bank")]
        public string fld_BankCode { get; set; }

        [StringLength(20)]
        [Display(Name = "Account Bank No")]
        public string fld_BankAcc { get; set; }

        [StringLength(20)]
        [Display(Name = "Perkeso No")]
        public string fld_PerkesoNo { get; set; }

     //   public int? fld_CreatedBy { get; set; }

        public DateTime? fld_CreatedDT { get; set; }

        public int? fld_ModifiedBy { get; set; }

        public DateTime? fld_ModifiedDT { get; set; }

        public string fld_Absconded { get; set; }
        public string fld_EndContract { get; set; }

        public string fld_Onleave { get; set; }
        public string fld_SickDeath { get; set; }

        public string costcenter { get; set; }


        public string fld_fomema { get; set; }

    }
}