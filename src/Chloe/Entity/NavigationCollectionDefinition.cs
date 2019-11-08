using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Entity
{
    public class NavigationCollectionDefinition : PropertyDefinition
    {
        public NavigationCollectionDefinition(PropertyInfo property, IList<object> annotations) : base(property, annotations)
        {
            this.ElementType = property.PropertyType.GetGenericArguments().First();
        }

        public Type ElementType { get; private set; }
    }
}
