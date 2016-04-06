using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    /// <summary>
    /// T.Id 纯粹的列名访问
    /// </summary>
    public class DbColumnAccessExpression : DbExpression
    {
        DbTableExpression _table;
        string _columnName;

        public DbColumnAccessExpression(Type type, DbTableExpression table, string columnName)
            : base(DbExpressionType.ColumnAccess, type)
        {
            this._table = table;
            this._columnName = columnName;
        }

        public DbTableExpression Table { get { return this._table; } set { this._table = value; } }

        public string ColumnName { get { return this._columnName; } set { this._columnName = value; } }


        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

    }

}
