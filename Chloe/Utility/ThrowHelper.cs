using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Utility
{
    public static class ThrowHelper
    {
        public static void ThrowNullReferenceException(string message = "未将对象引用设置到对象的实例。")
        {
            throw new NullReferenceException(message);
        }
        public static void ThrowInvalidCastException(string message = "指定的转换无效。")
        {
            throw new InvalidCastException(message);
        }
        public static void ThrowNotSupportedException(string message = null)
        {
            throw new NotSupportedException(message);
        }
    }
}
