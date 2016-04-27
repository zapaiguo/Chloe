using Chloe.DbExpressions;
using System.Reflection;

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

        public override MemberTypes MemberType
        {
            get { return MemberTypes.Property; }
        }
        public override MemberInfo MemberInfo
        {
            get { return this._propertyInfo; }
        }
        public override DbColumn Column
        {
            get { return this._column; }
        }
        public override object GetValue(object instance)
        {
            return this._propertyInfo.GetValue(instance);
        }
    }

}
