using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Core
{
    public interface IGroupingQuery<T> : IQuery
    {
        IGroupingQuery<T> ThenBy<K>(Expression<Func<T, K>> predicate);
        IGroupingQuery<T> Having<K>(Expression<Func<T, K>> predicate);
        IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
        //IOrderedGroupingQuery<T> OrderByAsc<K>(Expression<Func<T, K>> predicate);
        //IOrderedGroupingQuery<T> OrderByDesc<K>(Expression<Func<T, K>> predicate);
    }
}
