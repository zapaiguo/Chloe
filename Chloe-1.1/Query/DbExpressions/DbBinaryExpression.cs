using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public abstract class DbBinaryExpression : DbExpression
    {
        DbExpression _left;
        DbExpression _right;
        MethodInfo _method;
        protected DbBinaryExpression(DbExpressionType nodeType, Type type, DbExpression left, DbExpression right, MethodInfo method)
            : base(nodeType, type)
        {
            this._left = left;
            this._right = right;
            this._method = method;
        }

        public DbExpression Left { get { return _left; } }
        public DbExpression Right { get { return _right; } }
        public MethodInfo Method { get { return this._method; } }
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
