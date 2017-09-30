using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    public class EFContext : DbContext
    {
        public EFContext()
            : base("name=connString")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            System.Data.Entity.Database.SetInitializer<DbContext>(null);
            //this.Configuration.LazyLoadingEnabled = false;
            //this.Configuration.ProxyCreationEnabled = false;
            //this.Configuration.AutoDetectChangesEnabled = false;
            //this.Configuration.ValidateOnSaveEnabled = false;
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention
 >();
        }

        public DbSet<TestEntity> TestEntity { get; set; }

    }
}
