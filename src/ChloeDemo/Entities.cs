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

        [NotMapped]
        public string NotMapped { get; set; }
    }

    [TableAttribute("Users")]
    public class User : UserLite
    {
        [Column(DbType = DbType.Int32)]
        public Gender? Gender { get; set; }
        public int? Age { get; set; }
        public int? CityId { get; set; }
        public DateTime? OpTime { get; set; } = DateTime.Now;

        [Chloe.Annotations.NavigationAttribute("CityId")]
        public City City { get; set; }

        //[Column(IsRowVersion = true)]
        //public int RowVersion { get; set; }

        //[Column(IsRowVersion = true)]
        //public Byte[] RowVersion { get; set; }
    }

    public class EntityBase : IEntity
    {
        [Column(IsPrimaryKey = true)]
        [AutoIncrement]
        public virtual int Id { get; set; }
    }

    //如果使用 fluentmapping，就可以不用打特性了
    [TableAttribute("Person")]
    public class Person : EntityBase
    {
        [Chloe.Annotations.NavigationAttribute("CityId")]
        public City City { get; set; }

        [Column(DbType = DbType.String)]
        public string Name { get; set; }
        [Column(DbType = DbType.Int32)]
        public Gender? Gender { get; set; }
        public int? Age { get; set; }
        public int? CityId { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime? EditTime { get; set; }

        [NotMapped]
        public string NotMapped { get; set; }

        //[Column(IsRowVersion = true)]
        //public int RowVersion { get; set; }
    }

    public class City : EntityBase
    {
        public string Name { get; set; }
        public int ProvinceId { get; set; }

        [Chloe.Annotations.NavigationAttribute("ProvinceId")]
        public Province Province { get; set; }
        //[Chloe.Annotations.NavigationAttribute]
        public List<User> Users { get; set; } = new List<User>();
        [Chloe.Annotations.NavigationAttribute]
        public List<Person> Persons { get; set; } = new List<Person>();
    }

    public class Province : EntityBase
    {
        public string Name { get; set; }

        [Chloe.Annotations.NavigationAttribute]
        public List<City> Cities { get; set; } = new List<City>();
    }
}
