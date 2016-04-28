using Chloe.DbExpressions;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Chloe.Descriptors
{
    public class MappingPropertyDescriptor : MappingMemberDescriptor
    {
        PropertyInfo _propertyInfo;
        DbColumn _column;
        public MappingPropertyDescriptor(PropertyInfo propertyInfo, MappingTypeDescriptor declaringEntityDescriptor, string columnName)
            : base(declaringEntityDescriptor)
        {
            this._propertyInfo = propertyInfo;
            this._column = new DbColumn(columnName, propertyInfo.PropertyType);
        }

        public override MemberInfo MemberInfo
        {
            get { return this._propertyInfo; }
        }
        public override Type MemberInfoType
        {
            get { return this._propertyInfo.PropertyType; }
        }
        public override MemberTypes MemberType
        {
            get { return MemberTypes.Property; }
        }
        public override DbColumn Column
        {
            get { return this._column; }
        }
        public override void SetValue(object instance, object value)
        {
            this._propertyInfo.SetValue(instance, value);
        }
    }

}
