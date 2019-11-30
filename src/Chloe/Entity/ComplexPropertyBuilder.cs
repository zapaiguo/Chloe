using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Entity
{
    public class ComplexPropertyBuilder<TProperty> : IComplexPropertyBuilder<TProperty>
    {
        public ComplexPropertyBuilder(ComplexProperty property)
        {
            this.Property = property;
        }
        public ComplexProperty Property { get; private set; }

        public IComplexPropertyBuilder<TProperty> HasForeignKey(string foreignKey)
        {
            this.Property.ForeignKey = foreignKey;
            return this;
        }

        IComplexPropertyBuilder IComplexPropertyBuilder.HasForeignKey(string foreignKey)
        {
            this.Property.ForeignKey = foreignKey;
            return this;
        }
    }
}
