using Chloe.Extensions;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Query.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Chloe.Infrastructure;

namespace Chloe.Query
{
    public class MappingCollectionExpression : IMappingObjectExpression
    {
        Type _collectionType;

        public Type ObjectType { get { return this._collectionType; } }
        public MappingObjectExpression ElementModel { get; private set; }

        public MappingCollectionExpression(Type collectionType, MappingObjectExpression elementModel)
        {
            this._collectionType = collectionType;
            this.ElementModel = elementModel;
        }

        public void AddMappingConstructorParameter(ParameterInfo p, DbExpression exp)
        {
            throw new NotSupportedException();
        }
        public void AddComplexConstructorParameter(ParameterInfo p, IMappingObjectExpression exp)
        {
            throw new NotSupportedException();
        }
        public void AddMappingMemberExpression(MemberInfo p, DbExpression exp)
        {
            throw new NotSupportedException();
        }
        public void AddComplexMemberExpression(MemberInfo p, IMappingObjectExpression exp)
        {
            throw new NotSupportedException();
        }
        public DbExpression GetMappingMemberExpression(MemberInfo memberInfo)
        {
            throw new NotSupportedException();
        }
        public IMappingObjectExpression GetComplexMemberExpression(MemberInfo memberInfo)
        {
            throw new NotSupportedException();
        }
        public DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter)
        {
            throw new NotSupportedException();
        }
        public IMappingObjectExpression GetComplexMemberExpression(MemberExpression exp)
        {
            throw new NotSupportedException();
        }

        public IObjectActivatorCreator GenarateObjectActivatorCreator(DbSqlQueryExpression sqlQuery)
        {
            //创建 List
            throw new NotImplementedException();
        }


        public IMappingObjectExpression ToNewObjectExpression(DbSqlQueryExpression sqlQuery, DbTable table)
        {
            //创建 List
            throw new NotImplementedException();
        }

        public void SetNullChecking(DbExpression exp)
        {

        }
    }
}
