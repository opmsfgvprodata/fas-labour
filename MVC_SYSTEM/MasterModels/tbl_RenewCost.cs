namespace MVC_SYSTEM.MasterModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_RenewCost
    {
        [Key]
        public int fld_ID { get; set; }

        [StringLength(2)]
        public string fld_NationalityCode { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? fld_CostPrice { get; set; }

        public short? fld_PurposeIndicator { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        public bool? fld_Deleted { get; set; }
    }
}
