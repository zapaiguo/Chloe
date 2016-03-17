using Chloe.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Mapping
{
    public class MappingMember
    {
        public MappingMember(Type entityType)
        {
            this.EntityType = entityType;
            this.MappingMembers = new Dictionary<MemberInfo, int>();
            this.MappingNavMembers = new Dictionary<MemberInfo, MappingNavMember>();
        }
        public Type EntityType { get; set; }
        public Dictionary<MemberInfo, int> MappingMembers { get; private set; }
        public Dictionary<MemberInfo, MappingNavMember> MappingNavMembers { get; private set; }

        public IObjectActivtor CreateObjectActivtor()
        {
            /*
            * 根据 EntityType 生成 IObjectActivtor
            * 如果 EntityType 是匿名类型的话
           */

            EntityMapper mapper = EntityMapper.GetInstance(this.EntityType);
            List<IValueSetter> memberSetters = new List<IValueSetter>(this.MappingMembers.Count + this.MappingNavMembers.Count);
            foreach (var kv in this.MappingMembers)
            {
                Action<object, IDataReader, int> del = mapper.GetMemberSetter(kv.Key);
                MappingMemberBinder binder = new MappingMemberBinder(del, kv.Value);
                memberSetters.Add(binder);
            }

            foreach (var kv in this.MappingNavMembers)
            {
                Action<object, object> del = mapper.GetNavigationMemberSetter(kv.Key);
                IObjectActivtor memberActivtor = kv.Value.CreateObjectActivtor();
                NavigationMemberBinder binder = new NavigationMemberBinder(del, memberActivtor);
                memberSetters.Add(binder);
            }

            ObjectActivtor ret = new ObjectActivtor(mapper.InstanceCreator, memberSetters);

            return ret;
        }
    }
}
