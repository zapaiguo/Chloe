using Chloe.Query.DbExpressions;
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
        public MappingMembers(Type type)
        {
            this.Type = type;
            this.SelectedMembers = new Dictionary<MemberInfo, DbExpression>();
            this.SubResultEntities = new Dictionary<MemberInfo, MappingMembers>();
        }
        /// <summary>
        /// 返回类型
        /// </summary>
        public Type Type { get; protected set; }
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
    }
}
