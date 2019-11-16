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
    public class CollectionObjectModel : IObjectModel
    {
        Type _collectionType;

        public Type ObjectType { get { return this._collectionType; } }
        public TypeKind TypeKind { get { return TypeKind.Complex; } }
        public ComplexObjectModel ElementModel { get; private set; }

        public CollectionObjectModel(Type collectionType, ComplexObjectModel elementModel)
        {
            this._collectionType = collectionType;
            this.ElementModel = elementModel;
        }

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
            throw new NotSupportedException();
        }
        public IObjectModel GetComplexMember(MemberExpression exp)
        {
            throw new NotSupportedException();
        }

        public IObjectActivatorCreator GenarateObjectActivatorCreator(DbSqlQueryExpression sqlQuery)
        {
            //创建 List
            throw new NotImplementedException();
        }


        public IObjectModel ToNewObjectModel(DbSqlQueryExpression sqlQuery, DbTable table)
        {
            //创建 List
            throw new NotImplementedException();
        }

        public void SetNullChecking(DbExpression exp)
        {

        }
    }
}
