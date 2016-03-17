using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chloe.Extensions;

namespace Chloe.Core
{
    internal class EntityNavMember
    {
        MemberInfo _thisMapMember;
        MemberInfo _otherMapMember;

        public EntityNavMember(EntityDescriptor entityDescriptor, MemberInfo memberInfo, string thisKey, string otherKey)
        {
            EntityDescriptor = entityDescriptor;
            MemberInfo = memberInfo;
            ThisKey = thisKey;
            OtherKey = otherKey;
        }

        //定义该导航属性的实体的 EntityDescriptor
        public EntityDescriptor EntityDescriptor { get; private set; }
        //导航属性的 MemberInfo
        public MemberInfo MemberInfo { get; private set; }
        public string ThisKey { get; private set; }
        public string OtherKey { get; private set; }
        //public JoinType JoinType { get; set; }

        //定义导航属性实体对应的 MemberInfo
        public MemberInfo ThisMapMember
        {
            get
            {
                if (_thisMapMember == null)
                {
                    var kv = this.EntityDescriptor.MapMembers.Where(a => a.Key.Name == this.ThisKey).FirstOrDefault();
                    if (object.Equals(kv, default(KeyValuePair<MemberInfo, EntityMapMember>)))
                    {
                        throw new Exception(string.Format("定义导航属性 {0} 的类未含有名为 {1} 成员", this.MemberInfo.Name, this.ThisKey));
                    }
                    _thisMapMember = kv.Key;
                }
                return _thisMapMember;
            }
        }

        //导航属性实体对应的 MemberInfo
        public MemberInfo OtherMapMember
        {
            get
            {
                if (_otherMapMember == null)
                    _otherMapMember = this.MemberInfo.GetPropertyOrFieldType().GetMember(this.OtherKey, BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
                if (_otherMapMember == null)
                {
                    throw new Exception(string.Format("导航属性 {0} 的类型未含有名为 {1} 的成员", this.MemberInfo.Name, this.OtherKey));
                }
                return _otherMapMember;
            }
        }
    }
}
