using System;
using System.Collections.Generic;
using System.Reflection;

namespace Chloe.Descriptors
{
    public abstract class MemberDescriptor
    {
        Dictionary<Type, Attribute> _customAttributes = new Dictionary<Type, Attribute>();
        protected MemberDescriptor(MappingTypeDescriptor declaringEntityDescriptor)
        {
            this.DeclaringEntityDescriptor = declaringEntityDescriptor;
        }

        public MappingTypeDescriptor DeclaringEntityDescriptor { get; set; }
        public abstract MemberInfo MemberInfo { get; }
        public abstract Type MemberInfoType { get; }
        public abstract MemberTypes MemberType { get; }

        public virtual Attribute GetCustomAttribute(Type attributeType)
        {
            Attribute val;
            if (!this._customAttributes.TryGetValue(attributeType, out val))
            {
                val = this.MemberInfo.GetCustomAttribute(attributeType);
                lock (this._customAttributes)
                {
                    this._customAttributes[attributeType] = val;
                }
            }

            return val;
        }

    }
}
