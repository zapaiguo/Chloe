using System;
using System.Reflection;

namespace Chloe.Descriptors
{
    public class MappingMemberDescriptor : MemberDescriptor
    {
        public MappingMemberDescriptor(MemberInfo memberInfo, Type memberType, MappingTypeDescriptor declaringEntityDescriptor, string columnName)
            : base(memberInfo, memberType, declaringEntityDescriptor)
        {
            this.ColumnName = columnName;
        }

        public string ColumnName { get; private set; }
    }
}
