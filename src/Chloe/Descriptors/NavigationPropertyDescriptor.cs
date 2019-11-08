using Chloe.Entity;
using Chloe.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Descriptors
{
    public class NavigationPropertyDescriptor : PropertyDescriptor
    {
        public NavigationPropertyDescriptor(NavigationPropertyDefinition definition, TypeDescriptor declaringTypeDescriptor) : base(definition, declaringTypeDescriptor)
        {
            this.Definition = definition;

            MappingPropertyDescriptor foreignKeyProperty = declaringTypeDescriptor.PropertyDescriptors.Where(a => a.Property.Name == definition.ForeignKey).FirstOrDefault();

            if (foreignKeyProperty == null)
                throw new ChloeException($"Can not find property named '{definition.ForeignKey}'");

            this.ForeignKeyProperty = foreignKeyProperty;
        }

        public new NavigationPropertyDefinition Definition { get; private set; }
        public MappingPropertyDescriptor ForeignKeyProperty { get; private set; }
    }
}
