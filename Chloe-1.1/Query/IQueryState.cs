using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query;
using System.Linq.Expressions;
using Chloe.Query.QueryExpressions;
using Chloe.Query.DbExpressions;
using Chloe.Query.QueryState;
using Chloe.Query.Mapping;

namespace Chloe.Query
{
    public interface IQueryState
    {
        /// <summary>
        /// 源表内所拥有的字段信息
        /// </summary>
        ResultElement Result { get; }
        void AppendWhereExpression(WhereExpression exp);
        void AppendOrderExpression(OrderExpression exp);
        void IncludeNavigationMember(Expression exp);
        IQueryState UpdateSelectResult(SelectExpression selectExpression);
        MappingData GenerateMappingData();
        //DbExpression ToDbExpression();
    }
}
