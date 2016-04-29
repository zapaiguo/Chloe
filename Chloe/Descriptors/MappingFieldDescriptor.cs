using System.Reflection;
using System;
using Chloe.DbExpressions;

namespace Chloe.Descriptors
{
    public class MappingFieldDescriptor : MappingMemberDescriptor
    {
        FieldInfo _fieldInfo;
        DbColumn _column;
        public MappingFieldDescriptor(FieldInfo fieldInfo, MappingTypeDescriptor declaringEntityDescriptor, string columnName)
            : base(declaringEntityDescriptor)
        {
            this._fieldInfo = fieldInfo;
            this._column = new DbColumn(columnName, fieldInfo.FieldType);
        }


        public override MemberInfo MemberInfo
        {
            get { return this._fieldInfo; }
        }
        public override Type MemberInfoType
        {
            get { return this._fieldInfo.FieldType; }
        }
        public override MemberTypes MemberType
        {
            get { return MemberTypes.Field; }
        }
        public override DbColumn Column
        {
            get { return this._column; }
        }
    }
}
