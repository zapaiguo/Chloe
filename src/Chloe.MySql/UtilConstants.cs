using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.MySql
{
    static class UtilConstants
    {
        public const string ParameterNamePlaceholer = "?";
        public static readonly string ParameterNamePrefix = ParameterNamePlaceholer + "P_";
    }
}
