using Chloe.Annotations;
using Chloe.Core;
using Chloe.Entity;
using Chloe.Extensions;
using Chloe.Mapper;
using Chloe.SqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    public enum Gender
    {
        Man = 1,
        Woman
    }

    [TableAttribute("Users")]
    public class UserLite
    {
        [AutoIncrementAttribute]
        public virtual int Id { get; set; }
        public string Name { get; set; }
    }

    public interface IUser
    {
        int Id { get; set; }
        string Name { get; set; }
    }

    [TableAttribute("Users")]
    public class User : UserLite, IUser
    {
        //[Column(IsPrimaryKey = true)]
        //[AutoIncrementAttribute]
        //[NotAutoIncrementAttribute]
        //[Sequence("USERS_AUTOID")]
        //public int Id { get; set; }
        //[AutoIncrementAttribute]
        //[NotAutoIncrementAttribute]
        //[Column(IsPrimaryKey = true)]
        //public string Name { get; set; }
        [NotMapped]
        public string NickName { get; set; }

        public Gender? Gender { get; set; }

        //[Sequence("DecSeq")]
        public int? Age { get; set; }
        //[AutoIncrementAttribute]
        public int? CityId { get; set; }
        public DateTime? OpTime { get; set; }
        //public TimeSpan? TimeSpan { get; set; }

        [NotMapped]
        public Byte[] ByteArray { get; set; }

        public bool HasAge()
        {
            return this.Age != null;
        }
    }

    [Table("C", Schema = "dbo")]
    public class City
    {
        [Sequence("AutoId")]
        public int Id { get; set; }
        [Column("nn")]
        public string Name { get; set; }
        public int ProvinceId { get; set; }
        [NotMapped]
        public string s { get; set; }
    }

    public class Province
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
