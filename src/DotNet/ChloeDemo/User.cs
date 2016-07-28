using Chloe.Entity;
using Chloe.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    public enum Gender
    {
        Man = 1,
        Woman
    }

    [TableAttribute("Users")]
    public class User
    {
        public User()
        {

        }
        public User(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
        public User(string name)
        {
            this.Name = name;
        }
        public User(int id, string name, User u)
        {
            this.Id = id;
            this.Name = name;
            this.U = u;
        }

        [Column(IsPrimaryKey = true)]
        [AutoIncrementAttribute]
        public int Id { get; set; }

        public string Name { get; set; }
        public string NickName { get; set; }

        public Gender? Gender { get; set; }

        public int? Age { get; set; }

        public DateTime? OpTime { get; set; }

        public User U { get; set; }


        public Byte[] ByteArray { get; set; }

    }
}
