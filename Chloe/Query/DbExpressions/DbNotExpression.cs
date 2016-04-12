using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbNotExpression : DbExpression
    {
        private DbExpression _exp;

        public DbNotExpression(DbExpression exp)
            : base(DbExpressionType.Not)
        {
            _exp = exp;
        }

        public DbExpression Operand { get { return _exp; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
