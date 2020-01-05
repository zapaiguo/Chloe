using Chloe.DbExpressions;

namespace Chloe.SQLite.MethodHandlers
{
    class DiffYears_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            SqlGenerator.Append_DiffYears(generator, exp.Arguments[0], exp.Arguments[1]);
        }
    }
}
