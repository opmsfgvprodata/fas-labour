namespace MVC_SYSTEM.MasterModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_FlightDestination
    {
        [Key]
        public int fld_ID { get; set; }

        [StringLength(50)]
        [Display(Name = "Destination Name")]
        public string fld_DestinationName { get; set; }

        public int? fld_NegaraID { get; set; }

        public int? fld_SyarikatID { get; set; }

        public bool? fld_Deleted { get; set; }
    }
}
