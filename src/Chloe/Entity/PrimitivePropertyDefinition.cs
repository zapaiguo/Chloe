using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;

namespace Chloe.Entity
{
    public class PrimitivePropertyDefinition : PropertyDefinition
    {
        public PrimitivePropertyDefinition(PropertyInfo property, DbColumn column, bool isPrimaryKey, bool isAutoIncrement, string sequenceName, string sequenceSchema, IList<object> annotations) : base(property, annotations)
        {
            Utils.CheckNull(column, nameof(column));

            this.Column = column;
            this.IsPrimaryKey = isPrimaryKey;
            this.IsAutoIncrement = isAutoIncrement;
            this.SequenceName = sequenceName;
            this.SequenceSchema = sequenceSchema;
        }
        public override TypeKind Kind { get { return TypeKind.Primitive; } }
        public DbColumn Column { get; private set; }
        public bool IsPrimaryKey { get; private set; }
        public bool IsAutoIncrement { get; private set; }
        public string SequenceName { get; private set; }
        public string SequenceSchema { get; private set; }
    }
}
