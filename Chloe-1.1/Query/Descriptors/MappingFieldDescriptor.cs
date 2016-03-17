using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class MappingFieldDescriptor : MappingMemberDescriptor
    {
        public MappingFieldDescriptor(FieldInfo fieldInfo, MappingTypeDescriptor declaringEntityDescriptor, string columnName)
            : base(fieldInfo, fieldInfo.FieldType, declaringEntityDescriptor, columnName)
        {
        }
    }
}
