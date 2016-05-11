using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Query
{
    static class DbExpressionEqualityComparer
    {
        public static bool ExpressionEquals(DbExpression exp1, DbExpression exp2)
        {
            if (exp1.NodeType != exp2.NodeType)
                return false;

            switch (exp1.NodeType)
            {
                case DbExpressionType.ColumnAccess:
                    return ExpressionEquals((DbColumnAccessExpression)exp1, (DbColumnAccessExpression)exp2);
                case DbExpressionType.TableSegment:
                    return ExpressionEquals((DbTableSegmentExpression)exp1, (DbTableSegmentExpression)exp2);
                case DbExpressionType.Table:
                    return ExpressionEquals((DbTableExpression)exp1, (DbTableExpression)exp2);
                case DbExpressionType.Constant:
                    return ExpressionEquals((DbConstantExpression)exp1, (DbConstantExpression)exp2);
                case DbExpressionType.Convert:
                    return ExpressionEquals((DbConvertExpression)exp1, (DbConvertExpression)exp2);
                case DbExpressionType.Parameter:
                    return ExpressionEquals((DbParameterExpression)exp1, (DbParameterExpression)exp2);
                case DbExpressionType.MemberAccess:
                    return ExpressionEquals((DbMemberExpression)exp1, (DbMemberExpression)exp2);
                case DbExpressionType.Call:
                    return ExpressionEquals((DbMethodCallExpression)exp1, (DbMethodCallExpression)exp2);
                case DbExpressionType.Add:
                case DbExpressionType.Subtract:
                case DbExpressionType.Multiply:
                case DbExpressionType.Divide:
                case DbExpressionType.And:
                case DbExpressionType.Or:
                case DbExpressionType.Equal:
                case DbExpressionType.NotEqual:
                case DbExpressionType.LessThan:
                case DbExpressionType.LessThanOrEqual:
                case DbExpressionType.GreaterThan:
                case DbExpressionType.GreaterThanOrEqual:
                    return ExpressionEquals((DbBinaryExpression)exp1, (DbBinaryExpression)exp2);
                default:
                    return exp1 == exp2;
            }
        }
        public static bool ExpressionEquals(DbColumnAccessExpression exp1, DbColumnAccessExpression exp2)
        {
            if (exp1.Column.Name != exp2.Column.Name)
                return false;
            return exp1.Table.Name == exp2.Table.Name;
        }
        public static bool ExpressionEquals(DbTableSegmentExpression exp1, DbTableSegmentExpression exp2)
        {
            if (exp1.Alias != exp2.Alias)
                return false;
            return ExpressionEquals(exp1.Body, exp2.Body);
        }
        public static bool ExpressionEquals(DbTableExpression exp1, DbTableExpression exp2)
        {
            return exp1.Table.Name == exp2.Table.Name;
        }
        public static bool ExpressionEquals(DbConstantExpression exp1, DbConstantExpression exp2)
        {
            return exp1.Value == exp2.Value;
        }
        public static bool ExpressionEquals(DbConvertExpression exp1, DbConvertExpression exp2)
        {
            if (exp1.Type != exp2.Type)
                return false;
            return ExpressionEquals(exp1.Operand, exp2.Operand);
        }
        public static bool ExpressionEquals(DbParameterExpression exp1, DbParameterExpression exp2)
        {
            return exp1.Value == exp2.Value;
        }
        public static bool ExpressionEquals(DbMemberExpression exp1, DbMemberExpression exp2)
        {
            if (exp1.Member != exp2.Member)
                return false;
            return ExpressionEquals(exp1.Expression, exp2.Expression);
        }
        public static bool ExpressionEquals(DbMethodCallExpression exp1, DbMethodCallExpression exp2)
        {
            if (exp1.Method != exp2.Method)
                return false;
            if (exp1.Arguments.Count != exp2.Arguments.Count)
                return false;
            if (!ExpressionEquals(exp1.Object, exp2.Object))
                return false;

            for (int i = 0; i < exp1.Arguments.Count; i++)
            {
                if (!ExpressionEquals(exp1.Arguments[i], exp2.Arguments[i]))
                    return false;
            }

            return true;
        }

        public static bool ExpressionEquals(DbBinaryExpression exp1, DbBinaryExpression exp2)
        {
            if (exp1.Method != exp2.Method)
                return false;
            if (!ExpressionEquals(exp1.Left, exp2.Left))
                return false;
            if (!ExpressionEquals(exp1.Right, exp2.Right))
                return false;

            return true;
        }

    }
}
