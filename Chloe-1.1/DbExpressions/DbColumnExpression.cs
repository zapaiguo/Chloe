using System;

namespace Chloe.DbExpressions
{
    /// <summary>
    /// 完整的列  T.Name as Alias
    /// </summary>
    public class DbColumnExpression : DbExpression
    {
        DbExpression _body;
        string _alias;

        public DbColumnExpression(Type type, DbExpression body, string alias)
            : base(DbExpressionType.Column, type)
        {
            this._body = body;
            this._alias = alias;
        }

        /// <summary>
        /// T.Name 部分
        /// </summary>
        public DbExpression Body { get { return this._body; } set { this._body = value; } }
        public string Alias { get { return this._alias; } set { this._alias = value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

    }
}
