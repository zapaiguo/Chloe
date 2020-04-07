using Chloe;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.MySql;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    class MySqlDemo : DemoBase
    {
        MySqlContext _dbContext;
        public MySqlDemo()
        {
            this._dbContext = new MySqlContext(new MySqlConnectionFactory("Database='Chloe';Data Source=localhost;User ID=root;Password=sasa;CharSet=utf8;SslMode=None"));
        }

        public override IDbContext DbContext
        {
            get
            {
                return this._dbContext;
            }
        }

        public override void InitTable<TEntity>()
        {
            Type entityType = typeof(TEntity);

            string createTableScript = this.CreateTableScript(entityType);

            this.DbContext.Session.ExecuteNonQuery(createTableScript);
        }
        string CreateTableScript(Type entityType)
        {
            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entityType);
            string tableName = typeDescriptor.Table.Name;

            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE TABLE IF NOT EXISTS {this.QuoteName(tableName)}(");

            string c = "";
            foreach (var propertyDescriptor in typeDescriptor.PrimitivePropertyDescriptors)
            {
                sb.AppendLine(c);
                sb.Append($"  {this.BuildColumnPart(propertyDescriptor)}");
                c = ",";
            }

            if (typeDescriptor.PrimaryKeys.Count > 0)
            {
                string key = typeDescriptor.PrimaryKeys.First().Column.Name;
                sb.AppendLine(c);
                sb.Append($"PRIMARY KEY ({this.QuoteName(key)}) USING BTREE");
            }

            sb.AppendLine();
            sb.Append(");");

            return sb.ToString();
        }
        string QuoteName(string name)
        {
            return string.Concat("`", name, "`");
        }

        string BuildColumnPart(PrimitivePropertyDescriptor propertyDescriptor)
        {
            string part = $"{this.QuoteName(propertyDescriptor.Column.Name)} {this.GetMappedDbTypeName(propertyDescriptor)}";

            if (propertyDescriptor.IsAutoIncrement)
            {
                part += " AUTO_INCREMENT";
            }

            if (!propertyDescriptor.IsNullable)
            {
                part += " NOT NULL";
            }
            else
            {
                part += " NULL";
            }

            return part;
        }

        string GetMappedDbTypeName(PrimitivePropertyDescriptor propertyDescriptor)
        {
            Type type = propertyDescriptor.PropertyType.GetUnderlyingType();
            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            if (type == typeof(string))
            {
                int stringLength = propertyDescriptor.Column.Size ?? 4000;
                return $"varchar({stringLength})";
            }

            if (type == typeof(int))
            {
                return "int(11)";
            }

            if (type == typeof(byte))
            {
                return "int(11)";
            }

            if (type == typeof(Int16))
            {
                return "int(11)";
            }

            if (type == typeof(long))
            {
                return "bigint";
            }

            if (type == typeof(double))
            {
                return "float(10, 4)";
            }

            if (type == typeof(float))
            {
                return "float(10, 4)";
            }

            if (type == typeof(decimal))
            {
                return "decimal(10, 4)";
            }

            if (type == typeof(bool))
            {
                return "int(11)";
            }

            if (type == typeof(DateTime))
            {
                return "datetime(0)";
            }

            if (type == typeof(Guid))
            {
                return "varchar(50)";
            }

            throw new NotSupportedException(type.FullName);
        }
    }

}
