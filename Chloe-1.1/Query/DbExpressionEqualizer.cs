using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    internal static class DbExpressionEqualizer
    {
        public static bool Equals(DbExpression exp1, DbExpression exp2)
        {
            if (exp1.NodeType != exp2.NodeType)
                return false;

            switch (exp1.NodeType)
            {
                case DbExpressionType.Column:
                    return Equals((DbColumnExpression)exp1, (DbColumnExpression)exp2);
                case DbExpressionType.TableSegment:
                    return Equals((DbTableSegmentExpression)exp1, (DbTableSegmentExpression)exp2);
                case DbExpressionType.Table:
                    return Equals((DbTableExpression)exp1, (DbTableExpression)exp2);
                case DbExpressionType.Constant:
                    return Equals((DbConstantExpression)exp1, (DbConstantExpression)exp2);
                case DbExpressionType.Convert:
                    return Equals((DbConvertExpression)exp1, (DbConvertExpression)exp2);
                case DbExpressionType.Parameter:
                    return Equals((DbParameterExpression)exp1, (DbParameterExpression)exp2);
                case DbExpressionType.MemberAccess:
                    return Equals((DbMemberExpression)exp1, (DbMemberExpression)exp2);
                case DbExpressionType.Call:
                    return Equals((DbMethodCallExpression)exp1, (DbMethodCallExpression)exp2);
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
                    return Equals((DbBinaryExpression)exp1, (DbBinaryExpression)exp2);
                default:
                    return exp1 == exp2;
            }
        }

        public static bool Equals(DbColumnExpression exp1, DbColumnExpression exp2)
        {
            if (exp1.Name != exp2.Name)
                return false;
            return Equals(exp1.Table, exp2.Table);
        }
        public static bool Equals(DbTableSegmentExpression exp1, DbTableSegmentExpression exp2)
        {
            if (exp1.Alias != exp2.Alias)
                return false;
            return Equals(exp1.Body, exp2.Body);
        }
        public static bool Equals(DbTableExpression exp1, DbTableExpression exp2)
        {
            return exp1.Name == exp2.Name;
        }
        public static bool Equals(DbConstantExpression exp1, DbConstantExpression exp2)
        {
            return exp1.Value == exp2.Value;
        }
        public static bool Equals(DbConvertExpression exp1, DbConvertExpression exp2)
        {
            if (exp1.Method != exp2.Method)
                return false;
            return Equals(exp1.Operand, exp2.Operand);
        }
        public static bool Equals(DbParameterExpression exp1, DbParameterExpression exp2)
        {
            return exp1.Value == exp2.Value;
        }
        public static bool Equals(DbMemberExpression exp1, DbMemberExpression exp2)
        {
            if (exp1.Member != exp2.Member)
                return false;
            return Equals(exp1.Expression, exp2.Expression);
        }
        public static bool Equals(DbMethodCallExpression exp1, DbMethodCallExpression exp2)
        {
            if (exp1.Method != exp2.Method)
                return false;
            if (exp1.Arguments.Count != exp2.Arguments.Count)
                return false;
            if (!Equals(exp1.Object, exp2.Object))
                return false;

            for (int i = 0; i < exp1.Arguments.Count; i++)
            {
                if (!Equals(exp1.Arguments[i], exp2.Arguments[i]))
                    return false;
            }

            return true;
        }

        public static bool Equals(DbBinaryExpression exp1, DbBinaryExpression exp2)
        {
            if (exp1.Method != exp2.Method)
                return false;
            if (!Equals(exp1.Left, exp2.Left))
                return false;
            if (!Equals(exp1.Right, exp2.Right))
                return false;

            return true;
        }

    }
}
