using Chloe.DbExpressions;
using System.Linq;

namespace Chloe.PostgreSQL.MethodHandlers
{
    class EndsWith_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_EndsWith)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" ILIKE '%' || ");
            exp.Arguments.First().Accept(generator);
        }
    }

}
