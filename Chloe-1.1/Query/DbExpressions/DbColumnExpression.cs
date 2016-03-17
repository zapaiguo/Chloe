using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbColumnExpression : DbExpression
    {
        DbExpression _body;
        string _alias;
        public DbColumnExpression(Type type)
            : this(type, null, null)
        {
        }
        public DbColumnExpression(Type type, DbExpression body, string alias)
            : base(DbExpressionType.Column, type)
        {
            this._body = body;
            this._alias = alias;
        }

        public virtual DbExpression Body { get { return this._body; } set { this._body = value; } }
        public virtual string Alias { get { return this._alias; } set { this._alias = value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

    }
}
