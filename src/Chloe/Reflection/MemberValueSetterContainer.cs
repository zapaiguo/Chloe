using Chloe.Core.Emit;
using System.Reflection;

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
