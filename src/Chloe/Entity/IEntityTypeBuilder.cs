using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Chloe.Entity
{
    public interface IEntityTypeBuilder
    {
        EntityType EntityType { get; }
        IEntityTypeBuilder MapTo(string table);
        IEntityTypeBuilder HasSchema(string schema);
        IEntityTypeBuilder HasAnnotation(object value);
        IEntityTypeBuilder Ignore(string property);
        IPrimitivePropertyBuilder Property(string property);
        IComplexPropertyBuilder HasOne(string property);
        ICollectionPropertyBuilder HasMany(string property);
    }

    public interface IEntityTypeBuilder<TEntity> : IEntityTypeBuilder
    {
        new IEntityTypeBuilder<TEntity> MapTo(string table);
        new IEntityTypeBuilder<TEntity> HasSchema(string schema);
        new IEntityTypeBuilder<TEntity> HasAnnotation(object value);
        IEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> property);
        IPrimitivePropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> property);
        IComplexPropertyBuilder<TProperty> HasOne<TProperty>(Expression<Func<TEntity, TProperty>> property);
        ICollectionPropertyBuilder<TProperty> HasMany<TProperty>(Expression<Func<TEntity, TProperty>> property);
    }
}
