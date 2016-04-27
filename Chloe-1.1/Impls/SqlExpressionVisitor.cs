using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query;
using Chloe.DbExpressions;
using System.Reflection;
using Chloe.Extensions;
using System.Collections;
using Chloe.Utility;
using System.Collections.ObjectModel;

namespace Chloe.Impls
{
    public class SqlExpressionVisitor : DbExpressionVisitorBase
    {
        public const string ParameterPrefix = "@P_";

        DbColumnExpressionVisitor _columnExpressionVisitor = null;
        JoinConditionExpressionVisitor _joinConditionExpressionVisitor = null;

        delegate ISqlState BinaryWithMethodHandler(DbBinaryExpression exp, DbExpressionVisitor<ISqlState> visitor);
        static Dictionary<string, Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState>> MethodHandlers = InitMethodHandlers();
        static Dictionary<string, Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState>> FuncHandlers = InitFuncHandlers();
        static Dictionary<MethodInfo, BinaryWithMethodHandler> BinaryWithMethodHandlers = null;

        static MethodInfo StringConcatMethod_String_String = null;
        static MethodInfo StringConcatMethod_Object_Object = null;

        static MemberInfo MemberInfo_DateTime_Now = null;

        static readonly DbParameterExpression _tempDbParameterExpression = DbExpression.Parameter(1);
        static Dictionary<Type, string> CSharpType_DbType_Mappings = null;

        protected Dictionary<string, object> _parameterStorage = new Dictionary<string, object>();
        protected Dictionary<object, SqlState> _innerParameterStorage = new Dictionary<object, SqlState>();


        public static ReadOnlyCollection<DbExpressionType> SafeDbExpressionTypes = null;

        public static readonly Type TypeOfBoolean = typeof(bool);
        public static readonly Type TypeOfBoolean_Nullable = typeof(bool?);
        public static readonly Type TypeOfDateTime = typeof(DateTime);
        public static readonly Type TypeOfString = typeof(string);

        public static DbConstantExpression StringNullConstantExpression = DbExpression.Constant(null, TypeOfString);

        static SqlExpressionVisitor()
        {
            MemberInfo_DateTime_Now = TypeOfDateTime.GetProperty("Now");

            Type typeOfObject = typeof(object);
            MethodInfo concatMethod_String_String = TypeOfString.GetMethod("Concat", new Type[] { TypeOfString, TypeOfString });
            MethodInfo concatMethod_Object_Object = TypeOfString.GetMethod("Concat", new Type[] { typeOfObject, typeOfObject });
            StringConcatMethod_String_String = concatMethod_String_String;
            StringConcatMethod_Object_Object = concatMethod_Object_Object;

            List<DbExpressionType> list = new List<DbExpressionType>();
            list.Add(DbExpressionType.MemberAccess);
            list.Add(DbExpressionType.ColumnAccess);
            list.Add(DbExpressionType.Constant);
            list.Add(DbExpressionType.Parameter);
            list.Add(DbExpressionType.Call);
            SafeDbExpressionTypes = list.AsReadOnly();

            Dictionary<Type, string> cSharpType_DbType_Mappings = new Dictionary<Type, string>(8);
            cSharpType_DbType_Mappings.Add(typeof(string), "NVARCHAR(MAX)");
            cSharpType_DbType_Mappings.Add(typeof(Int16), "SMALLINT");
            cSharpType_DbType_Mappings.Add(typeof(int), "INT");
            cSharpType_DbType_Mappings.Add(typeof(long), "BIGINT");
            cSharpType_DbType_Mappings.Add(typeof(decimal), "DECIMAL");
            cSharpType_DbType_Mappings.Add(typeof(double), "FLOAT");
            cSharpType_DbType_Mappings.Add(typeof(float), "REAL");
            cSharpType_DbType_Mappings.Add(typeof(bool), "BIT");

            cSharpType_DbType_Mappings.Add(typeof(Int16?), "SMALLINT");
            cSharpType_DbType_Mappings.Add(typeof(int?), "INT");
            cSharpType_DbType_Mappings.Add(typeof(long?), "BIGINT");
            cSharpType_DbType_Mappings.Add(typeof(decimal?), "DECIMAL");
            cSharpType_DbType_Mappings.Add(typeof(double?), "FLOAT");
            cSharpType_DbType_Mappings.Add(typeof(float?), "REAL");
            cSharpType_DbType_Mappings.Add(typeof(bool?), "BIT");

            CSharpType_DbType_Mappings = cSharpType_DbType_Mappings;


            BinaryWithMethodHandlers = new Dictionary<MethodInfo, BinaryWithMethodHandler>(2);
            BinaryWithMethodHandlers.Add(concatMethod_String_String, StringConcat);
            BinaryWithMethodHandlers.Add(concatMethod_Object_Object, StringConcat);
        }

        SqlExpressionVisitor()
        {
            this._columnExpressionVisitor = new DbColumnExpressionVisitor(this);
            this._joinConditionExpressionVisitor = new JoinConditionExpressionVisitor(this);
        }
        protected SqlExpressionVisitor(int m)
        {
            this._columnExpressionVisitor = new DbColumnExpressionVisitor(this);
        }


        public override Dictionary<string, object> ParameterStorage { get { return this._parameterStorage; } }

        public static SqlExpressionVisitor CreateInstance()
        {
            return new SqlExpressionVisitor();
        }

        public override ISqlState Visit(DbEqualExpression exp)
        {
            SqlState state = null;
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = DbExpressionExtensions.ParseDbExpression(left);
            right = DbExpressionExtensions.ParseDbExpression(right);

            //明确 left right 其中一边一定为 null
            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(right))
            {
                state = new SqlState(2);
                state.Append(left.Accept(this), " IS NULL");
                return state;
            }

            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(left))
            {
                state = new SqlState(2);
                state.Append(right.Accept(this), " IS NULL");
                return state;
            }

            ISqlState leftState = left.Accept(this);
            ISqlState rightState = right.Accept(this);

            //明确 left right 其中至少一边一定不为 null
            if (DbExpressionExtensions.AffirmExpressionRetValueIsNotNull(right) || DbExpressionExtensions.AffirmExpressionRetValueIsNotNull(left))
            {
                state = new SqlState(3);
                state.Append(leftState, " = ", rightState);
                return state;
            }

            state = new SqlState(15);
            state.Append("(");

            state.Append("(", leftState, " = ", rightState, ")");

            state.Append(" OR ");

            state.Append("(");
            state.Append(leftState, " IS NULL", " AND ", rightState, " IS NULL");
            state.Append(")");

            state.Append(")");

            return state;
        }
        public override ISqlState Visit(DbNotEqualExpression exp)
        {
            var state = new SqlState(3);
            state.Append(exp.Left.Accept(this), " <> ", exp.Right.Accept(this));

            return state;
        }

        public override ISqlState Visit(DbNotExpression exp)
        {
            var state = new SqlState(4);
            state.Append("NOT ", "(", exp.Operand.Accept(this), ")");
            return state;
        }

        public override ISqlState Visit(DbAndExpression exp)
        {
            Stack<DbExpression> oprands = GatherBinaryExpressionOprand(exp);
            return this.ConcatOprands(oprands, " AND ");
        }
        public override ISqlState Visit(DbOrExpression exp)
        {
            Stack<DbExpression> oprands = GatherBinaryExpressionOprand(exp);
            return this.ConcatOprands(oprands, " OR ");
        }

        public override ISqlState Visit(DbConvertExpression exp)
        {
            DbExpression stripedExp = Helper.StripInvalidConvert(exp);

            if (stripedExp.NodeType != DbExpressionType.Convert)
            {
                return stripedExp.Accept(this);
            }

            exp = (DbConvertExpression)stripedExp;

            string dbTypeString;
            if (!CSharpType_DbType_Mappings.TryGetValue(exp.Type, out dbTypeString))
            {
                throw new NotSupportedException(string.Format("{0} 向 {1} 类型转换", exp.Operand.Type.Name, exp.Type.Name));
            }

            SqlState state = BuildCastState(EnsureDbExpressionReturnCSharpBoolean(exp.Operand).Accept(this), dbTypeString);

            return state;
        }
        // +
        public override ISqlState Visit(DbAddExpression exp)
        {
            MethodInfo method = exp.Method;
            if (method != null)
            {
                BinaryWithMethodHandler handler;
                if (BinaryWithMethodHandlers.TryGetValue(method, out handler))
                {
                    return handler(exp, this);
                }

                throw new NotSupportedException(string.Format("{0}.{1}", method.DeclaringType.FullName, exp.Method.Name));
            }

            Stack<DbExpression> oprands = GatherBinaryExpressionOprand(exp);
            return this.ConcatOprands(oprands, " + ");
        }
        // -
        public override ISqlState Visit(DbSubtractExpression exp)
        {
            Stack<DbExpression> oprands = GatherBinaryExpressionOprand(exp);
            return this.ConcatOprands(oprands, " - ");
        }
        // *
        public override ISqlState Visit(DbMultiplyExpression exp)
        {
            Stack<DbExpression> oprands = GatherBinaryExpressionOprand(exp);
            return this.ConcatOprands(oprands, " * ");
        }
        // /
        public override ISqlState Visit(DbDivideExpression exp)
        {
            Stack<DbExpression> oprands = GatherBinaryExpressionOprand(exp);
            return this.ConcatOprands(oprands, " / ");
        }
        // <
        public override ISqlState Visit(DbLessThanExpression exp)
        {
            var state = new SqlState(3);
            state.Append(exp.Left.Accept(this), " < ", exp.Right.Accept(this));

            return state;
        }
        // <=
        public override ISqlState Visit(DbLessThanOrEqualExpression exp)
        {
            var state = new SqlState(3);
            state.Append(exp.Left.Accept(this), " <= ", exp.Right.Accept(this));

            return state;
        }
        // >
        public override ISqlState Visit(DbGreaterThanExpression exp)
        {
            var state = new SqlState(3);
            state.Append(exp.Left.Accept(this), " > ", exp.Right.Accept(this));

            return state;
        }
        // >=
        public override ISqlState Visit(DbGreaterThanOrEqualExpression exp)
        {
            var state = new SqlState(3);
            state.Append(exp.Left.Accept(this), " >= ", exp.Right.Accept(this));

            return state;
        }

        public override ISqlState Visit(DbConstantExpression exp)
        {
            SqlState state = null;

            if (exp.Value == null)
            {
                state = new SqlState(1);
                state.Append("NULL");
                return state;
            }

            var objType = exp.Value.GetType();
            if (objType == typeof(bool))
            {
                state = new SqlState(1);
                state.Append(((bool)exp.Value) ? "CAST(1 AS BIT)" : "CAST(0 AS BIT)");
            }
            else if (objType == typeof(string))
            {
                state = new SqlState(3);
                state.Append("N'", exp.Value, "'");
            }
            else if (objType.IsEnum)
            {
                state = new SqlState(1);
                state.Append(((int)exp.Value).ToString());
            }
            else
            {
                state = new SqlState(1);
                state.Append(exp.Value);
            }

            return state;
        }

        // then 部分必须返回 C# type，所以得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
        public override ISqlState Visit(DbCaseWhenExpression exp)
        {
            SqlState state = new SqlState(4 + exp.WhenThenExps.Count * 4);
            state.Append("CASE");
            foreach (var item in exp.WhenThenExps)
            {
                // then 部分得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
                state.Append(" WHEN ", item.When.Accept(this), " THEN ", EnsureDbExpressionReturnCSharpBoolean(item.Then).Accept(this));
            }
            state.Append(" ELSE ", EnsureDbExpressionReturnCSharpBoolean(exp.Else).Accept(this), " END");

            return state;
        }

        public override ISqlState Visit(DbTableExpression exp)
        {
            var state = new SqlState(1);
            state.Append(QuoteName(exp.Table.Name));
            return state;
        }
        public override ISqlState Visit(DbTableSegmentExpression exp)
        {
            var state = new SqlState(3);
            ISqlState bodyState = exp.Body.Accept(this);
            state.Append(bodyState, " AS ", QuoteName(exp.Alias));
            return state;
        }

        public override ISqlState Visit(DbColumnAccessExpression exp)
        {
            var state = new SqlState(3);
            state.Append(QuoteName(exp.Table.Name), ".", QuoteName(exp.Column.Name));
            return state;
        }
        public override ISqlState Visit(DbColumnSegmentExpression exp)
        {
            var state = new SqlState();
            ISqlState bodyState = exp.Body.Accept(this);
            state.Append(bodyState, " AS ", QuoteName(exp.Alias));
            return state;
        }

        public override ISqlState Visit(DbMemberExpression exp)
        {
            SqlState state = null;

            if (IsDateTimeNowAccess(exp))
            {
                // DateTime.Now
                state = new SqlState(1);
                state.Append("GETDATE()");
                return state;
            }

            DbParameterExpression newExp;
            if (exp.TryParseToParameterExpression(out newExp))
            {
                return newExp.Accept(this);
            }

            MemberInfo member = exp.Member;
            if (member.Name == "Length" && member.DeclaringType == typeof(string))
            {
                state = new SqlState(3);
                state.Append("LEN(", exp.Expression.Accept(this), ")");
                return state;
            }
            else if (member.Name == "Value" && Nullable.GetUnderlyingType(exp.Expression.Type) != null)
            {
                return exp.Expression.Accept(this);
            }

            throw new NotSupportedException(member.Name);
        }
        public override ISqlState Visit(DbParameterExpression exp)
        {
            SqlState state;

            if (!this._innerParameterStorage.TryGetValue(exp.Value, out state))
            {
                state = new SqlState(1);
                string paramName = ParameterPrefix + this._innerParameterStorage.Count.ToString();
                state.Append(paramName);

                this._innerParameterStorage.Add(exp.Value, state);
                this._parameterStorage.Add(paramName, exp.Value);
            }

            return state;
        }

        public override ISqlState Visit(DbSubQueryExpression exp)
        {
            ISqlState state = exp.SqlQuery.Accept(this);
            return BracketState(state);
        }
        public override ISqlState Visit(DbSqlQueryExpression exp)
        {
            if (exp.TakeCount != null && exp.SkipCount != null)
            {
                //构建分页查询
                return this.BuildLimitSqlState(exp);
            }
            else if (exp.TakeCount != null)
            {
                //构建 top 查询
                return this.BuildTakeSqlState(exp);
            }
            else if (exp.SkipCount != null)
            {
                //构建 skip 查询
                return this.BuildSkipSqlState(exp);
            }
            else
            {
                //构建常规的查询
                return this.BuildGeneralSqlState(exp);
            }

            throw new NotImplementedException();
        }

        public override ISqlState Visit(DbMethodCallExpression exp)
        {
            Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState> methodHandler;
            if (!MethodHandlers.TryGetValue(exp.Method.Name, out methodHandler))
            {
                throw new NotSupportedException(exp.Method.Name);
            }
            return methodHandler(exp, this);
        }

        public override ISqlState Visit(DbFromTableExpression exp)
        {
            SqlState state = new SqlState(2);
            state.Append(exp.Table.Accept(this));
            state.Append(this.VisitDbJoinTableExpressions(exp.JoinTables));
            return state;
        }

        public override ISqlState Visit(DbJoinTableExpression exp)
        {
            DbJoinTableExpression joinTablePart = exp;
            string joinString = null;
            if (joinTablePart.JoinType == JoinType.LeftJoin)
            {
                joinString = " LEFT JOIN ";
            }
            else if (joinTablePart.JoinType == JoinType.InnerJoin)
            {
                joinString = " INNER JOIN ";
            }
            else if (joinTablePart.JoinType == JoinType.RightJoin)
            {
                joinString = " RIGHT JOIN ";
            }
            else if (joinTablePart.JoinType == JoinType.FullJoin)
            {
                joinString = " FULL JOIN ";
            }
            else
                throw new NotSupportedException("JoinType: " + joinTablePart.JoinType);

            SqlState state = new SqlState(5);
            state.Append(joinString, joinTablePart.Table.Accept(this), " ON ", joinTablePart.Condition.Accept(this._joinConditionExpressionVisitor));
            state.Append(this.VisitDbJoinTableExpressions(joinTablePart.JoinTables));

            return state;
        }

        public override ISqlState Visit(DbOrderSegmentExpression exp)
        {
            SqlState state = new SqlState(2);

            if (exp.OrderType == OrderType.Asc)
                state.Append(exp.DbExpression.Accept(this), " ASC");
            else if (exp.OrderType == OrderType.Desc)
                state.Append(exp.DbExpression.Accept(this), " DESC");
            else
                throw new NotImplementedException("OrderType: " + exp.OrderType);

            return state;
        }

        public override ISqlState Visit(DbFunctionExpression exp)
        {
            Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState> funcHandler;
            if (!FuncHandlers.TryGetValue(exp.Method.Name, out funcHandler))
            {
                throw new NotSupportedException(exp.Method.Name);
            }
            return funcHandler(exp, this);
        }
        public override ISqlState Visit(DbUpdateExpression exp)
        {
            SqlState state = new SqlState();
            state.Append("UPDATE ", QuoteName(exp.Table.Name), " SET ");

            bool first = true;
            foreach (var item in exp.UpdateColumns)
            {
                if (first)
                    first = false;
                else
                    state.Append(",");

                state.Append(QuoteName(item.Key.Name), " = ", item.Value.Accept(this));
            }

            state.Append(BuildWhereState(exp.Condition));

            return state;
        }
        public override ISqlState Visit(DbDeleteExpression exp)
        {
            SqlState state = new SqlState(3);
            state.Append("DELETE ", QuoteName(exp.Table.Name), BuildWhereState(exp.Condition));
            return state;
        }


        ISqlState VisitDbJoinTableExpressions(List<DbJoinTableExpression> tables)
        {
            SqlState state = new SqlState(tables.Count);
            foreach (var table in tables)
            {
                state.Append(table.Accept(this));
            }

            return state;
        }
        ISqlState BuildGeneralSqlState(DbSqlQueryExpression exp)
        {
            SqlState retState = null;

            SqlState columnsState = new SqlState();
            List<DbColumnSegmentExpression> columns = exp.Columns;

            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegmentExpression column = columns[i];
                if (i > 0)
                    columnsState.Append(",");

                columnsState.Append(column.Accept(this._columnExpressionVisitor));
            }

            ISqlState fromTableState = exp.Table.Accept(this);

            SqlState sqlState = new SqlState();
            sqlState.Append("SELECT ", columnsState, " FROM ", fromTableState);

            SqlState whereState = this.BuildWhereState(exp.Condition);
            sqlState.Append(whereState);

            if (exp.GroupSegments.Count > 0)
            {
                SqlState groupPartState = this.BuildGroupState(exp);
                sqlState.Append(groupPartState);
            }

            SqlState orderState = this.BuildOrderState(exp.OrderSegments);
            sqlState.Append(orderState);

            retState = sqlState;
            return retState;
        }
        ISqlState BuildLimitSqlState(DbSqlQueryExpression exp)
        {
            SqlState retState = null;

            SqlState columnsState = new SqlState();
            List<DbColumnSegmentExpression> columns = exp.Columns;
            List<ISqlState> columnStates = new List<ISqlState>(columns.Count);
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegmentExpression column = columns[i];
                if (i > 0)
                    columnsState.Append(",");

                ISqlState columnState = QuoteName(column.Alias);
                columnsState.Append(column.Body.Accept(this._columnExpressionVisitor), " AS ", columnState);
                columnStates.Add(columnState);
            }

            List<DbOrderSegmentExpression> orderParts = exp.OrderSegments;
            if (orderParts.Count == 0)
            {
                DbOrderSegmentExpression orderPart = new DbOrderSegmentExpression(_tempDbParameterExpression, OrderType.Asc);
                orderParts = new List<DbOrderSegmentExpression>();
                orderParts.Add(orderPart);
            }

            ISqlState fromTableState = exp.Table.Accept(this);

            SqlState orderState = this.ConcatOrderSegments(orderParts);

            string row_numberName = CreateRowNumberName(columns);

            SqlState row_numberNameState = QuoteName(row_numberName);
            SqlState row_numberState = new SqlState();
            row_numberState.Append("SELECT ", columnsState, ",ROW_NUMBER() OVER(ORDER BY ", orderState, ") AS ", row_numberNameState, " FROM ", fromTableState);

            SqlState whereState = this.BuildWhereState(exp.Condition);
            row_numberState.Append(whereState);

            if (exp.GroupSegments.Count > 0)
            {
                SqlState groupPartState = this.BuildGroupState(exp);
                row_numberState.Append(groupPartState);
            }

            string tableAlias = "T";
            SqlState tableState_tableAlias = QuoteName(tableAlias);

            SqlState selectedColumnState_TakeSql = new SqlState();
            for (int i = 0; i < columnStates.Count; i++)
            {
                ISqlState columnState = columnStates[i];

                if (i > 0)
                    selectedColumnState_TakeSql.Append(",");

                selectedColumnState_TakeSql.Append(tableState_tableAlias, ".", columnState, " AS ", columnState);
            }

            SqlState sqlState = new SqlState();
            sqlState.Append("SELECT TOP (", exp.TakeCount.ToString(), ") ", selectedColumnState_TakeSql, " FROM ", BracketState(row_numberState), " AS ", tableState_tableAlias, " WHERE ", tableState_tableAlias, ".", row_numberNameState, " > ", exp.SkipCount.ToString());

            retState = sqlState;
            return retState;
        }
        ISqlState BuildTakeSqlState(DbSqlQueryExpression exp)
        {
            SqlState retState = null;

            SqlState columnsState = new SqlState();
            List<DbColumnSegmentExpression> columns = exp.Columns;

            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegmentExpression column = columns[i];
                if (i > 0)
                    columnsState.Append(",");

                columnsState.Append(column.Accept(this._columnExpressionVisitor));
            }

            List<DbOrderSegmentExpression> orderParts = exp.OrderSegments;

            ISqlState fromTableState = exp.Table.Accept(this);

            SqlState sqlState = new SqlState();
            sqlState.Append("SELECT TOP (", exp.TakeCount.Value.ToString(), ") ", columnsState, " FROM ", fromTableState);

            SqlState whereState = this.BuildWhereState(exp.Condition);
            sqlState.Append(whereState);

            if (exp.GroupSegments.Count > 0)
            {
                SqlState groupPartState = this.BuildGroupState(exp);
                sqlState.Append(groupPartState);
            }

            SqlState orderState = this.BuildOrderState(orderParts);
            sqlState.Append(orderState);

            retState = sqlState;
            return retState;
        }
        ISqlState BuildSkipSqlState(DbSqlQueryExpression exp)
        {
            SqlState retState = null;

            SqlState columnsState = new SqlState();
            List<DbColumnSegmentExpression> columns = exp.Columns;
            List<SqlState> columnStates = new List<SqlState>(columns.Count);
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegmentExpression column = columns[i];
                if (i > 0)
                    columnsState.Append(",");

                SqlState columnState = QuoteName(column.Alias);
                columnsState.Append(column.Body.Accept(this._columnExpressionVisitor), " AS ", columnState);
                columnStates.Add(columnState);
            }

            List<DbOrderSegmentExpression> orderParts = exp.OrderSegments;
            if (orderParts.Count == 0)
            {
                DbOrderSegmentExpression orderPart = new DbOrderSegmentExpression(_tempDbParameterExpression, OrderType.Asc);
                orderParts = new List<DbOrderSegmentExpression>();
                orderParts.Add(orderPart);
            }

            ISqlState fromTableState = exp.Table.Accept(this);
            SqlState orderState = this.ConcatOrderSegments(orderParts);

            string row_numberName = CreateRowNumberName(columns);

            SqlState row_numberNameState = QuoteName(row_numberName);
            SqlState row_numberState = new SqlState();
            row_numberState.Append("SELECT ", columnsState, ",ROW_NUMBER() OVER(ORDER BY ", orderState, ") AS ", row_numberNameState, " FROM ", fromTableState);

            SqlState whereState = this.BuildWhereState(exp.Condition);
            row_numberState.Append(whereState);

            if (exp.GroupSegments.Count > 0)
            {
                SqlState groupPartState = this.BuildGroupState(exp);
                row_numberState.Append(groupPartState);
            }

            string tableAlias = "T";
            SqlState tableState_tableAlias = QuoteName(tableAlias);

            SqlState selectedColumnState_TakeSql = new SqlState();
            for (int i = 0; i < columnStates.Count; i++)
            {
                SqlState columnState = columnStates[i];

                if (i > 0)
                    selectedColumnState_TakeSql.Append(",");

                selectedColumnState_TakeSql.Append(tableState_tableAlias, ".", columnState, " AS ", columnState);
            }

            SqlState sqlState = new SqlState();
            sqlState.Append("SELECT ", selectedColumnState_TakeSql, " FROM ", BracketState(row_numberState), " AS ", tableState_tableAlias, " WHERE ", tableState_tableAlias, ".", row_numberNameState, " > ", exp.SkipCount.ToString());

            retState = sqlState;
            return retState;
        }
        SqlState BuildWhereState(DbExpression whereExpression)
        {
            SqlState whereState = null;
            if (whereExpression != null)
            {
                whereState = new SqlState(2);
                whereState.Append(" WHERE ", whereExpression.Accept(this));
            }

            return whereState;
        }
        SqlState BuildOrderState(List<DbOrderSegmentExpression> orderSegments)
        {
            SqlState orderState = null;

            if (orderSegments.Count > 0)
            {
                orderState = new SqlState(2);
                orderState.Append(" ORDER BY ", this.ConcatOrderSegments(orderSegments));
            }

            return orderState;
        }
        SqlState ConcatOrderSegments(List<DbOrderSegmentExpression> orderSegments)
        {
            SqlState state = new SqlState(orderSegments.Count + 1);

            for (int i = 0; i < orderSegments.Count; i++)
            {
                if (i > 0)
                {
                    state.Append(",");
                }

                state.Append(orderSegments[i].Accept(this));
            }

            return state;
        }
        SqlState BuildGroupState(DbSqlQueryExpression exp)
        {
            SqlState groupPartState = null;
            var groupSegments = exp.GroupSegments;
            groupPartState = new SqlState(2 + groupSegments.Count + (exp.HavingCondition != null ? 2 : 0));
            groupPartState.Append(" GROUP BY ");

            for (int i = 0; i < groupSegments.Count; i++)
            {
                if (i > 0)
                    groupPartState.Append(",");

                groupPartState.Append(groupSegments[i].Accept(this));
            }

            if (exp.HavingCondition != null)
            {
                groupPartState.Append(" HAVING ");
                groupPartState.Append(exp.HavingCondition.Accept(this));
            }

            return groupPartState;
        }

        ISqlState ConcatOprands(Stack<DbExpression> oprands, string connector)
        {
            SqlState state = new SqlState(2 + oprands.Count + oprands.Count - 1);
            state.Append("(");

            bool first = true;
            foreach (DbExpression oprand in oprands)
            {
                if (first)
                    first = false;
                else
                    state.Append(connector);

                state.Append(oprand.Accept(this));
            }

            state.Append(")");
            return state;
        }

        public static SqlState QuoteName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            SqlState state = new SqlState(3);
            state.Append("[", name, "]");
            return state;
        }
        public static SqlState BracketState(ISqlState state)
        {
            var retState = new SqlState(3);
            retState.Append("(", state, ")");
            return retState;
        }

        static SqlState BuildCastState(object castObject, string targetDbTypeString)
        {
            SqlState state = new SqlState(5);
            state.Append("CAST(", castObject, " AS ", targetDbTypeString, ")");
            return state;
        }
        static string CreateRowNumberName(List<DbColumnSegmentExpression> columns)
        {
            int ROW_NUMBER_INDEX = 1;
            string row_numberName = "ROW_NUMBER_0";
            while (columns.Any(a => string.Equals(a.Alias, row_numberName, StringComparison.OrdinalIgnoreCase)))
            {
                row_numberName = "ROW_NUMBER_" + ROW_NUMBER_INDEX.ToString();
                ROW_NUMBER_INDEX++;
            }

            return row_numberName;
        }
        static ISqlState StringConcat(DbBinaryExpression exp, DbExpressionVisitor<ISqlState> visitor)
        {
            MethodInfo method = exp.Method;

            List<DbExpression> oprands = new List<DbExpression>();
            oprands.Add(exp.Right);

            DbExpression left = exp.Left;
            DbAddExpression e = null;
            while ((e = (left as DbAddExpression)) != null && (e.Method == StringConcatMethod_String_String || e.Method == StringConcatMethod_Object_Object))
            {
                oprands.Add(e.Right);
                left = e.Left;
            }

            oprands.Add(left);

            SqlState state = new SqlState(3 + oprands.Count);

            DbExpression whenExp = null;

            state.Append("(");
            for (int i = oprands.Count - 1; i >= 0; i--)
            {
                DbExpression oprand = oprands[i];
                DbExpression opBody = oprand;
                if (opBody.Type != TypeOfString)
                {
                    // 需要 cast type
                    opBody = DbExpression.Convert(TypeOfString, opBody);
                }

                DbExpression equalNullExp = DbExpression.Equal(opBody, StringNullConstantExpression);

                if (whenExp == null)
                    whenExp = equalNullExp;
                else
                    whenExp = DbExpression.And(whenExp, equalNullExp);

                DbExpression thenExp = DbExpression.Constant("", TypeOfString);
                DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(equalNullExp, thenExp);

                List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1);
                whenThenExps.Add(whenThenPair);

                DbExpression elseExp = opBody;

                DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps.AsReadOnly(), elseExp, TypeOfString);

                if (i < oprands.Count - 1)
                    state.Append(" + ");
                state.Append(caseWhenExpression.Accept(visitor));
            }
            state.Append(")");

            SqlState retState = new SqlState(8);
            retState.Append("CASE", " WHEN ", whenExp.Accept(visitor), " THEN ", StringNullConstantExpression.Accept(visitor));
            retState.Append(" ELSE ", state, " END");

            return retState;
        }
        static DbExpression EnsureDbExpressionReturnCSharpBoolean(DbExpression exp)
        {
            if (exp.Type != TypeOfBoolean && exp.Type != TypeOfBoolean_Nullable)
                return exp;

            //DbExpression stripedExp = DbExpressionExtensions.StripConvert(exp);

            if (SafeDbExpressionTypes.Contains(exp.NodeType))
            {
                return exp;
            }

            //将且认为不符合上述条件的都是诸如 a.Id>1,a.Name=="name" 等不能作为 bool 返回值的表达式
            //构建 case when 
            return ConstructReturnCSharpBooleanCaseWhenExpression(exp);
        }
        public static DbCaseWhenExpression ConstructReturnCSharpBooleanCaseWhenExpression(DbExpression exp)
        {
            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(exp, DbExpression.Constant(true, UtilConstants.TypeOfBoolean));
            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1);
            whenThenExps.Add(whenThenPair);
            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps.AsReadOnly(), DbExpression.Constant(false, UtilConstants.TypeOfBoolean), UtilConstants.TypeOfBoolean);

            return caseWhenExpression;
        }
        static Stack<DbExpression> GatherBinaryExpressionOprand(DbBinaryExpression exp)
        {
            DbExpressionType nodeType = exp.NodeType;

            Stack<DbExpression> items = new Stack<DbExpression>();
            items.Push(exp.Right);

            DbExpression left = exp.Left;
            while (left.NodeType == nodeType)
            {
                exp = (DbBinaryExpression)left;
                items.Push(exp.Right);
                left = exp.Left;
            }

            items.Push(left);
            return items;
        }
        static bool IsDateTimeNowAccess(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;
            return exp.Expression == null && member == MemberInfo_DateTime_Now;
        }
        static void EnsureMethodDeclaringType(DbMethodCallExpression exp, Type ensureType)
        {
            MethodInfo method = exp.Method;
            if (method.DeclaringType != ensureType)
                throw new NotSupportedException(exp.Method.Name);
        }


        #region

        static Dictionary<string, Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState>> InitMethodHandlers()
        {
            var methodHandlers = new Dictionary<string, Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState>>();
            methodHandlers.Add("Trim", Method_Trim);
            methodHandlers.Add("TrimStart", Method_TrimStart);
            methodHandlers.Add("TrimEnd", Method_TrimEnd);
            methodHandlers.Add("StartsWith", Method_StartsWith);
            methodHandlers.Add("EndsWith", Method_EndsWith);
            methodHandlers.Add("Contains", Method_Contains);
            methodHandlers.Add("IsNullOrEmpty", Method_IsNullOrEmpty);

            methodHandlers.Add("Count", Method_Count);
            methodHandlers.Add("LongCount", Method_LongCount);
            methodHandlers.Add("Sum", Method_Sum);
            methodHandlers.Add("Max", Method_Max);
            methodHandlers.Add("Min", Method_Min);
            methodHandlers.Add("Average", Method_Average);

            var ret = new Dictionary<string, Func<DbMethodCallExpression, SqlExpressionVisitor, ISqlState>>(methodHandlers.Count, StringComparer.Ordinal);
            foreach (var item in methodHandlers)
            {
                ret.Add(item.Key, item.Value);
            }

            return ret;
        }

        static ISqlState Method_Trim(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            EnsureMethodDeclaringType(exp, TypeOfString);

            MethodInfo method = exp.Method;
            if (method.GetParameters().Length > 0)
                throw new NotSupportedException("一个或多个参数方法: " + exp.Method.Name);

            var state = new SqlState(3);
            state.Append("RTRIM(LTRIM(", exp.Object.Accept(visitor), "))");
            return state;
        }
        static ISqlState Method_TrimStart(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            EnsureMethodDeclaringType(exp, TypeOfString);

            var state = new SqlState(3);
            state.Append("LTRIM(", exp.Object.Accept(visitor), ")");
            return state;
        }
        static ISqlState Method_TrimEnd(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            EnsureMethodDeclaringType(exp, TypeOfString);

            SqlState state = new SqlState(3);
            state.Append("RTRIM(", exp.Object.Accept(visitor), ")");
            return state;
        }
        static ISqlState Method_StartsWith(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            EnsureMethodDeclaringType(exp, TypeOfString);

            if (exp.Arguments.Count > 0)
                throw new NotSupportedException("一个或多个参数方法: " + exp.Method.Name);

            SqlState state = new SqlState(4);
            state.Append(exp.Object.Accept(visitor), " LIKE ", exp.Arguments.First().Accept(visitor), " + '%'");
            return state;
        }
        static ISqlState Method_EndsWith(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            EnsureMethodDeclaringType(exp, TypeOfString);

            if (exp.Arguments.Count > 0)
                throw new NotSupportedException("一个或多个参数方法: " + exp.Method.Name);

            SqlState state = new SqlState(3);
            state.Append(exp.Object.Accept(visitor), " LIKE '%' + ", exp.Arguments.First().Accept(visitor));
            return state;
        }
        static ISqlState Method_StringContains(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            EnsureMethodDeclaringType(exp, TypeOfString);

            SqlState state = new SqlState(4);
            state.Append(exp.Object.Accept(visitor), " LIKE '%' + ", exp.Arguments.First().Accept(visitor), " + '%'");
            return state;
        }
        static ISqlState Method_IsNullOrEmpty(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            EnsureMethodDeclaringType(exp, TypeOfString);

            DbExpression e = exp.Arguments.First();
            DbEqualExpression equalNullExpression = DbExpression.Equal(e, DbExpression.Constant(null, e.Type));
            DbEqualExpression equalEmptyExpression = DbExpression.Equal(e, DbExpression.Constant(string.Empty, e.Type));

            DbOrExpression orExpression = DbExpression.Or(equalNullExpression, equalEmptyExpression);

            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(orExpression, DbExpression.Constant(true, TypeOfBoolean));

            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1);
            whenThenExps.Add(whenThenPair);

            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps.AsReadOnly(), DbExpression.Constant(false, TypeOfBoolean), TypeOfBoolean);

            return caseWhenExpression.Accept(visitor);
        }
        static ISqlState Method_Contains(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            MethodInfo method = exp.Method;

            if (method.DeclaringType == TypeOfString)
                return Method_StringContains(exp, visitor);

            List<DbExpression> exps = null;
            IEnumerable values = null;
            DbExpression arg = null;

            if (typeof(IList).IsAssignableFrom(method.DeclaringType))
            {
                DbMemberExpression memberExp = exp.Object as DbMemberExpression;

                if (memberExp == null || !memberExp.CanEvaluate())
                    throw new NotSupportedException(exp.Object.ToString());

                values = memberExp.GetMemberValue() as IEnumerable; //Enumerable
                exps = new List<DbExpression>(((IList)values).Count);
                arg = exp.Arguments.First();
                goto constructInState;
            }
            if (method.IsStatic && method.DeclaringType == typeof(Enumerable) && exp.Arguments.Count == 2)
            {
                DbMemberExpression memberExp = exp.Arguments.First() as DbMemberExpression;

                if (memberExp == null || !memberExp.CanEvaluate())
                    throw new NotSupportedException(exp.Object.ToString());

                values = memberExp.GetMemberValue() as IEnumerable;
                exps = new List<DbExpression>();
                arg = exp.Arguments.Skip(1).First();
                goto constructInState;
            }

            throw new NotSupportedException(exp.Object.ToString());

        constructInState:
            foreach (object value in values)
            {
                if (value == null)
                    exps.Add(DbExpression.Constant(null, typeof(DBNull)));
                else
                    exps.Add(DbExpression.Parameter(value));
            }
            return In(visitor, exps, arg);
        }

        static ISqlState In(DbExpressionVisitor<ISqlState> visitor, List<DbExpression> elementExps, DbExpression arg)
        {
            SqlState state = null;

            if (elementExps.Count == 0)
            {
                state = new SqlState(1);
                return state.Append("1=0");
            }

            state = new SqlState((elementExps.Count == 1 ? 3 : 4) + elementExps.Count);
            state.Append(arg.Accept(visitor));
            state.Append(" IN (");

            var first = true;
            foreach (DbExpression ele in elementExps)
            {
                if (first)
                    first = false;
                else
                    state.Append(",");

                state.Append(ele.Accept(visitor));
            }

            state.Append(")");

            return state;
        }

        static ISqlState Method_Count(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_Count();
        }
        static ISqlState Method_LongCount(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_LongCount();
        }
        static ISqlState Method_Sum(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_Sum(exp.Arguments.First(), visitor);
        }
        static ISqlState Method_Max(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_Max(exp.Arguments.First(), visitor);
        }
        static ISqlState Method_Min(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_Min(exp.Arguments.First(), visitor);
        }
        static ISqlState Method_Average(DbMethodCallExpression exp, SqlExpressionVisitor visitor)
        {
            ISqlState state = Func_Average(exp.Arguments.First(), visitor);

            string dbTypeString;
            if (CSharpType_DbType_Mappings.TryGetValue(exp.Type, out dbTypeString))
            {
                state = BuildCastState(state, dbTypeString);
            }

            return state;
        }

        #endregion

        #region
        static Dictionary<string, Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState>> InitFuncHandlers()
        {
            var funcHandlers = new Dictionary<string, Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState>>();
            funcHandlers.Add("Count", Func_Count);
            funcHandlers.Add("LongCount", Func_LongCount);
            funcHandlers.Add("Sum", Func_Sum);
            funcHandlers.Add("Max", Func_Max);
            funcHandlers.Add("Min", Func_Min);
            funcHandlers.Add("Average", Func_Average);

            var ret = new Dictionary<string, Func<DbFunctionExpression, SqlExpressionVisitor, ISqlState>>(funcHandlers.Count, StringComparer.Ordinal);
            foreach (var item in funcHandlers)
            {
                ret.Add(item.Key, item.Value);
            }

            return ret;
        }

        static ISqlState Func_Count(DbFunctionExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_Count();
        }
        static ISqlState Func_LongCount(DbFunctionExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_LongCount();
        }
        static ISqlState Func_Sum(DbFunctionExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_Sum(exp.Parameters.First(), visitor);
        }
        static ISqlState Func_Max(DbFunctionExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_Max(exp.Parameters.First(), visitor);
        }
        static ISqlState Func_Min(DbFunctionExpression exp, SqlExpressionVisitor visitor)
        {
            return Func_Min(exp.Parameters.First(), visitor);
        }
        static ISqlState Func_Average(DbFunctionExpression exp, SqlExpressionVisitor visitor)
        {
            ISqlState state = Func_Average(exp.Parameters.First(), visitor);

            string dbTypeString;
            if (CSharpType_DbType_Mappings.TryGetValue(exp.Type, out dbTypeString))
            {
                state = BuildCastState(state, dbTypeString);
            }

            return state;
        }

        #endregion

        #region AggregateFunction
        static ISqlState Func_Count()
        {
            SqlState state = new SqlState(1);
            state.Append("COUNT(1)");
            return state;
        }
        static ISqlState Func_LongCount()
        {
            SqlState state = new SqlState(1);
            state.Append("COUNT_BIG(1)");
            return state;
        }
        static ISqlState Func_Sum(DbExpression exp, SqlExpressionVisitor visitor)
        {
            SqlState state = new SqlState(3);
            state.Append("SUM(", exp.Accept(visitor), ")");
            return state;
        }
        static ISqlState Func_Max(DbExpression exp, SqlExpressionVisitor visitor)
        {
            SqlState state = new SqlState(3);
            state.Append("MAX(", exp.Accept(visitor), ")");
            return state;
        }
        static ISqlState Func_Min(DbExpression exp, SqlExpressionVisitor visitor)
        {
            SqlState state = new SqlState(3);
            state.Append("MIN(", exp.Accept(visitor), ")");
            return state;
        }
        static ISqlState Func_Average(DbExpression exp, SqlExpressionVisitor visitor)
        {
            SqlState state = new SqlState(3);
            state.Append("AVG(", exp.Accept(visitor), ")");
            return state;
        }
        #endregion
    }

}
