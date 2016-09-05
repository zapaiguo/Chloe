using Chloe.Core;
using Chloe.DbExpressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Oracle
{
    partial class SqlGenerator : DbExpressionVisitor<DbExpression>
    {
        public const string ParameterPrefix = ":P_";
        static readonly object Boxed_1 = 1;
        static readonly object Boxed_0 = 0;

        internal ISqlBuilder _sqlBuilder = new SqlBuilder();
        List<DbParam> _parameters = new List<DbParam>();

        DbValueExpressionVisitor _valueExpressionVisitor = null;

        static readonly Dictionary<string, Action<DbMethodCallExpression, SqlGenerator>> MethodHandlers = InitMethodHandlers();
        static readonly Dictionary<string, Action<DbAggregateExpression, SqlGenerator>> AggregateHandlers = InitAggregateHandlers();
        static readonly Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGenerator>> BinaryWithMethodHandlers = InitBinaryWithMethodHandlers();
        static readonly Dictionary<Type, string> CastTypeMap = null;

        public static readonly ReadOnlyCollection<DbExpressionType> SafeDbExpressionTypes = null;

        static readonly List<string> CacheParameterNames = null;

        static SqlGenerator()
        {
            List<DbExpressionType> list = new List<DbExpressionType>();
            list.Add(DbExpressionType.MemberAccess);
            list.Add(DbExpressionType.ColumnAccess);
            list.Add(DbExpressionType.Constant);
            list.Add(DbExpressionType.Parameter);
            list.Add(DbExpressionType.Convert);
            SafeDbExpressionTypes = list.AsReadOnly();

            Dictionary<Type, string> castTypeMap = new Dictionary<Type, string>();
            //castTypeMap.Add(typeof(string), "NVARCHAR2"); // instead of using to_char(exp) 
            castTypeMap.Add(typeof(byte), "NUMBER");
            castTypeMap.Add(typeof(Int16), "NUMBER");
            castTypeMap.Add(typeof(int), "NUMBER");
            castTypeMap.Add(typeof(long), "NUMBER");
            castTypeMap.Add(typeof(decimal), "NUMBER");
            castTypeMap.Add(typeof(double), "BINARY_DOUBLE");
            castTypeMap.Add(typeof(float), "BINARY_FLOAT");
            castTypeMap.Add(typeof(bool), "NUMBER");
            //castTypeMap.Add(typeof(DateTime), "DATE"); // instead of using to_date(exp) 
            //castTypeMap.Add(typeof(Guid), "BLOB");

            CastTypeMap = Utils.Clone(castTypeMap);

            int cacheParameterNameCount = 2 * 12;
            List<string> cacheParameterNames = new List<string>(cacheParameterNameCount);

            for (int i = 0; i < cacheParameterNameCount; i++)
            {
                string paramName = ParameterPrefix + i.ToString();
                cacheParameterNames.Add(paramName);
            }

            CacheParameterNames = cacheParameterNames;
        }

        public ISqlBuilder SqlBuilder { get { return this._sqlBuilder; } }
        public List<DbParam> Parameters { get { return this._parameters; } }

        DbValueExpressionVisitor ValueExpressionVisitor
        {
            get
            {
                if (this._valueExpressionVisitor == null)
                    this._valueExpressionVisitor = new DbValueExpressionVisitor(this);

                return this._valueExpressionVisitor;
            }
        }

        public static SqlGenerator CreateInstance()
        {
            return new SqlGenerator();
        }

        public override DbExpression Visit(DbEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = DbExpressionExtensions.ParseDbExpression(left);
            right = DbExpressionExtensions.ParseDbExpression(right);

            //明确 left right 其中一边一定为 null
            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(right))
            {
                left.Accept(this);
                this._sqlBuilder.Append(" IS NULL");
                return exp;
            }

            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(left))
            {
                right.Accept(this);
                this._sqlBuilder.Append(" IS NULL");
                return exp;
            }

            left.Accept(this);
            this._sqlBuilder.Append(" = ");
            right.Accept(this);

            return exp;
        }
        public override DbExpression Visit(DbNotEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = DbExpressionExtensions.ParseDbExpression(left);
            right = DbExpressionExtensions.ParseDbExpression(right);

            //明确 left right 其中一边一定为 null
            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(right))
            {
                left.Accept(this);
                this._sqlBuilder.Append(" IS NOT NULL");
                return exp;
            }

            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(left))
            {
                right.Accept(this);
                this._sqlBuilder.Append(" IS NOT NULL");
                return exp;
            }

            left.Accept(this);
            this._sqlBuilder.Append(" <> ");
            right.Accept(this);

            return exp;
        }

        public override DbExpression Visit(DbNotExpression exp)
        {
            this._sqlBuilder.Append("NOT ");
            this._sqlBuilder.Append("(");
            exp.Operand.Accept(this);
            this._sqlBuilder.Append(")");

            return exp;
        }

        public override DbExpression Visit(DbAndExpression exp)
        {
            this._sqlBuilder.Append("BITAND(");
            exp.Left.Accept(this);
            this._sqlBuilder.Append(",");
            exp.Left.Accept(this);
            this._sqlBuilder.Append(")");

            return exp;
        }
        public override DbExpression Visit(DbAndAlsoExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " AND ");

            return exp;
        }
        public override DbExpression Visit(DbOrExpression exp)
        {
            throw new NotSupportedException("'|' operator is not supported.");
        }
        public override DbExpression Visit(DbOrElseExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " OR ");

            return exp;
        }

        // +
        public override DbExpression Visit(DbAddExpression exp)
        {
            MethodInfo method = exp.Method;
            if (method != null)
            {
                Action<DbBinaryExpression, SqlGenerator> handler;
                if (BinaryWithMethodHandlers.TryGetValue(method, out handler))
                {
                    handler(exp, this);
                    return exp;
                }

                throw UtilExceptions.NotSupportedMethod(exp.Method);
            }

            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " + ");

            return exp;
        }
        // -
        public override DbExpression Visit(DbSubtractExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " - ");

            return exp;
        }
        // *
        public override DbExpression Visit(DbMultiplyExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " * ");

            return exp;
        }
        // /
        public override DbExpression Visit(DbDivideExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " / ");

            return exp;
        }
        // <
        public override DbExpression Visit(DbLessThanExpression exp)
        {
            exp.Left.Accept(this);
            this._sqlBuilder.Append(" < ");
            exp.Right.Accept(this);

            return exp;
        }
        // <=
        public override DbExpression Visit(DbLessThanOrEqualExpression exp)
        {
            exp.Left.Accept(this);
            this._sqlBuilder.Append(" <= ");
            exp.Right.Accept(this);

            return exp;
        }
        // >
        public override DbExpression Visit(DbGreaterThanExpression exp)
        {
            exp.Left.Accept(this);
            this._sqlBuilder.Append(" > ");
            exp.Right.Accept(this);

            return exp;
        }
        // >=
        public override DbExpression Visit(DbGreaterThanOrEqualExpression exp)
        {
            exp.Left.Accept(this);
            this._sqlBuilder.Append(" >= ");
            exp.Right.Accept(this);

            return exp;
        }


        public override DbExpression Visit(DbAggregateExpression exp)
        {
            Action<DbAggregateExpression, SqlGenerator> aggregateHandler;
            if (!AggregateHandlers.TryGetValue(exp.Method.Name, out aggregateHandler))
            {
                throw UtilExceptions.NotSupportedMethod(exp.Method);
            }

            aggregateHandler(exp, this);
            return exp;
        }


        public override DbExpression Visit(DbTableExpression exp)
        {
            this.QuoteName(exp.Table.Name);

            return exp;
        }
        public override DbExpression Visit(DbColumnAccessExpression exp)
        {
            this.QuoteName(exp.Table.Name);
            this._sqlBuilder.Append(".");
            this.QuoteName(exp.Column.Name);

            return exp;
        }
        public override DbExpression Visit(DbFromTableExpression exp)
        {
            this.AppendTableSegment(exp.Table);
            this.VisitDbJoinTableExpressions(exp.JoinTables);

            return exp;
        }
        public override DbExpression Visit(DbJoinTableExpression exp)
        {
            DbJoinTableExpression joinTablePart = exp;
            string joinString = null;

            if (joinTablePart.JoinType == JoinType.InnerJoin)
            {
                joinString = " INNER JOIN ";
            }
            else if (joinTablePart.JoinType == JoinType.LeftJoin)
            {
                joinString = " LEFT JOIN ";
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

            this._sqlBuilder.Append(joinString);
            this.AppendTableSegment(joinTablePart.Table);
            this._sqlBuilder.Append(" ON ");
            joinTablePart.Condition.Accept(this);
            this.VisitDbJoinTableExpressions(joinTablePart.JoinTables);

            return exp;
        }


        public override DbExpression Visit(DbSubQueryExpression exp)
        {
            this._sqlBuilder.Append("(");
            exp.SqlQuery.Accept(this);
            this._sqlBuilder.Append(")");

            return exp;
        }
        public override DbExpression Visit(DbSqlQueryExpression exp)
        {
            if (exp.TakeCount != null)
            {
                DbSqlQueryExpression newSqlQuery = CloneWithoutLimitInfo(exp, "TTAKE");

                if (exp.SkipCount == null)
                    AppendLimitCondition(newSqlQuery, exp.TakeCount.Value);
                else
                {
                    AppendLimitCondition(newSqlQuery, exp.TakeCount.Value + exp.SkipCount.Value);
                    newSqlQuery.SkipCount = exp.SkipCount.Value;
                }

                newSqlQuery.Accept(this);
                return exp;
            }
            else if (exp.SkipCount != null)
            {
                DbSqlQueryExpression subSqlQuery = CloneWithoutLimitInfo(exp, "TSKIP");

                string row_numberName = GenRowNumberName(subSqlQuery.ColumnSegments);
                DbColumnSegment row_numberSeg = new DbColumnSegment(OracleSemantics.DbMemberExpression_ROWNUM, row_numberName);
                subSqlQuery.ColumnSegments.Add(row_numberSeg);

                DbTable table = new DbTable("T");
                DbSqlQueryExpression newSqlQuery = WrapSqlQuery(subSqlQuery, table, exp.ColumnSegments);

                DbColumnAccessExpression columnAccessExp = new DbColumnAccessExpression(row_numberSeg.Body.Type, table, row_numberName);
                newSqlQuery.Condition = DbExpression.GreaterThan(columnAccessExp, DbExpression.Constant(exp.SkipCount.Value));

                newSqlQuery.Accept(this);
                return exp;
            }

            this.BuildGeneralSql(exp);
            return exp;
        }
        public override DbExpression Visit(DbInsertExpression exp)
        {
            this._sqlBuilder.Append("INSERT INTO ");
            this.QuoteName(exp.Table.Name);
            this._sqlBuilder.Append("(");

            bool first = true;
            foreach (var item in exp.InsertColumns)
            {
                if (first)
                    first = false;
                else
                {
                    this._sqlBuilder.Append(",");
                }

                this.QuoteName(item.Key.Name);
            }

            this._sqlBuilder.Append(")");

            this._sqlBuilder.Append(" VALUES(");
            first = true;
            foreach (var item in exp.InsertColumns)
            {
                if (first)
                    first = false;
                else
                {
                    this._sqlBuilder.Append(",");
                }

                item.Value.Accept(this.ValueExpressionVisitor);
            }

            this._sqlBuilder.Append(")");

            return exp;
        }
        public override DbExpression Visit(DbUpdateExpression exp)
        {
            this._sqlBuilder.Append("UPDATE ");
            this.QuoteName(exp.Table.Name);
            this._sqlBuilder.Append(" SET ");

            bool first = true;
            foreach (var item in exp.UpdateColumns)
            {
                if (first)
                    first = false;
                else
                    this._sqlBuilder.Append(",");

                this.QuoteName(item.Key.Name);
                this._sqlBuilder.Append("=");
                item.Value.Accept(this.ValueExpressionVisitor);
            }

            this.BuildWhereState(exp.Condition);

            return exp;
        }
        public override DbExpression Visit(DbDeleteExpression exp)
        {
            this._sqlBuilder.Append("DELETE FROM ");
            this.QuoteName(exp.Table.Name);
            this.BuildWhereState(exp.Condition);

            return exp;
        }


        // then 部分必须返回 C# type，所以得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
        public override DbExpression Visit(DbCaseWhenExpression exp)
        {
            this._sqlBuilder.Append("CASE");
            foreach (var whenThen in exp.WhenThenPairs)
            {
                // then 部分得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
                this._sqlBuilder.Append(" WHEN ");
                whenThen.When.Accept(this);
                this._sqlBuilder.Append(" THEN ");
                EnsureDbExpressionReturnCSharpBoolean(whenThen.Then).Accept(this);
            }

            this._sqlBuilder.Append(" ELSE ");
            EnsureDbExpressionReturnCSharpBoolean(exp.Else).Accept(this);
            this._sqlBuilder.Append(" END");

            return exp;
        }
        public override DbExpression Visit(DbConvertExpression exp)
        {
            DbExpression stripedExp = DbExpressionHelper.StripInvalidConvert(exp);

            if (stripedExp.NodeType != DbExpressionType.Convert)
            {
                EnsureDbExpressionReturnCSharpBoolean(stripedExp).Accept(this);
                return exp;
            }

            exp = (DbConvertExpression)stripedExp;

            if (exp.Type == UtilConstants.TypeOfString)
            {
                this._sqlBuilder.Append("TO_CHAR(");
                exp.Operand.Accept(this);
                this._sqlBuilder.Append(")");
                return exp;
            }

            if (exp.Type == UtilConstants.TypeOfDateTime)
            {
                this._sqlBuilder.Append("TO_DATE(");
                exp.Operand.Accept(this);
                this._sqlBuilder.Append(",'yyyy-mm-dd hh24:mi:ss')");
                return exp;
            }

            string dbTypeString;
            if (TryGetCastTargetDbTypeString(exp.Operand.Type, exp.Type, out dbTypeString, false))
            {
                this.BuildCastState(EnsureDbExpressionReturnCSharpBoolean(exp.Operand), dbTypeString);
            }
            else
                EnsureDbExpressionReturnCSharpBoolean(exp.Operand).Accept(this);

            return exp;
        }


        public override DbExpression Visit(DbMethodCallExpression exp)
        {
            Action<DbMethodCallExpression, SqlGenerator> methodHandler;
            if (!MethodHandlers.TryGetValue(exp.Method.Name, out methodHandler))
            {
                throw UtilExceptions.NotSupportedMethod(exp.Method);
            }

            methodHandler(exp, this);
            return exp;
        }
        public override DbExpression Visit(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member == OracleSemantics.PropertyInfo_ROWNUM)
            {
                this._sqlBuilder.Append("ROWNUM");
                return exp;
            }

            if (member.DeclaringType == UtilConstants.TypeOfDateTime)
            {
                if (member == UtilConstants.PropertyInfo_DateTime_Now)
                {
                    this._sqlBuilder.Append("SYSTIMESTAMP");
                    return exp;
                }

                if (member == UtilConstants.PropertyInfo_DateTime_UtcNow)
                {
                    this._sqlBuilder.Append("SYS_EXTRACT_UTC(SYSTIMESTAMP)");
                    return exp;
                }

                if (member == UtilConstants.PropertyInfo_DateTime_Today)
                {
                    this._sqlBuilder.Append("TO_DATE(TO_CHAR(SYSDATE,'yyyy-mm-dd'),'yyyy-mm-dd')");
                    return exp;
                }

                if (member == UtilConstants.PropertyInfo_DateTime_Date)
                {
                    this._sqlBuilder.Append("TO_DATE(TO_CHAR(");
                    exp.Expression.Accept(this);
                    this._sqlBuilder.Append(",'yyyy-mm-dd'),'yyyy-mm-dd')");
                    return exp;
                }

                if (IsDatePart(exp))
                {
                    return exp;
                }
            }


            DbParameterExpression newExp;
            if (DbExpressionExtensions.TryParseToParameterExpression(exp, out newExp))
            {
                return newExp.Accept(this);
            }

            if (member.Name == "Length" && member.DeclaringType == UtilConstants.TypeOfString)
            {
                this._sqlBuilder.Append("LENGTH(");
                exp.Expression.Accept(this);
                this._sqlBuilder.Append(")");

                return exp;
            }
            else if (member.Name == "Value" && Utils.IsNullable(exp.Expression.Type))
            {
                exp.Expression.Accept(this);
                return exp;
            }

            throw new NotSupportedException(string.Format("'{0}.{1}' is not supported.", member.DeclaringType.FullName, member.Name));
        }
        public override DbExpression Visit(DbConstantExpression exp)
        {
            if (exp.Value == null || exp.Value == DBNull.Value)
            {
                this._sqlBuilder.Append("NULL");
                return exp;
            }

            var objType = exp.Value.GetType();
            if (objType == UtilConstants.TypeOfBoolean)
            {
                this._sqlBuilder.Append(((bool)exp.Value) ? "1" : "0");
                return exp;
            }
            else if (objType == UtilConstants.TypeOfString)
            {
                this._sqlBuilder.Append("N'", exp.Value, "'");
                return exp;
            }
            else if (objType.IsEnum)
            {
                this._sqlBuilder.Append(((int)exp.Value).ToString());
                return exp;
            }

            this._sqlBuilder.Append(exp.Value);
            return exp;
        }
        public override DbExpression Visit(DbParameterExpression exp)
        {
            object paramValue = exp.Value;
            Type paramType = exp.Type;

            if (paramType.IsEnum)
            {
                paramType = UtilConstants.TypeOfInt32;
                if (paramValue != null)
                {
                    paramValue = (int)paramValue;
                }
            }
            else if (paramType == UtilConstants.TypeOfBoolean)
            {
                paramType = UtilConstants.TypeOfInt32;
                if (paramValue != null)
                {
                    paramValue = (bool)paramValue ? Boxed_1 : Boxed_0;
                }
            }

            if (paramValue == null)
                paramValue = DBNull.Value;

            DbParam p;
            if (paramValue == DBNull.Value)
            {
                p = this._parameters.Where(a => Utils.AreEqual(a.Value, paramValue) && a.Type == paramType).FirstOrDefault();
            }
            else
                p = this._parameters.Where(a => Utils.AreEqual(a.Value, paramValue)).FirstOrDefault();

            if (p != null)
            {
                this._sqlBuilder.Append(p.Name);
                return exp;
            }

            string paramName = GenParameterName(this._parameters.Count);
            p = DbParam.Create(paramName, paramValue, paramType);

            if (paramValue.GetType() == UtilConstants.TypeOfString)
            {
                if (((string)paramValue).Length <= 4000)
                    p.Size = 4000;
            }

            this._parameters.Add(p);
            this._sqlBuilder.Append(paramName);
            return exp;
        }


        void AppendTableSegment(DbTableSegment seg)
        {
            seg.Body.Accept(this);
            this._sqlBuilder.Append(" ");
            this.QuoteName(seg.Alias);
        }
        void AppendColumnSegment(DbColumnSegment seg)
        {
            seg.Body.Accept(this.ValueExpressionVisitor);
            this._sqlBuilder.Append(" AS ");
            this.QuoteName(seg.Alias);
        }
        void AppendOrdering(DbOrdering ordering)
        {
            if (ordering.OrderType == OrderType.Asc)
            {
                ordering.Expression.Accept(this);
                this._sqlBuilder.Append(" ASC");
                return;
            }
            else if (ordering.OrderType == OrderType.Desc)
            {
                ordering.Expression.Accept(this);
                this._sqlBuilder.Append(" DESC");
                return;
            }

            throw new NotSupportedException("OrderType: " + ordering.OrderType);
        }

        void VisitDbJoinTableExpressions(List<DbJoinTableExpression> tables)
        {
            foreach (var table in tables)
            {
                table.Accept(this);
            }
        }
        void BuildGeneralSql(DbSqlQueryExpression exp)
        {
            if (exp.TakeCount != null || exp.SkipCount != null)
                throw new ArgumentException();

            this._sqlBuilder.Append("SELECT ");

            List<DbColumnSegment> columns = exp.ColumnSegments;
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this._sqlBuilder.Append(",");

                this.AppendColumnSegment(column);
            }

            this._sqlBuilder.Append(" FROM ");
            exp.Table.Accept(this);
            this.BuildWhereState(exp.Condition);
            this.BuildGroupState(exp);
            this.BuildOrderState(exp.Orderings);
        }


        internal void BuildWhereState(DbExpression whereExpression)
        {
            if (whereExpression != null)
            {
                this._sqlBuilder.Append(" WHERE ");
                whereExpression.Accept(this);
            }
        }
        internal void BuildOrderState(List<DbOrdering> orderings)
        {
            if (orderings.Count > 0)
            {
                this._sqlBuilder.Append(" ORDER BY ");
                this.ConcatOrderings(orderings);
            }
        }
        void ConcatOrderings(List<DbOrdering> orderings)
        {
            for (int i = 0; i < orderings.Count; i++)
            {
                if (i > 0)
                {
                    this._sqlBuilder.Append(",");
                }

                this.AppendOrdering(orderings[i]);
            }
        }
        void BuildGroupState(DbSqlQueryExpression exp)
        {
            var groupSegments = exp.GroupSegments;
            if (groupSegments.Count == 0)
                return;

            this._sqlBuilder.Append(" GROUP BY ");
            for (int i = 0; i < groupSegments.Count; i++)
            {
                if (i > 0)
                    this._sqlBuilder.Append(",");

                groupSegments[i].Accept(this);
            }

            if (exp.HavingCondition != null)
            {
                this._sqlBuilder.Append(" HAVING ");
                exp.HavingCondition.Accept(this);
            }
        }

        void ConcatOperands(IEnumerable<DbExpression> operands, string connector)
        {
            this._sqlBuilder.Append("(");

            bool first = true;
            foreach (DbExpression operand in operands)
            {
                if (first)
                    first = false;
                else
                    this._sqlBuilder.Append(connector);

                operand.Accept(this);
            }

            this._sqlBuilder.Append(")");
            return;
        }

        protected virtual void QuoteName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            this._sqlBuilder.Append("\"", name, "\"");
        }

        void BuildCastState(DbExpression castExp, string targetDbTypeString)
        {
            this._sqlBuilder.Append("CAST(");
            castExp.Accept(this);
            this._sqlBuilder.Append(" AS ", targetDbTypeString, ")");
        }

        bool IsDatePart(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member == UtilConstants.PropertyInfo_DateTime_Year)
            {
                //this._sqlBuilder.Append("EXTRACT(YEAR FROM ");
                //exp.Expression.Accept(this);
                //this._sqlBuilder.Append(")");

                DbFunction_DATEPART(this, "yyyy", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Month)
            {
                DbFunction_DATEPART(this, "mm", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Day)
            {
                DbFunction_DATEPART(this, "dd", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Hour)
            {
                DbFunction_DATEPART(this, "hh24", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Minute)
            {
                DbFunction_DATEPART(this, "mi", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Second)
            {
                DbFunction_DATEPART(this, "ss", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Millisecond)
            {
                /* exp.Expression must be TIMESTAMP,otherwise there will be an error occurred. */
                DbFunction_DATEPART(this, "ff3", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_DayOfWeek)
            {
                // CAST(TO_CHAR(SYSDATE,'D') AS NUMBER) - 1
                this._sqlBuilder.Append("(");
                DbFunction_DATEPART(this, "D", exp.Expression);
                this._sqlBuilder.Append(" - 1");
                this._sqlBuilder.Append(")");

                return true;
            }

            return false;
        }



        #region BinaryWithMethodHandlers

        static Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGenerator>> InitBinaryWithMethodHandlers()
        {
            var binaryWithMethodHandlers = new Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGenerator>>();
            binaryWithMethodHandlers.Add(UtilConstants.MethodInfo_String_Concat_String_String, StringConcat);
            binaryWithMethodHandlers.Add(UtilConstants.MethodInfo_String_Concat_Object_Object, StringConcat);

            var ret = Utils.Clone(binaryWithMethodHandlers);
            return ret;
        }

        static void StringConcat(DbBinaryExpression exp, SqlGenerator generator)
        {
            MethodInfo method = exp.Method;

            List<DbExpression> operands = new List<DbExpression>();
            operands.Add(exp.Right);

            DbExpression left = exp.Left;
            DbAddExpression e = null;
            while ((e = (left as DbAddExpression)) != null && (e.Method == UtilConstants.MethodInfo_String_Concat_String_String || e.Method == UtilConstants.MethodInfo_String_Concat_Object_Object))
            {
                operands.Add(e.Right);
                left = e.Left;
            }

            operands.Add(left);

            DbExpression whenExp = null;
            List<DbExpression> operandExps = new List<DbExpression>(operands.Count);
            for (int i = operands.Count - 1; i >= 0; i--)
            {
                DbExpression operand = operands[i];
                DbExpression opBody = operand;
                if (opBody.Type != UtilConstants.TypeOfString)
                {
                    // 需要 cast type
                    opBody = DbExpression.Convert(opBody, UtilConstants.TypeOfString);
                }

                DbExpression equalNullExp = DbExpression.Equal(opBody, UtilConstants.DbConstant_Null_String);

                if (whenExp == null)
                    whenExp = equalNullExp;
                else
                    whenExp = DbExpression.AndAlso(whenExp, equalNullExp);

                DbExpression thenExp = DbConstantExpression.StringEmpty;
                DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(equalNullExp, thenExp);

                List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1);
                whenThenExps.Add(whenThenPair);

                DbExpression elseExp = opBody;

                DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, elseExp, UtilConstants.TypeOfString);

                operandExps.Add(caseWhenExpression);
            }

            generator._sqlBuilder.Append("CASE", " WHEN ");
            whenExp.Accept(generator);
            generator._sqlBuilder.Append(" THEN ");
            DbConstantExpression.Null.Accept(generator);
            generator._sqlBuilder.Append(" ELSE ");

            generator._sqlBuilder.Append("(");

            for (int i = 0; i < operandExps.Count; i++)
            {
                if (i > 0)
                    generator._sqlBuilder.Append(" || ");

                operandExps[i].Accept(generator);
            }

            generator._sqlBuilder.Append(")");

            generator._sqlBuilder.Append(" END");
        }

        #endregion
    }
}
