using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query;
using Chloe.Query.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Core
{
    //public interface IQuery //: IEnumerable
    //{
    //    QueryExpression QueryExpression { get; }
    //}

    public interface IQuery1 //: IEnumerable
    {
        QueryExpression QueryExpression { get; }
    }
    public abstract class QueryExpression1
    {
        public Type ElementType { get; set; }
        public abstract IEnumerable<Expression> Parameters { get; }
    }
 }
