using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe;
using Chloe.Query;
using Chloe.SqlServer;
using Newtonsoft.Json;

namespace ChloePerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Expression predicateBody = null;
            //Expression<Func<DPShop, bool>> predicate = null;

            int age = 18;
            Expression<Func<int, object>> pre = a => new { Name = "1", Age = age };


        }
    }

    public class A
    {
        public string S { get; set; }
        public string S1 { get; set; }
    }
}
