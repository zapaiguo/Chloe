using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query.DbExpressions;
using Chloe.Extensions;
using System.Reflection;
using Chloe.Utility;

namespace Chloe.Query.Implementation
{
    //主要检查子查询字段是否存在
    internal class MyExpressionVisitor2 : MyExpressionVisitor
    {
        private SelectedMembersInfo _prevSelectedMembersInfo;
        public MyExpressionVisitor2(QueryHelper queryHelper)
            : base(queryHelper)
        {
            _queryHelper = queryHelper;
        }
        public MyExpressionVisitor2(QueryHelper queryHelper, SelectedMembersInfo prevSelectedMembersInfo)
            : base(queryHelper)
        {
            _prevSelectedMembersInfo = prevSelectedMembersInfo;
        }

        protected override DbExpression VisitMemberAccess(MemberExpression exp)
        {
            DeriveType deriveType = exp.GetMemberExpDeriveType();

            if (deriveType == DeriveType.Parameter)
            {
                // 派生自参数
                return MemberAccessDeriveParameter(exp, _queryHelper, _prevSelectedMembersInfo);
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

        //主要检查子查询字段是否存在
        private static DbExpression MemberAccessDeriveParameter(MemberExpression exp, QueryHelper queryHelper, SelectedMembersInfo prevSelectedMembersInfo1)
        {
            SelectedMembersInfo prevSelectedMembersInfo = prevSelectedMembersInfo1;
            DbExpression dbExp = null;

            var stack = FillStack(exp);

            foreach (var m in stack)
            {
                var memberType = m.Member.GetPropertyOrFieldType();
                if (!Utils.IsMapType(memberType))
                {
                    SelectedMembersInfo nextSelectedMembersInfo;
                    if (!prevSelectedMembersInfo.SelectedNavMembers.TryGetValue(m.Member, out nextSelectedMembersInfo))
                    {
                        throw new Exception(string.Format("指定 select 成员中未包含 {0}", m.Member.Name));
                    }

                    prevSelectedMembersInfo = nextSelectedMembersInfo;
                }
                else
                {
                    if (dbExp == null)
                    {
                        IDictionary<MemberInfo, KeyValuePair<string, string>> p = prevSelectedMembersInfo.SelectedMembers;
                        KeyValuePair<string, string> columnAlias;
                        if (!p.TryGetValue(m.Member, out columnAlias))
                        {
                            throw new Exception(string.Format("指定 select 成员中未包含 {0}", m.Member.Name));
                        }
                        dbExp = DbExpression.ColumnAccess(columnAlias.Key, prevSelectedMembersInfo.TableAlias, m.Member);
                    }
                    else
                        dbExp = DbExpression.MemberAccess(m.Member, dbExp);
                }
            }

            return dbExp;
        }
    }
}
