using Chloe.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    public class MultKeyTable
    {
        [Column(IsPrimaryKey = true)]
        public int CityId { get; set; }
        [Column(IsPrimaryKey = true)]
        public string UserId { get; set; }
        public string Name { get; set; }
    }
}
