using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Chloe.Mapper
{
    public class ArgumentActivatorEnumerator
    {
        List<IObjectActivator> _objectActivators;
        int _next;

        public static readonly MethodInfo MethodOfNext;
        static ArgumentActivatorEnumerator()
        {
            MethodInfo method = typeof(ArgumentActivatorEnumerator).GetMethod("Next");
            MethodOfNext = method;
        }

        public ArgumentActivatorEnumerator(List<IObjectActivator> objectActivators)
        {
            this._objectActivators = objectActivators;
            this._next = 0;
        }
        public IObjectActivator Next()
        {
            IObjectActivator ret = this._objectActivators[this._next];
            this._next++;
            return ret;
        }
        public void Reset()
        {
            this._next = 0;
        }
    }

}
