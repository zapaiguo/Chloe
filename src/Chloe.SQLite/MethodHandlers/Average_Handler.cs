using Chloe.DbExpressions;
using System.Linq;

namespace Chloe.SQLite.MethodHandlers
{
    class Average_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            SqlGenerator.Aggregate_Average(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }
    }
}
