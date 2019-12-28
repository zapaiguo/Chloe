using Chloe.Core.Emit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Chloe.Reflection
{
    public class MemberValueGetterContainer
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, MemberValueGetter> Cache = new System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, MemberValueGetter>();
        public static MemberValueGetter GetMemberValueGetter(MemberInfo memberInfo)
        {
            MemberValueGetter getter = Cache.GetOrAdd(memberInfo, member =>
            {
                return DelegateGenerator.CreateValueGetter(member);
            });

            return getter;
        }
    }
}
