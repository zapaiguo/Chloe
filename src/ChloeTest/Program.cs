using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Entity;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using Chloe.Infrastructure.Interception;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string s = Guid.NewGuid().ToString("N");
            string s1 = Guid.NewGuid().ToString();

            Int16 shortId = 2;
            int id = 2;
            long longId = 2;
            Gender gender = Gender.Man;

            User u = new User();
            //u.Age = 1; ;
            int? i = null;
            Expression<Func<int, object>> e = a => (Gender)longId;

            //var b = ExpressionEvaluator.Evaluate(e.Body);

            MappingTypeSystem.Configure(new String_MappingType());
            IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            DbInterception.Add(interceptor);

            //FeatureTest_SQLite.Test();
            //FeatureTest.Test();
            //MySqlDemo.Run();
            //OracleDemo.Run();
            PostgreSQLTest.Test();
        }

        static void Test()
        {
            EntityTypeBuilder<City> builder = new EntityTypeBuilder<City>();

            builder.MapTo("CC");
            builder.Ignore(a => a.s);
            builder.Ignore(a => a.s);

            builder.Property(a => a.Id).IsPrimaryKey(false).MapTo("ID");

            var e = builder.EntityType;
            var p = e.Properties;
            var annotations = e.Annotations;

            var info = e.MakeDefinition();
        }

        static void F<TEntity>(IEntityTypeBuilder<TEntity> builder) where TEntity : User
        {
            builder.Property(a => a.Id);
        }

      
    }
}
