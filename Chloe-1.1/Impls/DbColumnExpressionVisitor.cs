using Chloe.Extensions;
using Chloe.DbExpressions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query;

namespace Chloe.Impls
{
    public class DbColumnExpressionVisitor : DbExpressionVisitor<ISqlState>
    {
        SqlExpressionVisitor _visitor = null;

        public DbColumnExpressionVisitor(SqlExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            this._visitor = visitor;
        }

        public override ISqlState Visit(DbEqualExpression exp)
        {
            return this.VisistDbBooleanExpression(exp);
        }
        public override ISqlState Visit(DbNotEqualExpression exp)
        {
            return this.VisistDbBooleanExpression(exp);
        }
        public override ISqlState Visit(DbNotExpression exp)
        {
            return this.VisistDbBooleanExpression(exp);
        }
        public override ISqlState Visit(DbAndExpression exp)
        {
            return this.VisistDbBooleanExpression(exp);
        }

        public override ISqlState Visit(DbOrExpression exp)
        {
            return this.VisistDbBooleanExpression(exp);
        }

        public override ISqlState Visit(DbConvertExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        // +
        public override ISqlState Visit(DbAddExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        // -
        public override ISqlState Visit(DbSubtractExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        // *
        public override ISqlState Visit(DbMultiplyExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        // /
        public override ISqlState Visit(DbDivideExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        // <
        public override ISqlState Visit(DbLessThanExpression exp)
        {
            return this.VisistDbBooleanExpression(exp);
        }
        // <=
        public override ISqlState Visit(DbLessThanOrEqualExpression exp)
        {
            return this.VisistDbBooleanExpression(exp);
        }
        // >
        public override ISqlState Visit(DbGreaterThanExpression exp)
        {
            return this.VisistDbBooleanExpression(exp);
        }
        // >=
        public override ISqlState Visit(DbGreaterThanOrEqualExpression exp)
        {
            return this.VisistDbBooleanExpression(exp);
        }

        public override ISqlState Visit(DbConstantExpression exp)
        {
            return exp.Accept(this._visitor);
        }

        public override ISqlState Visit(DbCaseWhenExpression exp)
        {
            return exp.Accept(this._visitor);
        }

        public override ISqlState Visit(DbTableExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        public override ISqlState Visit(DbDerivedTableExpression exp)
        {
            return exp.Accept(this._visitor);
        }

        public override ISqlState Visit(DbColumnAccessExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        public override ISqlState Visit(DbColumnExpression exp)
        {
            return exp.Accept(this._visitor);
        }

        public override ISqlState Visit(DbMemberExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        public override ISqlState Visit(DbParameterExpression exp)
        {
            return exp.Accept(this._visitor);
        }

        public override ISqlState Visit(DbSubQueryExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        public override ISqlState Visit(DbSqlQueryExpression exp)
        {
            return exp.Accept(this._visitor);
        }

        public override ISqlState Visit(DbMethodCallExpression exp)
        {
            if (exp.Type == UtilConstants.TypeOfBoolean || exp.Type == UtilConstants.TypeOfBoolean_Nullable)
                return this.VisistDbBooleanExpression(exp);
            else
                return exp.Accept(this._visitor);
        }

        public override ISqlState Visit(DbFromTableExpression exp)
        {
            return exp.Accept(this._visitor);
        }

        public override ISqlState Visit(DbJoinTableExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        public override ISqlState Visit(DbOrderSegmentExpression exp)
        {
            return exp.Accept(this._visitor);
        }
        public override ISqlState Visit(DbFunctionExpression exp)
        {
            return exp.Accept(this._visitor);
        }

        ISqlState VisistDbBooleanExpression(DbExpression exp)
        {
            DbCaseWhenExpression caseWhenExpression = SqlExpressionVisitor.ConstructReturnCSharpBooleanCaseWhenExpression(exp);
            ISqlState state = this.Visit(caseWhenExpression);
            return state;
        }

    }
}
