using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Entity
{
    public class CollectionPropertyBuilder<TProperty> : ICollectionPropertyBuilder<TProperty>
    {
        public CollectionPropertyBuilder(CollectionProperty property)
        {
            this.Property = property;
        }
        public CollectionProperty Property { get; private set; }
    }
}
