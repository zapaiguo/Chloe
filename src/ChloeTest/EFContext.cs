//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ChloeTest
//{
//    public class EFContext : DbContext
//    {
//        public EFContext() : base(GetOptions())
//        {
//        }
//        public DbSet<Users> Users { get; set; }

//        protected override void OnModelCreating(ModelBuilder builder)
//        {
//        }

//        static DbContextOptions<EFContext> GetOptions()
//        {
//            DbContextOptionsBuilder<EFContext> optionsBuilder = new DbContextOptionsBuilder<EFContext>();
//            optionsBuilder.UseSqlServer("Data Source = .;Initial Catalog = Chloe;Integrated Security = SSPI;");

//            return optionsBuilder.Options;
//        }
//    }

//    public class Users
//    {
//        public virtual int Id { get; set; }
//        public string Name { get; set; }
//    }
//}
