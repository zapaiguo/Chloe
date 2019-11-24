using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Reflection;
using Chloe.InternalExtensions;
using Chloe.Utility;
using Chloe.Exceptions;
using Chloe.Query;

namespace Chloe.Descriptors
{
    public class TypeDescriptor
    {
        Dictionary<MemberInfo, PropertyDescriptor> _propertyDescriptorMap;
        Dictionary<MemberInfo, PrimitivePropertyDescriptor> _primitivePropertyDescriptorMap;
        Dictionary<MemberInfo, DbColumnAccessExpression> _primitivePropertyColumnMap;
        DefaultExpressionParser _expressionParser = null;

        public TypeDescriptor(TypeDefinition definition)
        {
            this.Definition = definition;
            this.PrimitivePropertyDescriptors = this.Definition.Properties.Select(a => new PrimitivePropertyDescriptor(a, this)).ToList().AsReadOnly();

            this.PrimaryKeys = this.PrimitivePropertyDescriptors.Where(a => a.Definition.IsPrimaryKey).ToList().AsReadOnly();
            this.AutoIncrement = this.PrimitivePropertyDescriptors.Where(a => a.Definition.IsAutoIncrement).FirstOrDefault();

            this._primitivePropertyDescriptorMap = PublicHelper.Clone(this.PrimitivePropertyDescriptors.ToDictionary(a => (MemberInfo)a.Definition.Property, a => a));
            this._primitivePropertyColumnMap = PublicHelper.Clone(this.PrimitivePropertyDescriptors.ToDictionary(a => (MemberInfo)a.Definition.Property, a => new DbColumnAccessExpression(this.Definition.Table, a.Definition.Column)));
        }

        public TypeDefinition Definition { get; private set; }
        public ReadOnlyCollection<PrimitivePropertyDescriptor> PrimitivePropertyDescriptors { get; private set; }
        public ReadOnlyCollection<ComplexPropertyDescriptor> ComplexPropertyDescriptors { get; private set; }
        public ReadOnlyCollection<CollectionPropertyDescriptor> CollectionPropertyDescriptors { get; private set; }
        public ReadOnlyCollection<PrimitivePropertyDescriptor> PrimaryKeys { get; private set; }
        /* It will return null if an entity has no auto increment member. */
        public PrimitivePropertyDescriptor AutoIncrement { get; private set; }

        public DbTable Table { get { return this.Definition.Table; } }

        public DefaultExpressionParser GetExpressionParser(DbTable explicitDbTable)
        {
            if (explicitDbTable == null)
            {
                if (this._expressionParser == null)
                    this._expressionParser = new DefaultExpressionParser(this, null);
                return this._expressionParser;
            }
            else
                return new DefaultExpressionParser(this, explicitDbTable);
        }

        public ConstructorInfo GetDefaultConstructor()
        {
            return this.Definition.Type.GetConstructor(Type.EmptyTypes);
        }

        public bool HasPrimaryKey()
        {
            return this.PrimaryKeys.Count > 0;
        }
        public PrimitivePropertyDescriptor FindPrimitivePropertyDescriptor(MemberInfo member)
        {
            member = member.AsReflectedMemberOf(this.Definition.Type);
            PrimitivePropertyDescriptor propertyDescriptor = this._primitivePropertyDescriptorMap.FindValue(member);
            return propertyDescriptor;
        }
        public PrimitivePropertyDescriptor GetPrimitivePropertyDescriptor(MemberInfo member)
        {
            PrimitivePropertyDescriptor propertyDescriptor = this.FindPrimitivePropertyDescriptor(member);
            if (propertyDescriptor == null)
                throw new ChloeException(string.Format("The member '{0}' does not map any column.", member.Name));

            return propertyDescriptor;
        }
        public PropertyDescriptor FindPropertyDescriptor(MemberInfo member)
        {
            member = member.AsReflectedMemberOf(this.Definition.Type);
            PropertyDescriptor propertyDescriptor = this._propertyDescriptorMap.FindValue(member);
            return propertyDescriptor;
        }
        public PropertyDescriptor GetPropertyDescriptor(MemberInfo member)
        {
            PropertyDescriptor propertyDescriptor = this.FindPropertyDescriptor(member);
            if (propertyDescriptor == null)
                throw new ChloeException($"Cannot find property descriptor which named {member.Name}");

            return propertyDescriptor;
        }
        public DbColumnAccessExpression FindColumnAccessExpression(MemberInfo member)
        {
            member = member.AsReflectedMemberOf(this.Definition.Type);
            DbColumnAccessExpression dbColumnAccessExpression = this._primitivePropertyColumnMap.FindValue(member);
            return dbColumnAccessExpression;
        }
        public DbColumnAccessExpression GetColumnAccessExpression(MemberInfo member)
        {
            DbColumnAccessExpression dbColumnAccessExpression = this.FindColumnAccessExpression(member);
            if (dbColumnAccessExpression == null)
                throw new ChloeException(string.Format("The member '{0}' does not map any column.", member.Name));

            return dbColumnAccessExpression;
        }

        internal ComplexObjectModel GenObjectModel(DbTable table)
        {
            ComplexObjectModel model = new ComplexObjectModel(this.Definition.Type);
            foreach (PrimitivePropertyDescriptor propertyDescriptor in this.PrimitivePropertyDescriptors)
            {
                DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(table, propertyDescriptor.Column);
                model.AddPrimitiveMember(propertyDescriptor.Property, columnAccessExpression);

                if (propertyDescriptor.IsPrimaryKey)
                    model.PrimaryKey = columnAccessExpression;
            }

            return model;
        }
    }
}
