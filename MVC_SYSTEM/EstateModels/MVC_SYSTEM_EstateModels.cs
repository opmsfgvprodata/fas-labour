namespace MVC_SYSTEM.EstateModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MVC_SYSTEM_EstateModels : DbContext
    {
        public static string host1 = "";
        public static string catalog1 = "";
        public static string user1 = "";
        public static string pass1 = "";
        public MVC_SYSTEM_EstateModels()
            : base(nameOrConnectionString: "BYOWN")
        {
            base.Database.Connection.ConnectionString = "data source=" + host1 + ";initial catalog=" + catalog1 + ";user id=" + user1 + ";password=" + pass1 + ";MultipleActiveResultSets=True;App=EntityFramework";
        }

        public static MVC_SYSTEM_EstateModels ConnectToSqlServer(string host, string catalog, string user, string pass)
        {
            host1 = host;
            catalog1 = catalog;
            user1 = user;
            pass1 = pass;

            return new MVC_SYSTEM_EstateModels();
        }

        public virtual DbSet<tbl_Pkjmast> tbl_Pkjmast { get; set; }
        public virtual DbSet<tbl_CutiPeruntukan> tbl_CutiPeruntukan { get; set; }
        public virtual DbSet<tbl_PkjCarumanTambahan> tbl_PkjCarumanTambahan { get; set; }
        public virtual DbSet<tbl_PktUtama> tbl_PktUtama { get; set; }
        public virtual DbSet<tbl_GajiBulanan> tbl_GajiBulanan { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
