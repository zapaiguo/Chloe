using Chloe.Query.DbExpressions;
using Chloe.Query.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class MappingMembers
    {
        public MappingMembers(ConstructorInfo constructor)
        {
            this.Constructor = constructor;
            this.SelectedMembers = new Dictionary<MemberInfo, DbExpression>();
            this.SubResultEntities = new Dictionary<MemberInfo, MappingMembers>();
        }
        /// <summary>
        /// 返回类型
        /// </summary>
        public ConstructorInfo Constructor { get; protected set; }
        public Dictionary<ParameterInfo, DbExpression> ConstructorParameters { get; private set; }
        public Dictionary<ParameterInfo, MappingMembers> ConstructorEntityParameters { get; private set; }
        public Dictionary<MemberInfo, DbExpression> SelectedMembers { get; protected set; }
        public Dictionary<MemberInfo, MappingMembers> SubResultEntities { get; protected set; }
        //public bool IsIncludeMember { get; set; }

        /// <summary>
        /// 当 IsIncludeMember 为 true 时，AssociatingMemberInfo 为导航属性中相对应的关联属性 如 T.UserId=User.Id ,则 AssociatingMemberInfo 为 User.Id
        /// </summary>
        public MemberInfo AssociatingMemberInfo { get; set; }
        public DbExpression GetDbExpression(MemberInfo memberInfo)
        {
            DbExpression ret = null;
            if (!this.SelectedMembers.TryGetValue(memberInfo, out ret))
            {
                return null;
            }

            return ret;
        }
        public MappingMembers GetSubResultEntity(MemberInfo memberInfo)
        {
            MappingMembers ret = null;
            if (!this.SubResultEntities.TryGetValue(memberInfo, out ret))
            {
                return null;
            }

            return ret;
        }

        //public void FillColumnList(List<DbColumnExpression> columnList, MappingEntity mappingMember)
        //{
        //    MappingMembers mappingMembers = this;
        //    foreach (var kv in mappingMembers.SelectedMembers)
        //    {
        //        MemberInfo member = kv.Key;
        //        DbExpression exp = kv.Value;

        //        DbColumnExpression columnExp = new DbColumnExpression(exp.Type, exp, member.Name);
        //        columnList.Add(columnExp);

        //        if (mappingMember != null)
        //        {
        //            int ordinal = columnList.Count - 1;
        //            mappingMember.Members.Add(member, ordinal);
        //        }
        //    }

        //    foreach (var kv in mappingMembers.SubResultEntities)
        //    {
        //        MemberInfo member = kv.Key;
        //        MappingMembers val = kv.Value;

        //        MappingEntityMember navMappingMember = null;
        //        if (mappingMember != null)
        //        {
        //            navMappingMember = new MappingEntityMember(val.Constructor);
        //            mappingMember.EntityMembers.Add(kv.Key, navMappingMember);

        //            //TODO 设置 AssociatingColumnOrdinal
        //            //if (val.IsIncludeMember)
        //            //{
        //            //TODO 获取关联的键
        //            navMappingMember.AssociatingMemberInfo = val.AssociatingMemberInfo;
        //            //}
        //        }

        //        val.FillColumnList(columnList, navMappingMember);
        //    }
        //}

        public MappingEntity GetMappingEntity(DbSqlQueryExpression sqlQuery)
        {
            List<DbColumnExpression> columnList = sqlQuery.Columns;
            MappingEntity mappingEntity = new MappingEntity(this.Constructor);
            MappingMembers mappingMembers = this;
            foreach (var kv in mappingMembers.SelectedMembers)
            {
                MemberInfo member = kv.Key;
                DbExpression exp = kv.Value;

                string alias = sqlQuery.GenerateUniqueColumnAlias(member.Name);
                DbColumnExpression columnExp = new DbColumnExpression(exp.Type, exp, alias);
                columnList.Add(columnExp);

                int ordinal = columnList.Count - 1;
                mappingEntity.Members.Add(member, ordinal);
            }

            foreach (var kv in mappingMembers.SubResultEntities)
            {
                MemberInfo member = kv.Key;
                MappingMembers val = kv.Value;

                MappingEntity navMappingMember = val.GetMappingEntity(sqlQuery);
                mappingEntity.EntityMembers.Add(kv.Key, navMappingMember);
            }

            return mappingEntity;
        }

    }
}
