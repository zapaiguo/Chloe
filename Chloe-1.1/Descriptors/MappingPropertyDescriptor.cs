using System.Reflection;

namespace Chloe.Descriptors
{
    public class MappingPropertyDescriptor : MappingMemberDescriptor
    {
        public MappingPropertyDescriptor(PropertyInfo propertyInfo, MappingTypeDescriptor declaringEntityDescriptor, string columnName)
            : base(propertyInfo, propertyInfo.PropertyType, declaringEntityDescriptor, columnName)
        {
        }
    }

}
