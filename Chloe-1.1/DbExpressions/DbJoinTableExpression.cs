using System.Collections.Generic;

namespace Chloe.DbExpressions
{
    public class DbJoinTableExpression : DbExpression
    {
        JoinType _joinType;
        DbTableExpression _table;
        DbTableExpression _relatedTable;
        DbFromTableExpression _fromTable;
        DbExpression _condition;
        List<DbJoinTableExpression> _joinTables;
        public DbJoinTableExpression(JoinType joinType, DbTableExpression table, DbTableExpression relatedTable, DbFromTableExpression fromTable, DbExpression condition)
            : base(DbExpressionType.JoinTable)
        {
            this._joinType = joinType;
            this._table = table;
            this._relatedTable = relatedTable;
            this._fromTable = fromTable;
            this._condition = condition;
            this._joinTables = new List<DbJoinTableExpression>();
        }

        public DbTableExpression Table { get { return this._table; } }
        public DbTableExpression RelatedTable { get { return this._relatedTable; } }
        public DbFromTableExpression FromTable { get { return this._fromTable; } }
        public JoinType JoinType { get { return this._joinType; } }
        public DbExpression Condition { get { return this._condition; } }
        public List<DbJoinTableExpression> JoinTables { get { return this._joinTables; } }
        public bool ExistTableAlias(string alias)
        {
            if (this.Table.Alias == alias)
                return true;

            foreach (var item in this.JoinTables)
            {
                if (item.ExistTableAlias(alias))
                    return true;
            }

            return false;
        }
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
