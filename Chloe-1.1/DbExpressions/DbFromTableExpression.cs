using System.Collections.Generic;

namespace Chloe.DbExpressions
{
    public class DbFromTableExpression : DbExpression
    {
        DbTableExpression _table;
        List<DbJoinTableExpression> _joinTables;
        public DbFromTableExpression(DbTableExpression table)
            : base(DbExpressionType.FromTable)
        {
            this._table = table;
            this._joinTables = new List<DbJoinTableExpression>();
        }
        public DbTableExpression Table { get { return this._table; } }

        public List<DbJoinTableExpression> JoinTables { get { return this._joinTables; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
