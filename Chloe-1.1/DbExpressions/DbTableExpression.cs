using Chloe.Utility;

namespace Chloe.DbExpressions
{
    [System.Diagnostics.DebuggerDisplay("Name = {Name}")]
    public class DbTableExpression : DbExpression
    {
        string _name;
        public DbTableExpression(string name)
            : base(DbExpressionType.Table, UtilConstants.TypeOfVoid)
        {
            this._name = name;
        }

        public string Name { get { return this._name; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
