using Chloe;
using Chloe.Core;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.SqlServer;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    class MsSqlDemo : DemoBase
    {
        MsSqlContext _dbContext;
        public MsSqlDemo()
        {
            this._dbContext = new MsSqlContext("Data Source = .;Initial Catalog = Chloe;Integrated Security = SSPI;");
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
            sb.AppendLine($"IF NOT EXISTS (select * from sysobjects where name='{tableName}' and xtype='U')");
            sb.Append($"CREATE TABLE {this.QuoteName(tableName)}(");

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
                string constraintName = $"PK_{tableName}";
                sb.AppendLine(c);
                sb.Append($"CONSTRAINT {this.QuoteName(constraintName)} PRIMARY KEY CLUSTERED ( {this.QuoteName(key)} ASC )");
            }

            sb.AppendLine();
            sb.Append(");");

            return sb.ToString();
        }
        string QuoteName(string name)
        {
            return string.Concat("[", name, "]");
        }

        string BuildColumnPart(PrimitivePropertyDescriptor propertyDescriptor)
        {
            string part = $"{this.QuoteName(propertyDescriptor.Column.Name)} {this.GetMappedDbTypeName(propertyDescriptor)}";

            if (propertyDescriptor.IsAutoIncrement)
            {
                part += " IDENTITY(1,1)";
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
                return $"NVARCHAR({stringLength})";
            }

            if (type == typeof(int))
            {
                return "int";
            }

            if (type == typeof(byte))
            {
                return "tinyint";
            }

            if (type == typeof(Int16))
            {
                return "smallint";
            }

            if (type == typeof(long))
            {
                return "bigint";
            }

            if (type == typeof(double))
            {
                return "float";
            }

            if (type == typeof(float))
            {
                return "real";
            }

            if (type == typeof(decimal))
            {
                int scale = propertyDescriptor.Column.Scale ?? 18;
                int precision = propertyDescriptor.Column.Precision ?? 2;
                return $"decimal({scale},{precision})";
            }

            if (type == typeof(bool))
            {
                return "bit";
            }

            if (type == typeof(DateTime))
            {
                return "datetime";
            }

            if (type == typeof(Guid))
            {
                return "uniqueidentifier";
            }

            throw new NotSupportedException(type.FullName);
        }
    }
}
