using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Chloe
{
    public interface IIncludableQuery<TEntity, TProperty> : IQuery<TEntity>
    {
        IIncludableQuery<TEntity, TProperty> Where(Expression<Func<TProperty, bool>> predicate);
        IIncludableQuery<TEntity, TInclude> Include<TInclude>(Expression<Func<TProperty, TInclude>> p);
        IIncludableQuery<TEntity, TInclude> Include<TInclude>(Expression<Func<TProperty, IEnumerable<TInclude>>> p);
    }
}
