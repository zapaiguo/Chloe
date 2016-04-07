using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query.DbExpressions;
using Chloe.Extensions;

namespace Chloe.Query.Implementation
{
    internal class GeneralExpressionVisitor : BaseExpressionVisitor
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
    }
}
