using System.Web.Mvc;
using MVC_SYSTEM.App_LocalResources;

namespace MVC_SYSTEM.MasterModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_CarumanTambahan
    {
        [Key]
        public int fld_JenisCarumanID { get; set; }

        [Required]
        [StringLength(5)]
        public string fld_KodCaruman { get; set; }

        [Required]
        [StringLength(30)]
        public string fld_NamaCaruman { get; set; }

        [Required]
        public bool fld_Berjadual { get; set; }

        [Required]
        public int fld_CarumanOleh { get; set; }

        public int? fld_Warganegara { get; set; }

        public bool fld_Default { get; set; }

        public int fld_NegaraID { get; set; }

        public int fld_SyarikatID { get; set; }

        public bool fld_Deleted { get; set; }
    }

    //public partial class tbl_CarumanTambahanViewModelCreate
    //{
    //    [Key]
    //    public int fld_JenisCarumanID { get; set; }

    //    [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
    //    [Remote("IsContributionCodeExist", "Maintenance", ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelDuplicateCode")]
    //    [StringLength(5)]
    //    public string fld_KodCaruman { get; set; }

    //    [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
    //    [StringLength(30)]
    //    public string fld_NamaCaruman { get; set; }

    //    [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
    //    public bool fld_Berjadual { get; set; }

    //    [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
    //    public int fld_CarumanOleh { get; set; }

    //    public bool fld_Default { get; set; }

    //    public int fld_NegaraID { get; set; }

    //    public int fld_SyarikatID { get; set; }

    //    public bool fld_Deleted { get; set; }
    //}
}
