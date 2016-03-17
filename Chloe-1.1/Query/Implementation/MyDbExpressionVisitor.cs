//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Chloe.Query.DbExpressions;
//using Chloe.Extensions;

//namespace Chloe.Query.Implementation
//{
//    public class MyDbExpressionVisitor : DbExpressionVisitor<SqlState>
//    {
//        public override SqlState Visit(DbEqualExpression exp)
//        {
//            SqlState state = null;
//            var left = exp.Left;
//            var right = exp.Right;
//            var leftState = left.Accept(this);
//            var rightState = right.Accept(this);
//            // left right 其中一边为常量null
//            if (left.NodeType == DbExpressionType.Null || right.NodeType == DbExpressionType.Null)
//            {
//                state = new SqlState(3);
//                return state += leftState + " IS" + rightState;
//            }
//            else if (left.NodeType == DbExpressionType.ParameterName || right.NodeType == DbExpressionType.ParameterName || left.NodeType == DbExpressionType.Constant || right.NodeType == DbExpressionType.Constant)
//            {
//                state = new SqlState(3);
//                return state += leftState + " =" + rightState;
//            }

//            state = new SqlState(9);
//            state += "((" + leftState + " =" + rightState + ") OR (" + leftState + " IS NULL AND " + rightState + " IS NULL))";

//            return state;
//        }

//        public override SqlState Visit(DbNotEqualExpression exp)
//        {
//            var state = new SqlState(3);
//            state += exp.Left.Accept(this) + "<>" + exp.Right.Accept(this);

//            return state;
//        }
//        // +
//        public override SqlState Visit(DbAddExpression exp)
//        {
//            var state = new SqlState(5);
//            state += "(" + exp.Left.Accept(this) + "+" + exp.Right.Accept(this) + ")";

//            return state;
//        }
//        // -
//        public override SqlState Visit(DbSubtractExpression exp)
//        {
//            var state = new SqlState(5);
//            state += "(" + exp.Left.Accept(this) + "-" + exp.Right.Accept(this) + ")";

//            return state;
//        }
//        // *
//        public override SqlState Visit(DbMultiplyExpression exp)
//        {
//            var state = new SqlState(5);
//            state += "(" + exp.Left.Accept(this) + "*" + exp.Right.Accept(this) + ")";

//            return state;
//        }
//        // /
//        public override SqlState Visit(DbDivideExpression exp)
//        {
//            var state = new SqlState(5);
//            state += "(" + exp.Left.Accept(this) + "/" + exp.Right.Accept(this) + ")";

//            return state;
//        }
//        // <
//        public override SqlState Visit(DbLessThanExpression exp)
//        {
//            var state = new SqlState(3);
//            state += exp.Left.Accept(this) + "<" + exp.Right.Accept(this);

//            return state;
//        }
//        // <=
//        public override SqlState Visit(DbLessThanOrEqualExpression exp)
//        {
//            var state = new SqlState(3);
//            state += exp.Left.Accept(this) + "<=" + exp.Right.Accept(this);

//            return state;
//        }
//        // >
//        public override SqlState Visit(DbGreaterThanExpression exp)
//        {
//            var state = new SqlState(3);
//            state += exp.Left.Accept(this) + ">" + exp.Right.Accept(this);

//            return state;
//        }
//        // >=
//        public override SqlState Visit(DbGreaterThanOrEqualExpression exp)
//        {
//            var state = new SqlState(3);
//            state += exp.Left.Accept(this) + ">=" + exp.Right.Accept(this);

//            return state;
//        }

//        public override SqlState Visit(DbAndExpression exp)
//        {
//            var state = new SqlState(5);
//            state += "(" + exp.Left.Accept(this) + " AND " + exp.Right.Accept(this) + ")";

//            return state;
//        }

//        public override SqlState Visit(DbOrExpression exp)
//        {
//            var state = new SqlState(5);
//            state += "(" + exp.Left.Accept(this) + " OR " + exp.Right.Accept(this) + ")";

//            return state;
//        }

//        public override SqlState Visit(DbConstantExpression exp)
//        {
//            var state = new SqlState(1);
//            var objType = exp.Value.GetType();
//            if (objType == typeof(bool))
//                state.Append(((bool)exp.Value) ? " 1" : " 0");
//            else if (objType == typeof(string))
//                state += " '" + exp.Value + "'";
//            else
//                state += " " + exp.Value;
//            return state;
//        }

//        public override SqlState Visit(DbNotExpression exp)
//        {
//            var state = new SqlState(3);
//            state += " NOT (" + exp.Operand.Accept(this) + ")";
//            return state;
//        }

//        public override SqlState Visit(DbCaseWhenExpression exp)
//        {
//            var state = new SqlState(4 + exp.WhenThenExps.Count * 4);
//            state.Append(" CASE");
//            foreach (var item in exp.WhenThenExps)
//            {
//                state += " WHEN " + item.Key.Accept(this) + " THEN " + item.Value.Accept(this);
//            }
//            state += " ELSE " + exp.ElseExp.Accept(this) + " END";
//            return state;
//        }

//        public override SqlState Visit(DbColumnAccessExpression exp)
//        {
//            var state = new SqlState();
//            if (!string.IsNullOrEmpty(exp.Table))
//                state += "[" + exp.Table + "].";
//            state += "[" + exp.Column + "]";
//            return state;
//        }

//        public override SqlState Visit(DbMemberExpression exp)
//        {
//            var state = new SqlState();

//            if (exp.Member.Name == "Length")
//            {
//                return state += " LEN(" + exp.Expression.Accept(this) + ")";
//            }
//            else if (exp.Member.Name == "Value" && Nullable.GetUnderlyingType(exp.Member.GetMemberInfoType()) != null)
//            {
//                state.Append(exp.Expression.Accept(this));
//            }

//            return state;
//        }

//        public override SqlState Visit(DbInExpression exp)
//        {
//            var state = new SqlState();

//            if (exp.Elements.Count == 0)
//            {
//                return state.Append("1=0");
//            }

//            state.Append(exp.Item.Accept(this));
//            state.Append(" IN (");

//            var first = true;
//            foreach (var item in exp.Elements)
//            {
//                if (first)
//                {
//                    first = false;
//                }
//                else
//                {
//                    state.Append(",");
//                }

//                state.Append(item.Accept(this));
//            }

//            state.Append(")");

//            return state;
//        }

//        public override SqlState Visit(DbParameterNameExpression exp)
//        {
//            var state = new SqlState(1);
//            state += " " + exp.Name;
//            return state;
//        }

//        public override SqlState Visit(DbNullExpression exp)
//        {
//            var state = new SqlState(1);
//            state.Append(" NULL");
//            return state;
//        }

//        public override SqlState Visit(DbMethodCallExpression exp)
//        {
//            MethodHandler methodHandler;
//            if (!MethodHandlers.TryGetValue(exp.Method.Name, out methodHandler))
//            {
//                throw new NotSupportedException(exp.Method.Name);
//            }
//            return methodHandler(exp, this);
//        }


//        private delegate SqlState MethodHandler(DbMethodCallExpression exp, DbExpressionVisitor<SqlState> visitor);
//        private static Dictionary<string, MethodHandler> MethodHandlers = InitializeMethodHandlers();
//        private static Dictionary<string, MethodHandler> InitializeMethodHandlers()
//        {
//            var methodHandlers = new Dictionary<string, MethodHandler>(7, StringComparer.Ordinal);
//            methodHandlers.Add("Trim", Method_Trim);
//            methodHandlers.Add("TrimStart", Method_TrimStart);
//            methodHandlers.Add("TrimEnd", Method_TrimEnd);
//            methodHandlers.Add("StartsWith", Method_StartsWith);
//            methodHandlers.Add("EndsWith", Method_EndsWith);
//            methodHandlers.Add("Contains", Method_StringContains);
//            methodHandlers.Add("IsNullOrEmpty", Method_IsNullOrEmpty);

//            return methodHandlers;
//        }

//        private static SqlState Method_Trim(DbMethodCallExpression exp, DbExpressionVisitor<SqlState> visitor)
//        {
//            var state = new SqlState(3);
//            state += " RTRIM(LTRIM(" + exp.Object.Accept(visitor) + "))";
//            return state;
//        }
//        private static SqlState Method_TrimStart(DbMethodCallExpression exp, DbExpressionVisitor<SqlState> visitor)
//        {
//            var state = new SqlState(3);
//            state += " LTRIM(" + exp.Object.Accept(visitor) + ")";
//            return state;
//        }
//        private static SqlState Method_TrimEnd(DbMethodCallExpression exp, DbExpressionVisitor<SqlState> visitor)
//        {
//            var state = new SqlState(3);
//            state += " RTRIM(" + exp.Object.Accept(visitor) + ")";
//            return state;
//        }
//        private static SqlState Method_StartsWith(DbMethodCallExpression exp, DbExpressionVisitor<SqlState> visitor)
//        {
//            var state = new SqlState(4);
//            state += exp.Object.Accept(visitor) + " LIKE " + exp.Arguments.First().Accept(visitor) + " + '%'";
//            return state;
//        }
//        private static SqlState Method_EndsWith(DbMethodCallExpression exp, DbExpressionVisitor<SqlState> visitor)
//        {
//            var state = new SqlState(3);
//            state += exp.Object.Accept(visitor) + " LIKE '%' +" + exp.Arguments.First().Accept(visitor);
//            return state;
//        }
//        private static SqlState Method_StringContains(DbMethodCallExpression exp, DbExpressionVisitor<SqlState> visitor)
//        {
//            var state = new SqlState(4);
//            state += exp.Object.Accept(visitor) + " LIKE '%' +" + exp.Arguments.First().Accept(visitor) + " + '%'";
//            return state;
//        }
//        private static SqlState Method_IsNullOrEmpty(DbMethodCallExpression exp, DbExpressionVisitor<SqlState> visitor)
//        {
//            var state = new SqlState(5);
//            var state1 = exp.Arguments.First().Accept(visitor);
//            state += " CASE WHEN " + state1 + " IS NULL OR " + state1 + "='' THEN 1 ELSE 0 END = 1";
//            return state;
//        }

//    }
//}
