using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.DbExpressions
{
    [System.Diagnostics.DebuggerDisplay("Name = {Name}")]
    public class DbColumnExpression : DbExpression
    {
        string _name;
        public DbColumnExpression(Type type, string name)
            : base(DbExpressionType.Column, type)
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
