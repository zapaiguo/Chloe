using Chloe.DbExpressions;

namespace Chloe.Oracle.MethodHandlers
{
    class LongCount_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            SqlGenerator.Aggregate_LongCount(generator);
        }
    }
}
