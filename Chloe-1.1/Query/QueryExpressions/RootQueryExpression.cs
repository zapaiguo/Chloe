using Chloe.Query.QueryState;
using Chloe.Query.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class RootQueryExpression : QueryExpression
    {
        public RootQueryExpression(Type elementType)
            : base(QueryExpressionType.Root, elementType, null)
        {

        }

        public override IQueryState Accept(IQueryState queryState)
        {
            IQueryState state = new RootQueryState(this.ElementType);
            return state;
        }
        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
