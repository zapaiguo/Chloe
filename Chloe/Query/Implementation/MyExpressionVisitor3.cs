using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.Core;
using Chloe.Query.DbExpressions;
using Chloe.Extensions;
using Chloe.Utility;

namespace Chloe.Query.Implementation
{
    //用于 delete update 的条件
    internal class MyExpressionVisitor3 : MyExpressionVisitor
    {
        EntityDescriptor _entityDescriptor;
        public MyExpressionVisitor3(QueryHelper queryHelper)
            : base(queryHelper)
        {
            _queryHelper = queryHelper;
        }
        public MyExpressionVisitor3(QueryHelper queryHelper, EntityDescriptor entityDescriptor)
            : base(queryHelper)
        {
            _entityDescriptor = entityDescriptor;
        }

        protected override DbExpression VisitMemberAccess(MemberExpression exp)
        {
            DeriveType deriveType = exp.GetMemberExpDeriveType();

            if (deriveType == DeriveType.Parameter)
            {
                // 派生自参数
                return MemberAccessDeriveParameter(exp, _queryHelper, _entityDescriptor);
            }
            else if (deriveType == DeriveType.Constant)
            {
                //派生自常量，则求值
                return base.MemberAccessDeriveConstant(exp);
            }
            else
            {
                return base.VisitMemberAccess(exp);
            }
        }

        private static DbExpression MemberAccessDeriveParameter(MemberExpression exp, QueryHelper queryHelper, EntityDescriptor entityDescriptor1)
        {
            EntityDescriptor entityDescriptor = entityDescriptor1;
            DbExpression dbExp = null;

            var stack = FillStack(exp);

            foreach (var m in stack)
            {
                var memberType = m.Member.GetPropertyOrFieldType();
                //if (typeof(IEntity).IsAssignableFrom(memberType))
                if (!Utils.IsMapType(memberType))
                {
                    throw new NotSupportedException("不支持 " + stack.Last().ToString());
                }
                else
                {


                    if (dbExp == null)
                    {
                        EntityMapMember entityMapMember;
                        if (!entityDescriptor.MapMembers.TryGetValue(m.Member, out entityMapMember))
                        {
                            throw new Exception("Map 成员异常： " + m.Member.Name);
                        }
                        dbExp = DbExpression.ColumnAccess(entityMapMember.ColumnName, entityDescriptor.TableName, m.Member);
                    }
                    else
                        dbExp = DbExpression.MemberAccess(m.Member, dbExp);
                }
            }

            return dbExp;
        }
    }
}
