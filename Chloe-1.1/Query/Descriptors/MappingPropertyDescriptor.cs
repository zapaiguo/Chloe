using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class MappingPropertyDescriptor : MappingMemberDescriptor
    {
        public MappingPropertyDescriptor(PropertyInfo propertyInfo, MappingTypeDescriptor declaringEntityDescriptor, string columnName)
            : base(propertyInfo, propertyInfo.PropertyType, declaringEntityDescriptor, columnName)
        {
        }
    }

}
