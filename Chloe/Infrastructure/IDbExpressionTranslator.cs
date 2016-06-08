using Chloe.Core;
using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Infrastructure
{
    public interface IDbExpressionTranslator
    {
        string Translate(DbExpression expression, out List<DbParam> parameters);
    }
}
