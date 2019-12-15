using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace Chloe.Entity
{
    public class PrimitiveProperty : PropertyBase
    {
        public PrimitiveProperty(PropertyInfo property) : base(property)
        {
            this.ColumnName = property.Name;
        }

        public string ColumnName { get; set; }
        public string SequenceName { get; set; }
        public string SequenceSchema { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsNullable { get; set; }
        public DbType? DbType { get; set; }
        public int? Size { get; set; }
        public byte? Scale { get; set; }
        public byte? Precision { get; set; }

        internal void SetIsPrimaryKey(bool isPrimaryKey)
        {
            this.IsPrimaryKey = isPrimaryKey;

            if (isPrimaryKey && this.DbType == null && this.Property.PropertyType == typeof(string))
            {
                //如果主键是 string 类型并且未显示指定 DbType，默认为 AnsiString
                this.DbType = System.Data.DbType.AnsiString;
            }
        }

        public PrimitivePropertyDefinition MakeDefinition()
        {
            PrimitivePropertyDefinition definition = new PrimitivePropertyDefinition(this.Property, new DbColumn(this.ColumnName, this.Property.PropertyType, this.DbType, this.Size, this.Scale, this.Precision), this.IsPrimaryKey, this.IsAutoIncrement, this.IsNullable, this.SequenceName, this.SequenceSchema, this.Annotations);
            return definition;
        }
    }
}
