using System;
using System.Reflection;

namespace Chloe.Descriptors
{
    public abstract class MemberDescriptor
    {
        protected MemberDescriptor(MappingTypeDescriptor declaringEntityDescriptor)
        {
            //this.MemberInfo = memberInfo;
            //this.MemberType = memberType;
            this.DeclaringEntityDescriptor = declaringEntityDescriptor;
        }

        public abstract MemberInfo MemberInfo { get; }
        //public abstract Type MemberType { get; }
        public MappingTypeDescriptor DeclaringEntityDescriptor { get; set; }
    }
}
