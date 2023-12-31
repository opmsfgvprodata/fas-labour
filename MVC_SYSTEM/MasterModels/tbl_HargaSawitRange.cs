//using System.Web.Mvc;
//using MVC_SYSTEM.App_LocalResources;

//namespace MVC_SYSTEM.MasterModels
//{
//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;
//    using System.ComponentModel.DataAnnotations.Schema;
//    using System.Data.Entity.Spatial;

//    public partial class tbl_HargaSawitRange
//    {
//        [Key]
//        public int fld_ID { get; set; }

//        [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
//        [StringLength(3, ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgMaxCurrencyModelValidation")]
//        [Remote("IsOilPriceRangeCodeExist", "Maintenance", ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelDuplicateCode")]
//        public string fld_KodHarga { get; set; }

//        [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
//        [Range(0.00, 9999999.99, ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgMaxCurrencyModelValidation")]
//        [RegularExpression("^\\d+(?:\\.\\d{1,2})?$", ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgNumberModelValidation")]
//        public decimal? fld_RangeHargaLower { get; set; }

//        [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
//        [Range(0.00, 9999999.99, ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgMaxCurrencyModelValidation")]
//        [RegularExpression("^\\d+(?:\\.\\d{1,2})?$", ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgNumberModelValidation")]
//        public decimal? fld_RangeHargaUpper { get; set; }

//        [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
//        [Range(0.00, 9999999.99, ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgMaxCurrencyModelValidation")]
//        [RegularExpression("^\\d+(?:\\.\\d{1,2})?$", ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgNumberModelValidation")]
//        public decimal? fld_Insentif { get; set; }

//        public int? fld_NegaraID { get; set; }

//        public int? fld_SyarikatID { get; set; }

//        public bool? fld_Deleted { get; set; }
//    }

//    public partial class tbl_HargaSawitRangeViewModel
//    {
//        [Key]
//        public int fld_ID { get; set; }

//        [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
//        [StringLength(3, ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgMaxCurrencyModelValidation")]
//        public string fld_KodHarga { get; set; }

//        [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
//        [Range(0.00, 9999999.99, ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgMaxCurrencyModelValidation")]
//        [RegularExpression("^\\d+(?:\\.\\d{1,2})?$", ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgNumberModelValidation")]
//        public decimal? fld_RangeHargaLower { get; set; }

//        [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
//        [Range(0.00, 9999999.99, ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgMaxCurrencyModelValidation")]
//        [RegularExpression("^\\d+(?:\\.\\d{1,2})?$", ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgNumberModelValidation")]
//        public decimal? fld_RangeHargaUpper { get; set; }

//        [Required(ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgModelValidation")]
//        [Range(0.00, 9999999.99, ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgMaxCurrencyModelValidation")]
//        [RegularExpression("^\\d+(?:\\.\\d{1,2})?$", ErrorMessageResourceType = typeof(GlobalResEstate), ErrorMessageResourceName = "msgNumberModelValidation")]
//        public decimal? fld_Insentif { get; set; }

//        public int? fld_NegaraID { get; set; }

//        public int? fld_SyarikatID { get; set; }

//        public bool? fld_Deleted { get; set; }
//    }
//}
