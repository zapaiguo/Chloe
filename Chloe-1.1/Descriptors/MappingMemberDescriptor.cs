using Chloe.Core;
using Chloe.DbExpressions;
using System;
using System.Reflection;

namespace Chloe.Descriptors
{
    public abstract class MappingMemberDescriptor : MemberDescriptor
    {
        Func<object, object> _valueGetter = null;
        protected MappingMemberDescriptor(MappingTypeDescriptor declaringEntityDescriptor)
            : base(declaringEntityDescriptor)
        {
        }

        public bool IsPrimaryKey { get; set; }

        public abstract DbColumn Column { get; }
        public virtual object GetValue(object instance)
        {
            if (null == this._valueGetter)
            {
                MemberInfo member = this.MemberInfo;
                lock (this)
                {
                    if (null == this._valueGetter)
                    {
                        this._valueGetter = DelegateGenerator.CreateValueGetter(member.DeclaringType, member);
                    }
                }
            }

            return this._valueGetter(instance);
        }
        public abstract void SetValue(object instance, object value);
    }
}
