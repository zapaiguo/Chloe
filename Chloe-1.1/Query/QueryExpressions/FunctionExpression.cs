using Chloe.Query.QueryState;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    //public class FunctionExpression : QueryExpression
    //{
    //    static readonly List<Expression> EmptyParameterList = new List<Expression>(0);
    //    ReadOnlyCollection<Expression> _parameters;
    //    public FunctionExpression(Type elementType, QueryExpression prevExpression)
    //        : this(elementType, prevExpression, EmptyParameterList)
    //    {
    //    }
    //    public FunctionExpression(Type elementType, QueryExpression prevExpression, List<Expression> parameters)
    //        : base(QueryExpressionType.Function, elementType, prevExpression)
    //    {
    //        this._parameters = new ReadOnlyCollection<Expression>(parameters);
    //    }

    //    public IReadOnlyCollection<Expression> Parameters { get { return this._parameters; } }

    //    public override IQueryState Accept(IQueryState queryState)
    //    {
    //        IQueryState state = null;
    //        return state;
    //    }
    //    public override T Accept<T>(QueryExpressionVisitor<T> visitor)
    //    {
    //        return visitor.Visit(this);
    //    }
    //}
}
