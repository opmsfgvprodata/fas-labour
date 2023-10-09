namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrNotificationApproval
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid fld_ID { get; set; }

        [StringLength(50)]
        public string fld_ApprovalSection { get; set; }

        [StringLength(200)]
        public string fld_ApprovalLink { get; set; }

        public int? fld_ApprovalRoleID { get; set; }

        public bool? fld_OpenAction { get; set; }

        public bool? fld_ApproveAction { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        public int? fld_WilayahID { get; set; }

        public int? fld_LadangID { get; set; }

        public int? fld_CreatedBy { get; set; }

        public DateTime? fld_CreatedDT { get; set; }

        [StringLength(50)]
        public string fld_NotificationCode { get; set; }
    }
}
