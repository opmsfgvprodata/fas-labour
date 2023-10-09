namespace MVC_SYSTEM.LabourModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using MVC_SYSTEM.Models;

    [DbConfigurationType(typeof(MVC_SYSTEM_Models_Config))]
    public partial class MVC_SYSTEM_Models : DbContext
    {
        public static string host1 = "";
        public static string catalog1 = "";
        public static string user1 = "";
        public static string pass1 = "";


        public MVC_SYSTEM_Models()
            : base(nameOrConnectionString: "BYOWN")
        {
            base.Database.Connection.ConnectionString = "data source=" + host1 + ";initial catalog=" + catalog1 + ";user id=" + user1 + ";password=" + pass1 + ";MultipleActiveResultSets=True;App=EntityFramework";
        }

        public static MVC_SYSTEM_Models ConnectToSqlServer(string host, string catalog, string user, string pass)
        {
            host1 = host;
            catalog1 = catalog;
            user1 = user;
            pass1 = pass;

            return new MVC_SYSTEM_Models();
        }

        public virtual DbSet<tbl_LbrEstQuota> tbl_LbrEstQuota { get; set; }
        public virtual DbSet<tbl_PdfGen> tbl_PdfGen { get; set; }

        public virtual DbSet<tbl_LbrHQQuota> tbl_LbrHQQuota { get; set; }
        public virtual DbSet<tbl_LbrRqst> tbl_LbrRqst { get; set; }
        public virtual DbSet<tbl_LbrDataInfo> tbl_LbrDataInfo { get; set; }
        public virtual DbSet<tbl_LbrTKTProcess> tbl_LbrTKTProcess { get; set; }
        public virtual DbSet<tbl_LbrTKAProcess> tbl_LbrTKAProcess { get; set; }
        public virtual DbSet<tbl_LbrTKTOfrLtr> tbl_LbrTKTOfrLtr { get; set; }
        public virtual DbSet<tbl_LbrTrnsferHistory> tbl_LbrTrnsferHistory { get; set; }
        public virtual DbSet<vw_LbrTrnsferData> vw_LbrTrnsferData { get; set; }
        public virtual DbSet<tbl_LbrFomemaRslt> tbl_LbrFomemaRslt { get; set; }
        public virtual DbSet<vw_LbrFomemaRslt> vw_LbrFememaRslt { get; set; }
        public virtual DbSet<tbl_LbrFlightRequest> tbl_LbrFlightRequest { get; set; }
        public virtual DbSet<tbl_LbrNotificationApproval> tbl_LbrNotificationApproval { get; set; }
        public virtual DbSet<vw_LbrFlightRequest> vw_LbrFlightRequest { get; set; }
        public virtual DbSet<tbl_LbrAbsconded> tbl_LbrAbsconded { get; set; }
        public virtual DbSet<vw_LbrAbsconded> vw_LbrAbsconded { get; set; }
        public virtual DbSet<tbl_LbrEndContract> tbl_LbrEndContract { get; set; }
        public virtual DbSet<vw_LbrEndContract> vw_LbrEndContract { get; set; }
        public virtual DbSet<tbl_LbrOnLeave> tbl_LbrOnLeave { get; set; }
        public virtual DbSet<vw_LbrOnLeave> vw_LbrOnLeave { get; set; }
        public virtual DbSet<tbl_LbrSickDeath> tbl_LbrSickDeath { get; set; }
        public virtual DbSet<vw_LbrSickDeath> vw_LbrSickDeath { get; set; }
        public virtual DbSet<vw_LbrTKTOffrLetter> vw_LbrTKTOffrLetter { get; set; }
        public virtual DbSet<tbl_LbrPrmtPsprtUpdate> tbl_LbrPrmtPsprtUpdate { get; set; }
        public virtual DbSet<vw_LbrPrmtPsprtUpdate> vw_LbrPrmtPsprtUpdate { get; set; }
        public virtual DbSet<tbl_LbrRelationInfo> tbl_LbrRelationInfo { get; set; }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
