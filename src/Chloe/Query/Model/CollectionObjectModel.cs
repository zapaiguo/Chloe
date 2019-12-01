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
using Chloe.Mapper;

namespace Chloe.Query
{
    public class CollectionObjectModel : ObjectModelBase
    {
        Type _collectionType;

        public CollectionObjectModel(Type ownerType, PropertyInfo associatedProperty, ComplexObjectModel elementModel) : base(associatedProperty.PropertyType)
        {
            this.OwnerType = ownerType;
            this.AssociatedProperty = associatedProperty;
            this._collectionType = associatedProperty.PropertyType;
            this.ElementModel = elementModel;
        }

        public override TypeKind TypeKind { get { return TypeKind.Collection; } }
        public ComplexObjectModel ElementModel { get; private set; }
        public Type OwnerType { get; private set; }
        public PropertyInfo AssociatedProperty { get; private set; }

        public override IObjectActivatorCreator GenarateObjectActivatorCreator(DbSqlQueryExpression sqlQuery)
        {
            IObjectActivatorCreator elementActivatorCreator = this.ElementModel.GenarateObjectActivatorCreator(sqlQuery);
            CollectionObjectActivatorCreator ret = new CollectionObjectActivatorCreator(this._collectionType, this.OwnerType, elementActivatorCreator);
            return ret;
        }
    }
}
