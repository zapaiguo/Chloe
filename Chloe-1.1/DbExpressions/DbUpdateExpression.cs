using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.DbExpressions
{
    class DbDeleteExpression : DbExpression
    {
        DbTableExpression _table;
        DbExpression _condition;
        public DbDeleteExpression(DbTableExpression table)
            : this(table, null)
        {
        }
        public DbDeleteExpression(DbTableExpression table, DbExpression condition)
            : base(DbExpressionType.Delete, UtilConstants.TypeOfVoid)
        {
            Utils.CheckNull(table);

            this._table = table;
            this._condition = condition;
        }

        public DbTableExpression Table { get { return this._table; } }
        public DbExpression Condition { get { return this._condition; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }
    }

    class DbUpdateExpression : DbExpression
    {
        DbTableExpression _table;
        DbExpression _condition;
        public DbUpdateExpression(DbTableExpression table)
            : this(table, null)
        {
        }
        public DbUpdateExpression(DbTableExpression table, DbExpression condition)
            : base(DbExpressionType.Update, UtilConstants.TypeOfVoid)
        {
            Utils.CheckNull(table);

            this._table = table;
            this._condition = condition;
            this.UpdateColumns = new Dictionary<DbColumnExpression, DbExpression>();
        }

        public DbTableExpression Table { get { return this._table; } }
        public Dictionary<DbColumnExpression, DbExpression> UpdateColumns { get; private set; }
        public DbExpression Condition { get { return this._condition; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
