using System;
using System.Collections.Generic;

namespace Chloe.DbExpressions
{
    public class DbCaseWhenExpression : DbExpression
    {
        IReadOnlyList<WhenThenExpressionPair> _whenThenExps;
        DbExpression _elseExp;
        public DbCaseWhenExpression(Type type, IReadOnlyList<WhenThenExpressionPair> whenThenExps, DbExpression elseExp)
            : base(DbExpressionType.CaseWhen, type)
        {
            this._whenThenExps = whenThenExps;
            this._elseExp = elseExp;
        }

        public IReadOnlyList<WhenThenExpressionPair> WhenThenExps { get { return this._whenThenExps; } }
        public DbExpression Else { get { return this._elseExp; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public struct WhenThenExpressionPair
        {
            DbExpression _when;
            DbExpression _then;
            public WhenThenExpressionPair(DbExpression when, DbExpression then)
            {
                this._when = when;
                this._then = then;
            }

            public DbExpression When { get { return this._when; } }
            public DbExpression Then { get { return this._then; } }
        }
    }
}
