using Chloe.Query.QueryState;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class FunctionExpression : QueryExpression
    {
         MethodInfo _method;
        ReadOnlyCollection<Expression> _parameters;

        public FunctionExpression(Type elementType, QueryExpression prevExpression, MethodInfo method, List<Expression> parameters)
            : base(QueryExpressionType.Function, elementType, prevExpression)
        {
            this._method = method;
            this._parameters = new ReadOnlyCollection<Expression>(parameters);
        }

        public MethodInfo Method { get { return this._method; } }
        public IReadOnlyCollection<Expression> Parameters { get { return this._parameters; } }


        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
