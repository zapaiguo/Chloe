using System;
using System.Collections.Generic;
using System.Reflection;

namespace Chloe.DbExpressions
{
    public class DbMethodCallExpression : DbExpression
    {
        DbExpression _object;
        MethodInfo _method;
        IReadOnlyList<DbExpression> _arguments;
        public DbMethodCallExpression(DbExpression @object, MethodInfo method, IReadOnlyList<DbExpression> arguments)
            : base(DbExpressionType.Call)
        {
            this._object = @object;
            this._method = method;
            this._arguments = arguments;
        }

        public IReadOnlyList<DbExpression> Arguments { get { return this._arguments; } }
        public MethodInfo Method { get { return _method; } }
        public DbExpression Object { get { return _object; } }
        public override Type Type { get { return this.Method.ReturnType; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
