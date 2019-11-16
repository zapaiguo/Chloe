using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.Query.QueryExpressions
{
    class IncludeExpression : QueryExpression
    {
        public IncludeExpression(Type elementType, QueryExpression prevExpression, QueryExpressionType expressionType, NavigationNode navigationNode)
           : base(expressionType, elementType, prevExpression)
        {
            this.NavigationNode = navigationNode;
        }
        public NavigationNode NavigationNode { get; private set; }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class NavigationNode
    {
        public PropertyInfo Property { get; set; }
        public LambdaExpression Condition { get; set; }
        public LambdaExpression Filter { get; set; }
        public NavigationNode Next { get; set; }
    }
}
