using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbDerivedTableExpression : DbExpression
    {
        string _tableName;
        public DbDerivedTableExpression(string tableName)
            : base(DbExpressionType.DerivedTable, UtilConstants.TypeOfVoid)
        {
            this._tableName = tableName;
        }

        public string TableName { get { return this._tableName; } private set { this._tableName = value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
