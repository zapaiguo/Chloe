using Chloe.Annotations;
using Chloe.Core;
using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Entity;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.Oracle
{
    public partial class OracleContext : DbContext
    {
        static PropertyDescriptor GetDefineSequencePropertyDescriptor(TypeDescriptor typeDescriptor, out string sequenceName)
        {
            sequenceName = null;
            PropertyDescriptor defineSequencePropertyDescriptor = typeDescriptor.PropertyDescriptors.Where(a => !string.IsNullOrEmpty(a.Definition.SequenceName)).FirstOrDefault();

            if (defineSequencePropertyDescriptor != null)
                EnsureDefineSequenceMemberType(defineSequencePropertyDescriptor);

            return defineSequencePropertyDescriptor;
        }

        static void EnsureDefineSequenceMemberType(PropertyDescriptor defineSequencePropertyDescriptor)
        {
            if (defineSequencePropertyDescriptor.MemberInfoType != UtilConstants.TypeOfInt16 && defineSequencePropertyDescriptor.MemberInfoType != UtilConstants.TypeOfInt32 && defineSequencePropertyDescriptor.MemberInfoType != UtilConstants.TypeOfInt64)
            {
                throw new ChloeException("Identity type must be Int16,Int32 or Int64.");
            }
        }
        static void EnsureMappingTypeHasPrimaryKey(TypeDescriptor typeDescriptor)
        {
            if (!typeDescriptor.HasPrimaryKey())
                throw new ChloeException(string.Format("Mapping type '{0}' does not define a primary key.", typeDescriptor.Definition.Type.FullName));
        }
        static object ConvertIdentityType(object identity, Type conversionType)
        {
            if (identity.GetType() != conversionType)
                return Convert.ChangeType(identity, conversionType);

            return identity;
        }
        static Dictionary<PropertyDescriptor, object> CreateKeyValueMap(TypeDescriptor typeDescriptor)
        {
            Dictionary<PropertyDescriptor, object> keyValueMap = new Dictionary<PropertyDescriptor, object>();
            foreach (PropertyDescriptor keyPropertyDescriptor in typeDescriptor.PrimaryKeys)
            {
                keyValueMap.Add(keyPropertyDescriptor, null);
            }

            return keyValueMap;
        }
        static DbExpression MakeCondition(Dictionary<PropertyDescriptor, object> keyValueMap, DbTable dbTable)
        {
            DbExpression conditionExp = null;
            foreach (var kv in keyValueMap)
            {
                PropertyDescriptor keyPropertyDescriptor = kv.Key;
                object keyVal = kv.Value;

                if (keyVal == null)
                    throw new ArgumentException(string.Format("The primary key '{0}' could not be null.", keyPropertyDescriptor.MemberInfo.Name));

                DbExpression left = new DbColumnAccessExpression(dbTable, keyPropertyDescriptor.Column);
                DbExpression right = DbExpression.Parameter(keyVal, keyPropertyDescriptor.MemberInfoType);
                DbExpression equalExp = new DbEqualExpression(left, right);
                conditionExp = conditionExp == null ? equalExp : DbExpression.And(conditionExp, equalExp);
            }

            return conditionExp;
        }

    }
}
