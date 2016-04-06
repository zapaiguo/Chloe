using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    /// <summary>
    /// User as T1   (select * from User) as T1
    /// </summary>
    public class DbTableExpression : DbExpression
    {
        DbExpression _body;
        string _alias;

        public DbTableExpression(DbExpression body)
            : this(body, null)
        {
        }

        public DbTableExpression(DbExpression body, string alias)
            : base(DbExpressionType.Table, UtilConstants.TypeOfVoid)
        {
            this._body = body;
            this._alias = alias;
        }

        /// <summary>
        /// User、(select * from User)
        /// </summary>
        public DbExpression Body { get { return this._body; } }
        public string Alias { get { return this._alias; } set { this._alias = value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
