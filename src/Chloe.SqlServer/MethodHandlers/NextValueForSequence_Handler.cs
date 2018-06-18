using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chloe.DbExpressions;
using Chloe.InternalExtensions;

namespace Chloe.SqlServer.MethodHandlers
{
    class NextValueForSequence_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != UtilConstants.TypeOfSql)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            string sequenceName = (string)exp.Arguments[0].Evaluate();
            generator.SqlBuilder.Append("NEXT VALUE FOR ", sequenceName);
        }
    }
}
