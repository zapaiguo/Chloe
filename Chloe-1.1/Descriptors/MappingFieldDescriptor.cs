using System.Reflection;

namespace Chloe.Descriptors
{
    public class MappingFieldDescriptor : MappingMemberDescriptor
    {
        public MappingFieldDescriptor(FieldInfo fieldInfo, MappingTypeDescriptor declaringEntityDescriptor, string columnName)
            : base(fieldInfo, fieldInfo.FieldType, declaringEntityDescriptor, columnName)
        {
        }
    }
}
