using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query.DbExpressions;
using Chloe.Extensions;
using Chloe.Core;
using Chloe.Utility;

namespace Chloe.Query.Implementation
{
    //加入 JoinInfos
    internal class MyExpressionVisitor1 : MyExpressionVisitor
    {
        private JoinInfo _joinInfo;
        public MyExpressionVisitor1(QueryHelper queryHelper)
            : base(queryHelper)
        {
        }
        public MyExpressionVisitor1(QueryHelper queryHelper, JoinInfo joinInfo)
            : base(queryHelper)
        {
            _joinInfo = joinInfo;
        }

        protected override DbExpression VisitMemberAccess(MemberExpression exp)
        {
            DeriveType deriveType = exp.GetMemberExpDeriveType();

            if (deriveType == DeriveType.Parameter)
            {
                // 派生自参数
                return MemberAccessDeriveParameter(exp, _queryHelper, _joinInfo);
            }
            else if (deriveType == DeriveType.Constant)
            {
                // 派生自常量，则求值
                return base.MemberAccessDeriveConstant(exp);
            }
            else
            {
                return base.VisitMemberAccess(exp);
            }
        }

        private static DbExpression MemberAccessDeriveParameter(MemberExpression exp, QueryHelper queryHelper, JoinInfo joinInfo1)
        {
            JoinInfo joinInfo = joinInfo1;
            DbExpression dbExp = null;

            var stack = FillStack(exp);

            foreach (var m in stack)
            {
                var memberType = m.Member.GetPropertyOrFieldType();
                //if (typeof(IEntity).IsAssignableFrom(memberType))
                if (!Utils.IsMapType(memberType))
                {
                    //加入 JoinInfos
                    JoinInfo newJoinInfo;
                    if (!joinInfo.JoinInfos.TryGetValue(m.Member, out newJoinInfo))
                    {
                        EntityNavMember entityNavMember;
                        //判断是否为导航属性
                        if (!joinInfo.EntityDescriptor.NavMembers.TryGetValue(m.Member, out entityNavMember))
                        {
                            //throw new Exception(string.Format("Select 成员中未包含 {0}", m.Member.Name));
                            throw new Exception(string.Format("成员 {0} 必须添加 AssociationAttribute 标识", m.Member.Name));
                        }


                        EntityDescriptor ed = EntityDescriptor.GetEntityDescriptor(memberType);
                        string alias = queryHelper.GetTableAlias(ed.TableName);
                        newJoinInfo = new JoinInfo(ed, alias);

                        //var prevEntityNavMember = joinInfo.EntityDescriptor.NavMembers[m.Member];
                        string thisAssociationColumn = ed.MapMembers[entityNavMember.OtherMapMember].ColumnName;
                        string associationKey = entityNavMember.EntityDescriptor.MapMembers[entityNavMember.ThisMapMember].ColumnName;

                        newJoinInfo.ThisAssociationKey = thisAssociationColumn;
                        newJoinInfo.AssociationTableAlias = joinInfo.Alias;
                        newJoinInfo.AssociationKey = associationKey;
                        joinInfo.JoinInfos.Add(m.Member, newJoinInfo);
                    }

                    joinInfo = newJoinInfo;
                }
                else
                {
                    if (dbExp == null)
                        dbExp = DbExpression.ColumnAccess(joinInfo.EntityDescriptor.MapMembers[m.Member].ColumnName, joinInfo.Alias, m.Member);
                    else
                        dbExp = DbExpression.MemberAccess(m.Member, dbExp);
                }
            }

            return dbExp;
        }
    }
}
