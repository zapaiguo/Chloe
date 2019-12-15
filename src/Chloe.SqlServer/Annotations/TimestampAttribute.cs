using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.SqlServer.Annotations
{
    /// <summary>
    /// 标识字段为 timestamp 类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [Obsolete("This attribute will be removed in future versions. Instead of using 'Chloe.Annotations.ColumnAttribute.IsRowVersion'.")]
    public class TimestampAttribute : Attribute
    {
        public TimestampAttribute()
        {
        }
    }
}
