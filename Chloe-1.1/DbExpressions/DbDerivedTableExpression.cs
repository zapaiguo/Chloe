using Chloe.Utility;

namespace Chloe.DbExpressions
{
    [System.Diagnostics.DebuggerDisplay("TableName = {TableName}")]
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
