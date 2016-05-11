using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Chloe
{
    public interface IOrderedQuery<T> : IQuery<T>
    {
        IOrderedQuery<T> ThenBy<K>(Expression<Func<T, K>> predicate);
        IOrderedQuery<T> ThenByDesc<K>(Expression<Func<T, K>> predicate);
    }
}
