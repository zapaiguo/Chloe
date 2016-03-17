using Chloe.Core;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Mapper
{
    public class EntityMapper
    {
        Dictionary<MemberInfo, Action<object, IDataReader, int>> _mappingMemberSetters = new Dictionary<MemberInfo, Action<object, IDataReader, int>>();
        Dictionary<MemberInfo, Action<object, object>> _navigationMemberSetters = new Dictionary<MemberInfo, Action<object, object>>();

        EntityMapper(Type t)
        {
            this.Type = t;
            this.Init();
        }

        /// <summary>
        /// 得考虑匿名类型
        /// </summary>
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
                    Action<object, IDataReader, int> valueSetter = DelegateCreateManage.CreateSetValueFromReaderDelegate(member);
                    this._mappingMemberSetters.Add(member, valueSetter);
                }
                else
                {
                    if (member.MemberType == MemberTypes.Property)
                    {
                        //throw new NotImplementedException();
                        Action<object, object> valueSetter = null;
                        this._navigationMemberSetters.Add(member, valueSetter);
                    }
                    else if (member.MemberType == MemberTypes.Field)
                    {
                        Action<object, object> valueSetter = null;
                        this._navigationMemberSetters.Add(member, valueSetter);
                    }
                    else
                        continue;

                    continue;
                }
            }
        }

        public Type Type { get; private set; }
        public Func<object> InstanceCreator { get; private set; }

        public Action<object, IDataReader, int> GetMemberSetter(MemberInfo memberInfo)
        {
            Action<object, IDataReader, int> valueSetter = null;
            this._mappingMemberSetters.TryGetValue(memberInfo, out valueSetter);
            return valueSetter;
        }
        public Action<object, object> GetNavigationMemberSetter(MemberInfo memberInfo)
        {
            Action<object, object> valueSetter = null;
            this._navigationMemberSetters.TryGetValue(memberInfo, out valueSetter);
            return valueSetter;
        }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, EntityMapper> _instanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, EntityMapper>();

        public static EntityMapper GetInstance(Type type)
        {
            EntityMapper instance;
            if (!_instanceCache.TryGetValue(type, out instance))
            {
                instance = new EntityMapper(type);
                instance = _instanceCache.GetOrAdd(type, instance);
            }

            return instance;
        }
    }
}
