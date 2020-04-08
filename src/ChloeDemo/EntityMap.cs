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

            /* global filter */
            this.HasQueryFilter(a => a.Id > -1);
        }
    }

    public class EntityMapBase<TEntity> : EntityTypeBuilder<TEntity> where TEntity : EntityBase
    {
        public EntityMapBase()
        {
            this.Property(a => a.Id).IsAutoIncrement().IsPrimaryKey();
        }
    }
    public class PersonMap : EntityMapBase<Person>
    {
        public PersonMap()
        {
            this.MapTo("Person");
            this.Property(a => a.Gender).HasDbType(DbType.Int32);

            this.HasOne(a => a.City).WithForeignKey(a => a.CityId);
            this.Ignore(a => a.NotMapped);

            /* global filter */
            this.HasQueryFilter(a => a.Id > -1);
        }
    }

    public class CityMap : EntityMapBase<City>
    {
        public CityMap()
        {
            this.HasMany(a => a.Persons);
            this.HasOne(a => a.Province).WithForeignKey(a => a.ProvinceId);

            this.HasQueryFilter(a => a.Id > -2);
        }
    }

    public class ProvinceMap : EntityMapBase<Province>
    {
        public ProvinceMap()
        {
            this.HasMany(a => a.Cities);

            this.HasQueryFilter(a => a.Id > -3);
        }
    }

    public class TestEntityMap : EntityTypeBuilder<TestEntity>
    {
        public TestEntityMap()
        {
            this.Property(a => a.Id).IsAutoIncrement().IsPrimaryKey();
            this.HasQueryFilter(a => a.Id > 0);
        }
    }

    public class OracleTestEntityMap : TestEntityMap
    {
        public OracleTestEntityMap()
        {
            //oralce 暂时不支持 guid
            this.Ignore(a => a.F_Guid);
        }
    }
}
