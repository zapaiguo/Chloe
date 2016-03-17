using Chloe.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class MappingMemberDescriptor : MemberDescriptor
    {
        //Action<object, IDataReader, int> _valueSetter;
        public MappingMemberDescriptor(MemberInfo memberInfo, Type memberType, MappingTypeDescriptor declaringEntityDescriptor, string columnName)
            : base(memberInfo, memberType, declaringEntityDescriptor)
        {
            this.ColumnName = columnName;
        }

        public string ColumnName { get; private set; }
    }
}
