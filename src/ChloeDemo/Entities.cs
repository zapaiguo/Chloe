using Chloe.Annotations;
using Chloe.Entity;
using Chloe.Oracle;
using Chloe.SqlServer;
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

        [Chloe.Annotations.ForeignKey("CityId")]
        public City City { get; set; }

        /* Marks the column is timestamp type(sqlserver only) */
        //[Chloe.SqlServer.Annotations.TimestampAttribute]
        //public Byte[] RowVersion { get; set; }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ProvinceId { get; set; }

        [Chloe.Annotations.ForeignKey("ProvinceId")]
        public Province Province { get; set; }
        public List<User> Users { get; set; }
    }

    public class Province
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<City> Cities { get; set; }
    }
}
