using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query;
using Chloe.Utility;

namespace Chloe.Core
{
    internal class EntityDescriptor
    {
        private Dictionary<MemberInfo, EntityMapMember> _mapMembers;
        private Dictionary<MemberInfo, EntityNavMember> _navMembers;

        public EntityDescriptor(Type t)
        {
            this.EntityType = t;
            #region 设置表名
            var tableFlags = t.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableFlags.Length > 0)
            {
                TableAttribute tableFlag = (TableAttribute)tableFlags.First();
                if (tableFlag.Name != null)
                    TableName = tableFlag.Name;
                else
                    TableName = t.Name;
            }
            else
                TableName = t.Name;
            #endregion

            #region 映射的列
            _mapMembers = new Dictionary<MemberInfo, EntityMapMember>();
            _navMembers = new Dictionary<MemberInfo, EntityNavMember>();

            List<MemberInfo> mapMemberList = new List<MemberInfo>();
            List<MemberInfo> navMemberList = new List<MemberInfo>();

            var members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance);

            foreach (var member in members)
            {
                var ignoreFlags = member.GetCustomAttributes(typeof(IgnoreAttribute), true);
                if (ignoreFlags.Length > 0)
                    continue;

                Type memberType = null;
                PropertyInfo prop = null;
                FieldInfo field = null;
                if ((prop = member as PropertyInfo) != null)
                {
                    if (prop.GetSetMethod() == null)
                        throw new Exception(string.Format("属性 {0} 未包含公共 Setter", prop.Name));
                    memberType = prop.PropertyType;
                }
                else if ((field = member as FieldInfo) != null)
                {
                    memberType = field.FieldType;
                }
                else
                    continue;//只支持公共属性和字段


                if (!Utils.IsMapType(memberType))
                {
                    var associationFlags = member.GetCustomAttributes(typeof(AssociationAttribute), true);
                    if (associationFlags.Length > 0)
                    {
                        var associationFlag = (AssociationAttribute)associationFlags.First();

                        EntityNavMember entityNavMember = new EntityNavMember(this, member, associationFlag.ThisKey, associationFlag.OtherKey);
                        NavMembers.Add(member, entityNavMember);
                    }
                    //else
                    //    throw new Exception(string.Format("成员 {0} 必须添加 AssociationAttribute 标识，若非导航属性，须添加 IgnoreAttribute 标识", member.Name));
                    continue;
                }
                mapMemberList.Add(member);
            }

            mapMemberList = mapMemberList.OrderBy(a => a.GetHashCode()).ToList();

            int count = mapMemberList.Count;
            for (int i = 0; i < count; i++)
            {
                MemberInfo memberInfo = mapMemberList[i];

                EntityMapMember entityMapMember = new EntityMapMember();
                entityMapMember.OrderedIndex = i;

                var columnFlags = memberInfo.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnFlags.Length > 0)
                {
                    ColumnAttribute columnFlag = (ColumnAttribute)columnFlags.First();
                    if (columnFlag.Name != null)
                        entityMapMember.ColumnName = columnFlag.Name;
                    else
                        entityMapMember.ColumnName = memberInfo.Name;
                    if (columnFlag.IsAutoIncrement)
                    {
                        if (AutoIncrementMember == null)
                            AutoIncrementMember = entityMapMember;
                        else
                            throw new Exception(string.Format("实体 {0} 多个成员被标识为自增长", this.EntityType.Name));
                    }
                    if (columnFlag.IsPrimaryKey)
                    {
                        this.PrimaryKeyMembers.Add(entityMapMember);
                    }
                }
                else
                    entityMapMember.ColumnName = memberInfo.Name;
                entityMapMember.MemberInfo = memberInfo;

                if (this.MapMembers.Any(a => a.Value.ColumnName == entityMapMember.ColumnName))
                    throw new Exception(string.Format("实体 {0} 映射异常: Database 字段 {1} 映射多个成员", this.EntityType.Name, entityMapMember.ColumnName));

                MapMembers.Add(memberInfo, entityMapMember);
            }

            this.PrimaryKeyMembers.TrimExcess();
            #endregion
        }

        public Type EntityType { get; private set; }

        /// <summary>
        /// 真实的表名
        /// </summary>
        public string TableName { get; private set; }
        /// <summary>
        /// 主键列
        /// </summary>
        public EntityMapMember PrimaryKeyMember { get; private set; }
        /// <summary>
        /// 自增长列
        /// </summary>
        public EntityMapMember AutoIncrementMember { get; private set; }

        private List<EntityMapMember> _primaryKeyMembers = new List<EntityMapMember>();
        public List<EntityMapMember> PrimaryKeyMembers
        {
            get { return _primaryKeyMembers; }
        }

        /// <summary>
        /// 映射的成员集合
        /// </summary>
        public Dictionary<MemberInfo, EntityMapMember> MapMembers { get { return _mapMembers; } }
        /// <summary>
        /// 导航属性集合
        /// </summary>
        public Dictionary<MemberInfo, EntityNavMember> NavMembers { get { return _navMembers; } }

        private Func<IDataReader, ObjectCreateContext, object> _Generator;
        /// <summary>
        /// 对象生成器
        /// </summary>
        public Func<IDataReader, ObjectCreateContext, object> Generator
        {
            get
            {
                if (_Generator == null)
                {
                    _Generator = DelegateCreateManage.CreateObjectGenerator(this);
                }
                return _Generator;
            }
        }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, EntityDescriptor> _entityDescriptorCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, EntityDescriptor>();

        public static EntityDescriptor GetEntityDescriptor(Type type)
        {
            EntityDescriptor entityDescriptor;
            if (!_entityDescriptorCache.TryGetValue(type, out entityDescriptor))
            {
                entityDescriptor = new EntityDescriptor(type);
                entityDescriptor = _entityDescriptorCache.GetOrAdd(type, entityDescriptor);
            }

            return entityDescriptor;
        }
    }
}
