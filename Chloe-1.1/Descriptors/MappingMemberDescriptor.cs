using Chloe.DbExpressions;
using System;
using System.Reflection;

namespace Chloe.Descriptors
{
    public abstract class MappingMemberDescriptor : MemberDescriptor
    {
        protected MappingMemberDescriptor(MappingTypeDescriptor declaringEntityDescriptor)
            : base(declaringEntityDescriptor)
        {

        }

        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }

        public abstract MemberTypes MemberType { get; }
        public abstract DbColumn Column { get; }
        public abstract object GetValue(object instance);
    }
}
