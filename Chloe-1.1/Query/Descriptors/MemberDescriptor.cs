using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class MemberDescriptor
    {
        public MemberDescriptor(MemberInfo memberInfo, Type memberType, MappingTypeDescriptor declaringEntityDescriptor)
        {
            this.MemberInfo = memberInfo;
            this.MemberType = memberType;
            this.DeclaringEntityDescriptor = declaringEntityDescriptor;
            this.Init();
        }

        void Init()
        {

        }
        public MemberInfo MemberInfo { get; set; }
        public Type MemberType { get; set; }
        public MappingTypeDescriptor DeclaringEntityDescriptor { get; set; }
    }
}
