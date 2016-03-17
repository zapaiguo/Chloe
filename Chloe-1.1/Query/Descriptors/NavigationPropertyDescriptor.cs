using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class NavigationPropertyDescriptor : NavigationMemberDescriptor
    {
        PropertyInfo propertyInfo;
        public NavigationPropertyDescriptor(PropertyInfo propertyInfo, MappingTypeDescriptor declaringEntityDescriptor, string thisKey, string associatingKey)
            : base(propertyInfo, propertyInfo.PropertyType, declaringEntityDescriptor, thisKey, associatingKey)
        {
            this.propertyInfo = propertyInfo;
        }
    }
}
