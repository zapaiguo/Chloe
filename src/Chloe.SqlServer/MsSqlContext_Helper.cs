using Chloe.Core;
using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Entity;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using Chloe.InternalExtensions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.SqlServer
{
    public partial class MsSqlContext : DbContext
    {
        static Action<TEntity, IDataReader> GetMapper<TEntity>(PropertyDescriptor propertyDescriptor, int ordinal)
        {
            Action<TEntity, IDataReader> mapper = (TEntity entity, IDataReader reader) =>
            {
                object value = reader.GetValue(ordinal);
                if (value == null || value == DBNull.Value)
                    throw new ChloeException("Unable to get the identity/sequence value.");

                value = PublicHelper.ConvertObjType(value, propertyDescriptor.PropertyType);
                propertyDescriptor.SetValue(entity, value);
            };

            return mapper;
        }

        static Dictionary<PropertyDescriptor, object> CreateKeyValueMap(TypeDescriptor typeDescriptor)
        {
            Dictionary<PropertyDescriptor, object> keyValueMap = new Dictionary<PropertyDescriptor, object>(typeDescriptor.PrimaryKeys.Count);
            foreach (PropertyDescriptor keyPropertyDescriptor in typeDescriptor.PrimaryKeys)
            {
                keyValueMap.Add(keyPropertyDescriptor, null);
            }

            return keyValueMap;
        }
        static SysType GetSysTypeByTypeName(string typeName)
        {
            SysType sysType;
            if (SysTypes.TryGetValue(typeName, out sysType))
            {
                return sysType;
            }

            throw new NotSupportedException(string.Format("Does not Support systype '{0}'", typeName));
        }
        static T GetValue<T>(IDataReader reader, string name)
        {
            object val = reader.GetValue(reader.GetOrdinal(name));
            if (val == DBNull.Value)
            {
                val = null;
                return (T)val;
            }

            return (T)Convert.ChangeType(val, typeof(T).GetUnderlyingType());
        }
        static string AppendInsertRangeSqlTemplate(DbTable table, List<PropertyDescriptor> mappingPropertyDescriptors)
        {
            StringBuilder sqlBuilder = new StringBuilder();

            sqlBuilder.Append("INSERT INTO ");
            sqlBuilder.Append(AppendTableName(table));
            sqlBuilder.Append("(");

            for (int i = 0; i < mappingPropertyDescriptors.Count; i++)
            {
                PropertyDescriptor mappingPropertyDescriptor = mappingPropertyDescriptors[i];
                if (i > 0)
                    sqlBuilder.Append(",");
                sqlBuilder.Append(Utils.QuoteName(mappingPropertyDescriptor.Column.Name));
            }

            sqlBuilder.Append(") VALUES");

            string sqlTemplate = sqlBuilder.ToString();
            return sqlTemplate;
        }
        static string AppendTableName(DbTable table)
        {
            if (string.IsNullOrEmpty(table.Schema))
                return Utils.QuoteName(table.Name);

            return string.Format("{0}.{1}", Utils.QuoteName(table.Schema), Utils.QuoteName(table.Name));
        }
    }
}
