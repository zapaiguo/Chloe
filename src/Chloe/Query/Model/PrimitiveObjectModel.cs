using Chloe.Extensions;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Query.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    public class PrimitiveObjectModel : IObjectModel
    {
        Type _type;
        DbExpression _exp;
        public PrimitiveObjectModel(Type type, DbExpression exp)
        {
            this._type = type;
            this._exp = exp;
        }

        public Type ObjectType { get { return this._type; } }
        public TypeKind TypeKind { get { return TypeKind.Complex; } }
        public DbExpression Expression { get { return this._exp; } }

        public DbExpression NullChecking { get; set; }

        public void AddConstructorParameter(ParameterInfo p, DbExpression primitiveExp)
        {
            throw new NotSupportedException();
        }
        public void AddConstructorParameter(ParameterInfo p, ComplexObjectModel complexModel)
        {
            throw new NotSupportedException();
        }
        public void AddPrimitiveMember(MemberInfo p, DbExpression exp)
        {
            throw new NotSupportedException();
        }
        public DbExpression GetPrimitiveMember(MemberInfo memberInfo)
        {
            throw new NotSupportedException();
        }

        public void AddComplexMember(MemberInfo p, ComplexObjectModel model)
        {
            throw new NotSupportedException();
        }
        public ComplexObjectModel GetComplexMember(MemberInfo memberInfo)
        {
            throw new NotSupportedException();
        }

        public void AddCollectionMember(MemberInfo p, CollectionObjectModel model)
        {
            throw new NotSupportedException();
        }
        public CollectionObjectModel GetCollectionMember(MemberInfo memberInfo)
        {
            throw new NotSupportedException();
        }

        public DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter)
        {
            Stack<MemberExpression> memberExpressions = ExpressionExtension.Reverse(memberExpressionDeriveParameter);

            if (memberExpressions.Count == 0)
                throw new Exception();

            DbExpression ret = this._exp;

            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;
                ret = DbExpression.MemberAccess(member, ret);
            }

            if (ret == null)
                throw new Exception(memberExpressionDeriveParameter.ToString());

            return ret;
        }
        public IObjectModel GetComplexMember(MemberExpression exp)
        {
            throw new NotSupportedException();
        }

        public IObjectActivatorCreator GenarateObjectActivatorCreator(DbSqlQueryExpression sqlQuery)
        {
            int ordinal;
            ordinal = ObjectModelHelper.TryGetOrAddColumn(sqlQuery, this._exp).Value;

            PrimitiveObjectActivatorCreator activatorCreator = new PrimitiveObjectActivatorCreator(this._type, ordinal);

            activatorCreator.CheckNullOrdinal = ObjectModelHelper.TryGetOrAddColumn(sqlQuery, this.NullChecking);

            return activatorCreator;
        }


        public IObjectModel ToNewObjectModel(DbSqlQueryExpression sqlQuery, DbTable table)
        {
            DbColumnAccessExpression cae = null;
            cae = ObjectModelHelper.ParseColumnAccessExpression(sqlQuery, table, this._exp);

            PrimitiveObjectModel mf = new PrimitiveObjectModel(this._type, cae);

            mf.NullChecking = ObjectModelHelper.TryGetOrAddNullChecking(sqlQuery, table, this.NullChecking);

            return mf;
        }

        public void SetNullChecking(DbExpression exp)
        {
            if (this.NullChecking == null)
                this.NullChecking = exp;
        }

    }
}
