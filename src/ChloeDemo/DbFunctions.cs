using Chloe.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChloeDemo
{
    public class DbFunctions
    {
        [DbFunctionAttribute()]
        public static string MyFunction(int value)
        {
            throw new NotImplementedException();
        }
    }
}
