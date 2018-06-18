using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using Chloe.SqlServer.MethodHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Chloe.SqlServer
{
    partial class SqlGenerator : DbExpressionVisitor<DbExpression>
    {
        static Dictionary<string, IMethodHandler> GetMethodHandlers()
        {
            var methodHandlers = new Dictionary<string, IMethodHandler>();

            methodHandlers.Add("Equals", new Equals_Handler());
            methodHandlers.Add("NotEquals", new NotEquals_Handler());

            methodHandlers.Add("Trim", new Trim_Handler());
            methodHandlers.Add("TrimStart", new TrimStart_Handler());
            methodHandlers.Add("TrimEnd", new TrimEnd_Handler());
            methodHandlers.Add("StartsWith", new StartsWith_Handler());
            methodHandlers.Add("EndsWith", new EndsWith_Handler());
            methodHandlers.Add("ToUpper", new ToUpper_Handler());
            methodHandlers.Add("ToLower", new ToLower_Handler());
            methodHandlers.Add("Substring", new Substring_Handler());
            methodHandlers.Add("IsNullOrEmpty", new IsNullOrEmpty_Handler());
            methodHandlers.Add("Replace", new Replace_Handler());

            methodHandlers.Add("ToString", new ToString_Handler());
            methodHandlers.Add("Contains", new Contains_Handler());
            methodHandlers.Add("In", new In_Handler());

            methodHandlers.Add("Count", new Count_Handler());
            methodHandlers.Add("LongCount", new LongCount_Handler());
            methodHandlers.Add("Sum", new Sum_Handler());
            methodHandlers.Add("Max", new Max_Handler());
            methodHandlers.Add("Min", new Min_Handler());
            methodHandlers.Add("Average", new Average_Handler());

            methodHandlers.Add("AddYears", new AddYears_Handler());
            methodHandlers.Add("AddMonths", new AddMonths_Handler());
            methodHandlers.Add("AddDays", new AddDays_Handler());
            methodHandlers.Add("AddHours", new AddHours_Handler());
            methodHandlers.Add("AddMinutes", new AddMinutes_Handler());
            methodHandlers.Add("AddSeconds", new AddSeconds_Handler());
            methodHandlers.Add("AddMilliseconds", new AddMilliseconds_Handler());

            methodHandlers.Add("Parse", new Parse_Handler());

            methodHandlers.Add("NewGuid", new NewGuid_Handler());

            methodHandlers.Add("DiffYears", new DiffYears_Handler());
            methodHandlers.Add("DiffMonths", new DiffMonths_Handler());
            methodHandlers.Add("DiffDays", new DiffDays_Handler());
            methodHandlers.Add("DiffHours", new DiffHours_Handler());
            methodHandlers.Add("DiffMinutes", new DiffMinutes_Handler());
            methodHandlers.Add("DiffSeconds", new DiffSeconds_Handler());
            methodHandlers.Add("DiffMilliseconds", new DiffMilliseconds_Handler());
            methodHandlers.Add("DiffMicroseconds", new DiffMicroseconds_Handler());

            methodHandlers.Add("Abs", new Abs_Handler());

            methodHandlers.Add("NextValueForSequence", new NextValueForSequence_Handler());

            var ret = Utils.Clone(methodHandlers);
            return ret;
        }
    }
}
