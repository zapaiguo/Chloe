using Chloe.Core.Emit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Chloe.Reflection
{
    public class MemberValueSetterContainer
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, MemberValueSetter> Cache = new System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, MemberValueSetter>();
        public static MemberValueSetter GetMemberValueSetter(MemberInfo memberInfo)
        {
            MemberValueSetter setter = Cache.GetOrAdd(memberInfo, member =>
            {
                return DelegateGenerator.CreateValueSetter(member);
            });

            return setter;
        }
    }
}
