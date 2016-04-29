using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloePerformanceTest
{
    public class EFContext : DbContext
    {
        public EFContext()
            : base("name=connString")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention
>();
        }

        public DbSet<Test> Test { get; set; }

    }
}
