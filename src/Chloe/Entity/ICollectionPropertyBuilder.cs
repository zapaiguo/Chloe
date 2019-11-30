using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Entity
{
    public interface ICollectionPropertyBuilder
    {
        CollectionProperty Property { get; }
    }

    public interface ICollectionPropertyBuilder<TProperty> : ICollectionPropertyBuilder
    {

    }
}
