using Chloe.Annotations;
using Chloe.Entity;
using Chloe.Oracle;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ChloeDemo
{
    public enum Gender
    {
        Man = 1,
        Woman
    }

    public interface IEntity
    {
        int Id { get; set; }
    }
    [TableAttribute("Users")]
    public class UserLite : IEntity
    {
        [Column(IsPrimaryKey = true)]
        [AutoIncrement]
        public virtual int Id { get; set; }
        [Column(DbType = DbType.String)]
        public string Name { get; set; }

        public string NotMapped { get; set; }
    }


    //如果使用 fluentmapping，就可以不用打特性了
    [TableAttribute("Users")]
    public class User : UserLite
    {
        [Column(DbType = DbType.Int32)]
        public Gender? Gender { get; set; }
        public int? Age { get; set; }
        public int? CityId { get; set; }
        public DateTime? OpTime { get; set; }
    }

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
            this.Ignore(a => a.NotMapped);
            this.Property(a => a.Gender).HasDbType(DbType.Int32);
        }
    }

    public class OracleUserMap : UserMap
    {
        public OracleUserMap()
        {
            this.Property(a => a.Id).HasSequence("USERS_AUTOID");
        }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProvinceId { get; set; }
    }

    public class Province
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
