using System;
using System.Linq.Expressions;
using Chloe.DbExpressions;
using Chloe.Extensions;
using Chloe.Utility;

namespace Chloe.Query.Visitors
{
    internal class GeneralExpressionVisitor : ExpressionVisitorBase
    {
        IMappingObjectExpression _fromTable;
        public GeneralExpressionVisitor(IMappingObjectExpression fromTable)
        {
            this._fromTable = fromTable;
        }

        protected override DbExpression VisitMemberAccess(MemberExpression exp)
        {
            DeriveType deriveType = exp.GetMemberExpressionDeriveType();

            if (deriveType == DeriveType.Parameter)
            {
                // 派生自参数
                return this._fromTable.GetDbExpression(exp);
            }
            else
            {
                return base.VisitMemberAccess(exp);
            }
        }

        protected override DbExpression VisitParameter(ParameterExpression exp)
        {
            //TODO 只支持 MappingFieldExpression 类型，即类似 q.Select(a=> a.Id).Where(a=> a > 0) 这种情况，也就是 ParameterExpression.Type 为基本映射类型。

            if (Utils.IsMapType(exp.Type))
            {
                MappingFieldExpression mfe = (MappingFieldExpression)this._fromTable;
                return mfe.Expression;
            }
            else
                throw new NotSupportedException(exp.ToString());
        }
    }
}
