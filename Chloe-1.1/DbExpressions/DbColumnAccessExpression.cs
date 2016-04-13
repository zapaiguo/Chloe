using System;

namespace Chloe.DbExpressions
{
    /// <summary>
    /// T.Id 纯粹的列名访问
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("ColumnName = {ColumnName}")]
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

        public DbTableExpression Table { get { return this._table; } }

        public string ColumnName { get { return this._columnName; } }


        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

    }

}
