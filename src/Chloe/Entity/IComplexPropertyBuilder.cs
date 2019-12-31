using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Chloe.Entity
{
    public interface IComplexPropertyBuilder
    {
        ComplexProperty Property { get; }
        IComplexPropertyBuilder WithForeignKey(string foreignKey);
    }

    public interface IComplexPropertyBuilder<TProperty, TEntity> : IComplexPropertyBuilder
    {
        new IComplexPropertyBuilder<TProperty, TEntity> WithForeignKey(string foreignKey);
        IComplexPropertyBuilder<TProperty, TEntity> WithForeignKey<TForeignKey>(Expression<Func<TEntity, TForeignKey>> foreignKey);
    }
}
