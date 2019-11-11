using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Entity
{
    public abstract class PropertyDefinition
    {
        protected PropertyDefinition(PropertyInfo property, IList<object> annotations)
        {
            Utils.CheckNull(property, nameof(property));
            Utils.CheckNull(annotations, nameof(annotations));

            this.Property = property;
            this.Annotations = annotations.Where(a => a != null).ToList().AsReadOnly();
        }
        public abstract TypeKind Kind { get; }
        public PropertyInfo Property { get; private set; }
        public ReadOnlyCollection<object> Annotations { get; private set; }
    }
}
