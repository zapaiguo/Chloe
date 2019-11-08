using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;

namespace Chloe.Entity
{
    public class MappingPropertyDefinition : PropertyDefinition
    {
        public MappingPropertyDefinition(PropertyInfo property, DbColumn column, bool isPrimaryKey, bool isAutoIncrement, string sequenceName, IList<object> annotations) : base(property, annotations)
        {
            Utils.CheckNull(column, nameof(column));

            this.Column = column;
            this.IsPrimaryKey = isPrimaryKey;
            this.IsAutoIncrement = isAutoIncrement;
            this.SequenceName = sequenceName;
        }
        public DbColumn Column { get; private set; }
        public bool IsPrimaryKey { get; private set; }
        public bool IsAutoIncrement { get; private set; }
        public string SequenceName { get; private set; }
    }
}
