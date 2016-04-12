using System.Collections.Generic;

namespace Chloe.DbExpressions
{
    public class DbFromTableExpression : DbExpression
    {
        public DbFromTableExpression(DbTableExpression table)
            : base(DbExpressionType.FromTable)
        {
            this.Table = table;
            this.JoinTables = new List<DbJoinTableExpression>();
        }
        public DbTableExpression Table { get; private set; }

        public List<DbJoinTableExpression> JoinTables { get; private set; }

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
