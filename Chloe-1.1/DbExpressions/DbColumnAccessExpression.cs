using System;

namespace Chloe.DbExpressions
{
    /// <summary>
    /// T.Id 列名访问
    /// </summary>
    public class DbColumnAccessExpression : DbExpression
    {
        DbTableSegmentExpression _table;
        DbColumnExpression _column;

        public DbColumnAccessExpression(Type type, DbTableSegmentExpression table, string columnName)
            : this(table, new DbColumnExpression(type, columnName))
        {
        }
        public DbColumnAccessExpression(DbTableSegmentExpression table, DbColumnExpression column)
            : base(DbExpressionType.ColumnAccess, column.Type)
        {
            this._table = table;
            this._column = column;
        }

        public DbTableSegmentExpression Table { get { return this._table; } }

        public DbColumnExpression Column { get { return this._column; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return this._table.Alias + "." + this._column.Name;
        }
    }

}
