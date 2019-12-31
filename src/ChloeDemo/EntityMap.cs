using Chloe.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ChloeDemo
{
    public class UserMapBase<TUser> : EntityTypeBuilder<TUser> where TUser : UserLite
    {
        public UserMapBase()
        {
            this.Ignore(a => a.NotMapped);
            this.Property(a => a.Id).IsAutoIncrement().IsPrimaryKey();
        }
    }
    public class UserMap : UserMapBase<User>
    {
        public UserMap()
        {
            this.MapTo("Users");
            this.HasOne(a => a.City).WithForeignKey(a => a.CityId);
            this.Ignore(a => a.NotMapped);
            this.Property(a => a.Gender).HasDbType(DbType.Int32);
        }
    }

    public class CityMap : EntityTypeBuilder<City>
    {
        public CityMap()
        {
            this.HasMany(a => a.Users);
            this.HasOne(a => a.Province).WithForeignKey(a => a.ProvinceId);
        }
    }

    public class ProvinceMap : EntityTypeBuilder<Province>
    {
        public ProvinceMap()
        {
            this.HasMany(a => a.Cities);
        }
    }
}
