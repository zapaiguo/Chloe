using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Entity
{
    public interface IComplexPropertyBuilder
    {
        ComplexProperty Property { get; }
        IComplexPropertyBuilder HasForeignKey(string foreignKey);
    }

    public interface IComplexPropertyBuilder<TProperty> : IComplexPropertyBuilder
    {
        new IComplexPropertyBuilder<TProperty> HasForeignKey(string foreignKey);
    }
}
