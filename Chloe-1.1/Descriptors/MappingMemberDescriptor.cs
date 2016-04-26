using Chloe.DbExpressions;
using System;
using System.Reflection;

namespace Chloe.Descriptors
{
    public abstract class MappingMemberDescriptor : MemberDescriptor
    {
        protected MappingMemberDescriptor(MemberInfo memberInfo, Type memberType, MappingTypeDescriptor declaringEntityDescriptor, string columnName)
            : base(memberInfo, memberType, declaringEntityDescriptor)
        {
            this.Column = new DbColumnExpression(memberType, columnName);
        }

        public DbColumnExpression Column { get; private set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
    }
}
