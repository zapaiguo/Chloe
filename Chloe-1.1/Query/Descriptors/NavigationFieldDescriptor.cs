using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class NavigationFieldDescriptor : NavigationMemberDescriptor
    {
        FieldInfo _fieldInfo;
        public NavigationFieldDescriptor(FieldInfo fieldInfo, MappingTypeDescriptor declaringEntityDescriptor, string thisKey, string associatingKey)
            : base(fieldInfo, fieldInfo.FieldType, declaringEntityDescriptor, thisKey, associatingKey)
        {
            this._fieldInfo = fieldInfo;
        }
    }
}
