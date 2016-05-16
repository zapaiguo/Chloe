using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Utility
{
    static class UtilExceptions
    {
        public static NullReferenceException NullReferenceException(string message = "未将对象引用设置到对象的实例。")
        {
            return new NullReferenceException(message);
        }
        public static InvalidCastException InvalidCastException(string message = "指定的转换无效。")
        {
            return new InvalidCastException(message);
        }
        public static NotSupportedException NotSupportedException(string message = null)
        {
            return new NotSupportedException(message);
        }
    }
}
