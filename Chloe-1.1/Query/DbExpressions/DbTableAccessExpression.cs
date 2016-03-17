using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbTableAccessExpression : DbExpression
    {
        string _tableName;
        public DbTableAccessExpression()
            : this(null)
        {
        }
        public DbTableAccessExpression(string tableName)
            : base(DbExpressionType.TableAccess, UtilConstants.TypeOfVoid)
        {
            this._tableName = tableName;
        }

        public virtual string TableName { get { return this._tableName; } set { this._tableName = value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

    }
}
