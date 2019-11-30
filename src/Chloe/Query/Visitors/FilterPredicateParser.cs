using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Chloe.Query.Visitors
{
    class FilterPredicateParser : ExpressionVisitor<DbExpression>
    {
        public static DbExpression Parse(LambdaExpression filterPredicate, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            return GeneralExpressionParser.Parse(BooleanResultExpressionTransformer.Transform(filterPredicate), scopeParameters, scopeTables);
        }
    }
}
