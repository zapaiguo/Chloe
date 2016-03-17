using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Core
{
    public interface IOrderedGroupingQuery<T> : IGroupingQuery<T>
    {
        IOrderedGroupingQuery<T> ThenByAsc<K>(Expression<Func<T, K>> predicate);
        IOrderedGroupingQuery<T> ThenByDesc<K>(Expression<Func<T, K>> predicate);
    }

}
