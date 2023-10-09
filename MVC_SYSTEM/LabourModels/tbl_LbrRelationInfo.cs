namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LbrRelationInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid fld_ID { get; set; }

        public Guid? fld_LbrRefID { get; set; }

        [StringLength(150)]
        [Display(Name = "Name")]
        public string fld_Name { get; set; }

        [StringLength(1)]
        [Display(Name = "Relationship")]
        public string fld_Relationship { get; set; }

        [StringLength(200)]
        [Display(Name = "Address")]
        public string fld_Address { get; set; }

        [StringLength(30)]
        [Display(Name = "Phone No")]
        public string fld_PhoneNo { get; set; }
    }
}
