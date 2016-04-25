using System;

namespace Chloe.DbExpressions
{
    /// <summary>
    /// T.Id 列名访问
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Name = {Name}")]
    public class DbColumnExpression : DbExpression
    {
        DbTableSegmentExpression _table;
        string _name;

        public DbColumnExpression(Type type, DbTableSegmentExpression table, string name)
            : base(DbExpressionType.Column, type)
        {
            this._table = table;
            this._name = name;
        }

        public DbTableSegmentExpression Table { get { return this._table; } }

        public string Name { get { return this._name; } }


        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

    }

}
