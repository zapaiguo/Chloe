using Chloe.Core;
using Chloe.Core.Emit;
using Chloe.Query.Mapping;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Mapper
{
    public class EntityMemberMapper
    {
        Dictionary<MemberInfo, IMRM> _mappingMemberMRMContainer = new Dictionary<MemberInfo, IMRM>();
        Dictionary<MemberInfo, Action<object, object>> _navigationMemberSetters = new Dictionary<MemberInfo, Action<object, object>>();

        EntityMemberMapper(Type t)
        {
            this.Type = t;
            this.Init();
        }

        void Init()
        {
            Type t = this.Type;
            var members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance);

            foreach (var member in members)
            {
                Type memberType = null;
                PropertyInfo prop = null;
                FieldInfo field = null;

                if ((prop = member as PropertyInfo) != null)
                {
                    if (prop.GetSetMethod() == null)
                        continue;//对于没有公共的 setter 直接跳过
                    memberType = prop.PropertyType;
                }
                else if ((field = member as FieldInfo) != null)
                {
                    memberType = field.FieldType;
                }
                else
                    continue;//只支持公共属性和字段

                if (Utils.IsMapType(memberType))
                {
                    IMRM mrm = MRMHelper.CreateMRM(member);
                    this._mappingMemberMRMContainer.Add(member, mrm);
                }
                else
                {
                    if (prop != null)
                    {
                        Action<object, object> valueSetter = DelegateGenerator.CreateValueSetter(prop);
                        this._navigationMemberSetters.Add(member, valueSetter);
                    }
                    else if (field != null)
                    {
                        Action<object, object> valueSetter = DelegateGenerator.CreateValueSetter(field);
                        this._navigationMemberSetters.Add(member, valueSetter);
                    }
                    else
                        continue;

                    continue;
                }
            }
        }

        public Type Type { get; private set; }

        public IMRM GetMemberMapper(MemberInfo memberInfo)
        {
            IMRM mapper = null;
            this._mappingMemberMRMContainer.TryGetValue(memberInfo, out mapper);
            return mapper;
        }
        public Action<object, object> GetNavigationMemberSetter(MemberInfo memberInfo)
        {
            Action<object, object> valueSetter = null;
            this._navigationMemberSetters.TryGetValue(memberInfo, out valueSetter);
            return valueSetter;
        }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, EntityMemberMapper> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, EntityMemberMapper>();

        public static EntityMemberMapper GetInstance(Type type)
        {
            EntityMemberMapper instance;
            if (!InstanceCache.TryGetValue(type, out instance))
            {
                lock (type)
                {
                    if (!InstanceCache.TryGetValue(type, out instance))
                    {
                        instance = new EntityMemberMapper(type);
                        InstanceCache.GetOrAdd(type, instance);
                    }
                }
            }

            return instance;
        }
    }
}
