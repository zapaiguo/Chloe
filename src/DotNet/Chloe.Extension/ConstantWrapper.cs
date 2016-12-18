using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Extension
{
    internal class ConstantWrapper<T>
    {
        static readonly PropertyInfo PropertyOfValue = typeof(ConstantWrapper<T>).GetProperty("Value");
        public ConstantWrapper(T value)
        {
            this.Value = value;
        }
        public T Value { get; private set; }
    }
}
