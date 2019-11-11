using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Chloe
{
    public interface IIncludableQuery<TEntity, TNavigation> : IQuery<TEntity>
    {
        IIncludableQuery<TEntity, TNavigation> WithCodition(Expression<Func<TNavigation, bool>> predicate);
        IIncludableQuery<TEntity, TProperty> ThenInclude<TProperty>(Expression<Func<TNavigation, TProperty>> p);
        IIncludableQuery<TEntity, TCollectionItem> ThenIncludeMany<TCollectionItem>(Expression<Func<TNavigation, IEnumerable<TCollectionItem>>> p);
    }
}
