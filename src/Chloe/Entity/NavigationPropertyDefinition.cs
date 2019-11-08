using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Entity
{
    public class NavigationPropertyDefinition : PropertyDefinition
    {
        public NavigationPropertyDefinition(PropertyInfo property, string foreignKey, IList<object> annotations) : base(property, annotations)
        {
            if (string.IsNullOrEmpty(foreignKey))
                throw new ArgumentException("'foreignKey' can not be null.");

            this.ForeignKey = foreignKey;
        }

        public string ForeignKey { get; private set; }
    }
}
