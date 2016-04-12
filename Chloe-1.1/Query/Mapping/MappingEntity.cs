using Chloe.Mapper;
using Chloe.Descriptors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Chloe.Query.Mapping
{
    public class MappingEntity : IObjectActivtorCreator
    {
        public MappingEntity(EntityConstructorDescriptor constructorDescriptor)
        {
            //this.EntityType = constructor.DeclaringType;
            this.ConstructorDescriptor = constructorDescriptor;
            this.ConstructorParameters = new Dictionary<ParameterInfo, int>();
            this.ConstructorEntityParameters = new Dictionary<ParameterInfo, IObjectActivtorCreator>();
            this.Members = new Dictionary<MemberInfo, int>();
            this.EntityMembers = new Dictionary<MemberInfo, IObjectActivtorCreator>();
        }
        public EntityConstructorDescriptor ConstructorDescriptor { get; private set; }
        public Dictionary<ParameterInfo, int> ConstructorParameters { get; private set; }
        public Dictionary<ParameterInfo, IObjectActivtorCreator> ConstructorEntityParameters { get; private set; }

        public Dictionary<MemberInfo, int> Members { get; private set; }
        public Dictionary<MemberInfo, IObjectActivtorCreator> EntityMembers { get; private set; }

        public IObjectActivtor CreateObjectActivtor()
        {
            /*
            * 根据 EntityType 生成 IObjectActivtor
            * 如果 EntityType 是匿名类型的话
           */

            EntityMemberMapper mapper = this.ConstructorDescriptor.GetEntityMemberMapper();
            List<IValueSetter> memberSetters = new List<IValueSetter>(this.Members.Count + this.EntityMembers.Count);
            foreach (var kv in this.Members)
            {
                Action<object, IDataReader, int> del = mapper.GetMemberSetter(kv.Key);
                MappingMemberBinder binder = new MappingMemberBinder(del, kv.Value);
                memberSetters.Add(binder);
            }

            foreach (var kv in this.EntityMembers)
            {
                Action<object, object> del = mapper.GetNavigationMemberSetter(kv.Key);
                IObjectActivtor memberActivtor = kv.Value.CreateObjectActivtor();
                NavigationMemberBinder binder = new NavigationMemberBinder(del, memberActivtor);
                memberSetters.Add(binder);
            }

            Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivtorEnumerator, object> instanceCreator = this.ConstructorDescriptor.GetInstanceCreator();

            List<int> readerOrdinals = this.ConstructorParameters.Select(a => a.Value).ToList();
            List<IObjectActivtor> objectActivtors = this.ConstructorEntityParameters.Select(a => a.Value.CreateObjectActivtor()).ToList();

            ObjectActivtor ret = new ObjectActivtor(instanceCreator, readerOrdinals, objectActivtors, memberSetters);

            return ret;
        }
    }
}
