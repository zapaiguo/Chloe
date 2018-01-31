using System.Data;

namespace Chloe.Infrastructure
{
    public interface IDatabaseProvider
    {
        IDbConnection CreateConnection();
        IDbExpressionTranslator CreateDbExpressionTranslator();
        string CreateParameterName(string name);
    }
}
