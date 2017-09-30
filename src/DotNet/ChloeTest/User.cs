using Chloe.Core;
using Chloe.Entity;
using Chloe.Extensions;
using Chloe.Mapper;
using Chloe.Oracle;
using Chloe.SqlServer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    public enum Gender : byte
    {
        Man = 1,
        Woman
    }

    public interface IUser
    {
        int Id { get; set; }

        string Name { get; set; }
    }
    public class EntityBase<T>
    {
        public T Id { get; set; }
    }
    [TableAttribute("Users")]
    public class UserLite : EntityBase<int>
    {
        //[Column(IsPrimaryKey = true)]
        //[Sequence("USERS_AUTOID")]//For oracle
        //public virtual int Id { get; set; }
        //[Column(DbType = DbType.AnsiString)]
        public string Name { get; set; }
    }

    [TableAttribute("Users")]
    public class User : UserLite, IUser
    {

        [NotMapped]
        [MinLengthAttribute(2, ErrorMessage = "{0} 太短")]
        //[RequiredAttribute(ErrorMessage = "{0} 不能为空")]
        [Display(Name = "昵称")]
        public string NickName { get; set; }

        [RequiredAttribute(ErrorMessage = "{0} 不能为空")]
        [Display(Name = "性别")]
        public Gender? Gender { get; set; }

        public int? Age { get; set; }
        //[AutoIncrementAttribute]
        public int? CityId { get; set; }
        public DateTime? OpTime { get; set; }

        //public TimeSpan? F_Time { get; set; }

        [NotMapped]
        public Byte[] ByteArray { get; set; }
    }



    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProvinceId { get; set; }
    }

    public class Province
    {
        //[Column(IsPrimaryKey = false)]
        [NonAutoIncrement]
        public int? Id { get; set; }
        public string Name { get; set; }
    }


    public class TUser
    {
        [Chloe.Entity.ColumnAttribute("Ida")]
        public int Id { get; set; }
        public string Name { get; set; }
    }


    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AdminRole : Role
    {
    }
    public class UserRole : Role
    {
    }
}
