using Chloe.Core.Emit;
using Chloe.DbExpressions;
using Chloe.Entity;
using Chloe.InternalExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using System.Threading;

namespace Chloe.Descriptors
{
    public class MappingPropertyDescriptor : PropertyDescriptor
    {
        public MappingPropertyDescriptor(MappingPropertyDefinition definition, TypeDescriptor declaringTypeDescriptor) : base(definition, declaringTypeDescriptor)
        {
            this.Definition = definition;
        }

        public new MappingPropertyDefinition Definition { get; private set; }

        public bool IsPrimaryKey { get { return this.Definition.IsPrimaryKey; } }
        public bool IsAutoIncrement { get { return this.Definition.IsAutoIncrement; } }
        public DbColumn Column { get { return this.Definition.Column; } }


        public bool HasSequence()
        {
            return !string.IsNullOrEmpty(this.Definition.SequenceName);
        }
    }
}
