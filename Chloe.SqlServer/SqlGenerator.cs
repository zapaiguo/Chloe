using Chloe.Core;
using Chloe.DbExpressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.SqlServer
{
    class SqlGenerator : DbExpressionVisitor<DbExpression>
    {
        public const string ParameterPrefix = "@P_";

        ISqlBuilder _sqlBuilder = new SqlBuilder();
        List<DbParam> _parameters = new List<DbParam>();

        DbValueExpressionVisitor _valueExpressionVisitor = null;

        static readonly Dictionary<string, Action<DbMethodCallExpression, SqlGenerator>> MethodHandlers = InitMethodHandlers();
        static readonly Dictionary<string, Action<DbFunctionExpression, SqlGenerator>> FuncHandlers = InitFuncHandlers();
        static readonly Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGenerator>> BinaryWithMethodHandlers = InitBinaryWithMethodHandlers();
        static readonly Dictionary<Type, string> CSharpType_DbType_Mappings = null;

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

            cSharpType_DbType_Mappings.Add(typeof(DateTime), "DATETIME");
            cSharpType_DbType_Mappings.Add(typeof(DateTime?), "DATETIME");

            cSharpType_DbType_Mappings.Add(typeof(Guid), "UNIQUEIDENTIFIER");
            cSharpType_DbType_Mappings.Add(typeof(Guid?), "UNIQUEIDENTIFIER");

            CSharpType_DbType_Mappings = cSharpType_DbType_Mappings;

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
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " & ");

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
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " | ");

            return exp;
        }
        public override DbExpression Visit(DbOrElseExpression exp)
        {
            Stack<DbExpression> operands = GatherBinaryExpressionOperand(exp);
            this.ConcatOperands(operands, " OR ");

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

            string dbTypeString;
            if (!CSharpType_DbType_Mappings.TryGetValue(exp.Type, out dbTypeString))
            {
                throw new NotSupportedException(string.Format("不支持将类型 {0} 转换为 {1}", exp.Operand.Type.Name, exp.Type.Name));
            }

            this.BuildCastState(EnsureDbExpressionReturnCSharpBoolean(exp.Operand), dbTypeString);

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

                throw new NotSupportedException(string.Format("{0}.{1}", method.DeclaringType.FullName, exp.Method.Name));
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
                this._sqlBuilder.Append(((bool)exp.Value) ? "CAST(1 AS BIT)" : "CAST(0 AS BIT)");
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

        // then 部分必须返回 C# type，所以得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
        public override DbExpression Visit(DbCaseWhenExpression exp)
        {
            this._sqlBuilder.Append("CASE");
            foreach (var item in exp.WhenThenExps)
            {
                // then 部分得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
                this._sqlBuilder.Append(" WHEN ");
                item.When.Accept(this);
                this._sqlBuilder.Append(" THEN ");
                EnsureDbExpressionReturnCSharpBoolean(item.Then).Accept(this);
            }

            this._sqlBuilder.Append(" ELSE ");
            EnsureDbExpressionReturnCSharpBoolean(exp.Else).Accept(this);
            this._sqlBuilder.Append(" END");

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

        public override DbExpression Visit(DbMemberExpression exp)
        {
            if (IsDbFunction_DATEDIFF(exp))
            {
                return exp;
            }

            MemberInfo member = exp.Member;

            if (member.DeclaringType == UtilConstants.TypeOfDateTime)
            {
                if (member == UtilConstants.PropertyInfo_DateTime_Now)
                {
                    this._sqlBuilder.Append("GETDATE()");
                    return exp;
                }

                if (member == UtilConstants.PropertyInfo_DateTime_UtcNow)
                {
                    this._sqlBuilder.Append("GETUTCDATE()");
                    return exp;
                }

                if (member == UtilConstants.PropertyInfo_DateTime_Today)
                {
                    BuildCastState("GETDATE()", "DATE");
                    return exp;
                }

                if (member == UtilConstants.PropertyInfo_DateTime_Date)
                {
                    BuildCastState(exp.Expression, "DATE");
                    return exp;
                }

                if (IsDbFunction_DATEPART(exp))
                {
                    return exp;
                }
            }


            DbParameterExpression newExp;
            if (DbExpressionExtensions.TryParseToParameterExpression(exp, out newExp))
            {
                return newExp.Accept(this);
            }

            if (member.Name == "Length" && member.DeclaringType == typeof(string))
            {
                this._sqlBuilder.Append("LEN(");
                exp.Expression.Accept(this);
                this._sqlBuilder.Append(")");

                return exp;
            }
            else if (member.Name == "Value" && Utils.IsNullable(exp.Expression.Type))
            {
                exp.Expression.Accept(this);
                return exp;
            }

            throw new NotSupportedException(member.Name);
        }
        public override DbExpression Visit(DbParameterExpression exp)
        {
            object val = exp.Value;
            if (val == null)
                val = DBNull.Value;

            string paramName;

            DbParam p;
            if (val == DBNull.Value)
            {
                p = this._parameters.Where(a => Utils.IsEqual(a.Value, val) && a.Type == exp.Type).FirstOrDefault();
            }
            else
                p = this._parameters.Where(a => Utils.IsEqual(a.Value, val)).FirstOrDefault();

            if (p != null)
            {
                this._sqlBuilder.Append(p.Name);
                return exp;
            }

            paramName = GenParameterName(this._parameters.Count);
            this._parameters.Add(DbParam.Create(paramName, val, exp.Type));
            this._sqlBuilder.Append(paramName);
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
            if (exp.SkipCount != null)
            {
                this.BuildSkipOrLimitSql(exp);
                return exp;
            }
            else
            {
                //构建常规的查询
                this.BuildGeneralSql(exp);
                return exp;
            }

            throw new NotImplementedException();
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

        public override DbExpression Visit(DbFunctionExpression exp)
        {
            Action<DbFunctionExpression, SqlGenerator> funcHandler;
            if (!FuncHandlers.TryGetValue(exp.Method.Name, out funcHandler))
            {
                throw new NotSupportedException(exp.Method.Name);
            }

            funcHandler(exp, this);
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
            this._sqlBuilder.Append("DELETE ");
            this.QuoteName(exp.Table.Name);
            this.BuildWhereState(exp.Condition);

            return exp;
        }

        void AppendTableSegment(DbTableSegment seq)
        {
            seq.Body.Accept(this);
            this._sqlBuilder.Append(" AS ");
            this.QuoteName(seq.Alias);
        }
        void AppendColumnSegment(DbColumnSegment seq)
        {
            seq.Body.Accept(this.ValueExpressionVisitor);
            this._sqlBuilder.Append(" AS ");
            this.QuoteName(seq.Alias);
        }
        void AppendOrderSegment(DbOrderSegment seq)
        {
            if (seq.OrderType == OrderType.Asc)
            {
                seq.DbExpression.Accept(this);
                this._sqlBuilder.Append(" ASC");
                return;
            }
            else if (seq.OrderType == OrderType.Desc)
            {
                seq.DbExpression.Accept(this);
                this._sqlBuilder.Append(" DESC");
                return;
            }

            throw new NotSupportedException("OrderType: " + seq.OrderType);
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
            this._sqlBuilder.Append("SELECT ");
            if (exp.TakeCount != null)
                this._sqlBuilder.Append("TOP (", exp.TakeCount.ToString(), ") ");

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
            this.BuildOrderState(exp.OrderSegments);
        }
        void BuildSkipOrLimitSql(DbSqlQueryExpression exp)
        {
            this._sqlBuilder.Append("SELECT ");
            if (exp.TakeCount != null)
                this._sqlBuilder.Append("TOP (", exp.TakeCount.ToString(), ") ");

            string tableAlias = "T";

            List<DbColumnSegment> columns = exp.ColumnSegments;
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this._sqlBuilder.Append(",");

                this.QuoteName(tableAlias);
                this._sqlBuilder.Append(".");
                this.QuoteName(column.Alias);
                this._sqlBuilder.Append(" AS ");
                this.QuoteName(column.Alias);
            }

            this._sqlBuilder.Append(" FROM ");
            this._sqlBuilder.Append("(");

            //------------------------//
            this._sqlBuilder.Append("SELECT ");
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this._sqlBuilder.Append(",");

                column.Body.Accept(this.ValueExpressionVisitor);
                this._sqlBuilder.Append(" AS ");
                this.QuoteName(column.Alias);
            }

            List<DbOrderSegment> orderSegs = exp.OrderSegments;
            if (orderSegs.Count == 0)
            {
                DbOrderSegment orderSeg = new DbOrderSegment(UtilConstants.DbParameter_1, OrderType.Asc);
                orderSegs = new List<DbOrderSegment>(1);
                orderSegs.Add(orderSeg);
            }

            string row_numberName = CreateRowNumberName(columns);
            this._sqlBuilder.Append(",ROW_NUMBER() OVER(ORDER BY ");
            this.ConcatOrderSegments(orderSegs);
            this._sqlBuilder.Append(") AS ");
            this.QuoteName(row_numberName);
            this._sqlBuilder.Append(" FROM ");
            exp.Table.Accept(this);
            this.BuildWhereState(exp.Condition);
            this.BuildGroupState(exp);
            //------------------------//

            this._sqlBuilder.Append(")");
            this._sqlBuilder.Append(" AS ");
            this.QuoteName(tableAlias);
            this._sqlBuilder.Append(" WHERE ");
            this.QuoteName(tableAlias);
            this._sqlBuilder.Append(".");
            this.QuoteName(row_numberName);
            this._sqlBuilder.Append(" > ");
            this._sqlBuilder.Append(exp.SkipCount.ToString());
        }


        void BuildWhereState(DbExpression whereExpression)
        {
            if (whereExpression != null)
            {
                this._sqlBuilder.Append(" WHERE ");
                whereExpression.Accept(this);
            }
        }
        void BuildOrderState(List<DbOrderSegment> orderSegments)
        {
            if (orderSegments.Count > 0)
            {
                this._sqlBuilder.Append(" ORDER BY ");
                this.ConcatOrderSegments(orderSegments);
            }
        }
        void ConcatOrderSegments(List<DbOrderSegment> orderSegments)
        {
            for (int i = 0; i < orderSegments.Count; i++)
            {
                if (i > 0)
                {
                    this._sqlBuilder.Append(",");
                }

                this.AppendOrderSegment(orderSegments[i]);
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

        static string GenParameterName(int ordinal)
        {
            if (ordinal < CacheParameterNames.Count)
            {
                return CacheParameterNames[ordinal];
            }

            return ParameterPrefix + ordinal.ToString();
        }
        void QuoteName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            this._sqlBuilder.Append("[", name, "]");
        }

        void BuildCastState(DbExpression castExp, string targetDbTypeString)
        {
            this._sqlBuilder.Append("CAST(");
            castExp.Accept(this);
            this._sqlBuilder.Append(" AS ", targetDbTypeString, ")");
        }
        void BuildCastState(object castObject, string targetDbTypeString)
        {
            this._sqlBuilder.Append("CAST(", castObject, " AS ", targetDbTypeString, ")");
        }
        static string CreateRowNumberName(List<DbColumnSegment> columns)
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

        static DbExpression EnsureDbExpressionReturnCSharpBoolean(DbExpression exp)
        {
            if (exp.Type != UtilConstants.TypeOfBoolean && exp.Type != UtilConstants.TypeOfBoolean_Nullable)
                return exp;

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
            // case when 1>0 then 1 when not (1>0) then 0 else Null end
            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(exp, DbConstantExpression.True);
            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair1 = new DbCaseWhenExpression.WhenThenExpressionPair(DbExpression.Not(exp), DbConstantExpression.False);
            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(2);
            whenThenExps.Add(whenThenPair);
            whenThenExps.Add(whenThenPair1);
            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, DbConstantExpression.Null, UtilConstants.TypeOfBoolean);

            return caseWhenExpression;
        }
        static Stack<DbExpression> GatherBinaryExpressionOperand(DbBinaryExpression exp)
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
        static void EnsureMethodDeclaringType(DbMethodCallExpression exp, Type ensureType)
        {
            if (exp.Method.DeclaringType != ensureType)
                throw UtilExceptions.NotSupportedMethod(exp.Method);
        }
        static void EnsureMethod(DbMethodCallExpression exp, MethodInfo methodInfo)
        {
            if (exp.Method != methodInfo)
                throw UtilExceptions.NotSupportedMethod(exp.Method);
        }


        #region BinaryWithMethodHandlers

        static Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGenerator>> InitBinaryWithMethodHandlers()
        {
            var binaryWithMethodHandlers = new Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGenerator>>();
            binaryWithMethodHandlers.Add(UtilConstants.MethodInfo_String_Concat_String_String, StringConcat);
            binaryWithMethodHandlers.Add(UtilConstants.MethodInfo_String_Concat_Object_Object, StringConcat);

            var ret = new Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGenerator>>(binaryWithMethodHandlers.Count);
            foreach (var item in binaryWithMethodHandlers)
            {
                ret.Add(item.Key, item.Value);
            }

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
                    generator._sqlBuilder.Append(" + ");

                operandExps[i].Accept(generator);
            }

            generator._sqlBuilder.Append(")");

            generator._sqlBuilder.Append(" END");
        }

        #endregion

        #region MethodHandlers

        static Dictionary<string, Action<DbMethodCallExpression, SqlGenerator>> InitMethodHandlers()
        {
            var methodHandlers = new Dictionary<string, Action<DbMethodCallExpression, SqlGenerator>>();
            methodHandlers.Add("Trim", Method_Trim);
            methodHandlers.Add("TrimStart", Method_TrimStart);
            methodHandlers.Add("TrimEnd", Method_TrimEnd);
            methodHandlers.Add("StartsWith", Method_StartsWith);
            methodHandlers.Add("EndsWith", Method_EndsWith);
            methodHandlers.Add("ToUpper", Method_String_ToUpper);
            methodHandlers.Add("ToLower", Method_String_ToLower);
            methodHandlers.Add("Substring", Method_String_Substring);
            methodHandlers.Add("IsNullOrEmpty", Method_String_IsNullOrEmpty);

            methodHandlers.Add("Contains", Method_Contains);

            methodHandlers.Add("Count", Method_Count);
            methodHandlers.Add("LongCount", Method_LongCount);
            methodHandlers.Add("Sum", Method_Sum);
            methodHandlers.Add("Max", Method_Max);
            methodHandlers.Add("Min", Method_Min);
            methodHandlers.Add("Average", Method_Average);

            methodHandlers.Add("AddYears", Method_DateTime_AddYears);
            methodHandlers.Add("AddMonths", Method_DateTime_AddMonths);
            methodHandlers.Add("AddDays", Method_DateTime_AddDays);
            methodHandlers.Add("AddHours", Method_DateTime_AddHours);
            methodHandlers.Add("AddMinutes", Method_DateTime_AddMinutes);
            methodHandlers.Add("AddSeconds", Method_DateTime_AddSeconds);
            methodHandlers.Add("AddMilliseconds", Method_DateTime_AddMilliseconds);

            methodHandlers.Add("Parse", Method_Parse);

            methodHandlers.Add("NewGuid", Method_Guid_NewGuid);

            var ret = new Dictionary<string, Action<DbMethodCallExpression, SqlGenerator>>(methodHandlers.Count, StringComparer.Ordinal);
            foreach (var item in methodHandlers)
            {
                ret.Add(item.Key, item.Value);
            }

            return ret;
        }

        static void Method_Trim(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_Trim);

            generator._sqlBuilder.Append("RTRIM(LTRIM(");
            exp.Object.Accept(generator);
            generator._sqlBuilder.Append("))");
        }
        static void Method_TrimStart(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_TrimStart);
            EnsureTrimCharArgumentIsSpaces(exp.Arguments[0]);

            generator._sqlBuilder.Append("LTRIM(");
            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Method_TrimEnd(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_TrimEnd);
            EnsureTrimCharArgumentIsSpaces(exp.Arguments[0]);

            generator._sqlBuilder.Append("RTRIM(");
            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Method_StartsWith(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_StartsWith);

            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(" LIKE ");
            exp.Arguments.First().Accept(generator);
            generator._sqlBuilder.Append(" + '%'");
        }
        static void Method_EndsWith(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_EndsWith);

            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(" LIKE '%' + ");
            exp.Arguments.First().Accept(generator);
        }
        static void Method_String_Contains(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_Contains);

            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(" LIKE '%' + ");
            exp.Arguments.First().Accept(generator);
            generator._sqlBuilder.Append(" + '%'");
        }
        static void Method_String_ToUpper(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_ToUpper);

            generator._sqlBuilder.Append("UPPER(");
            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Method_String_ToLower(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_ToLower);

            generator._sqlBuilder.Append("LOWER(");
            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Method_String_Substring(DbMethodCallExpression exp, SqlGenerator generator)
        {
            generator._sqlBuilder.Append("SUBSTRING(");
            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(",");
            exp.Arguments[0].Accept(generator);
            generator._sqlBuilder.Append(" + 1");
            generator._sqlBuilder.Append(",");
            if (exp.Method == UtilConstants.MethodInfo_String_Substring_Int32)
            {
                var string_LengthExp = DbExpression.MemberAccess(UtilConstants.PropertyInfo_String_Length, exp.Object);
                string_LengthExp.Accept(generator);
            }
            else if (exp.Method == UtilConstants.MethodInfo_String_Substring_Int32_Int32)
            {
                exp.Arguments[1].Accept(generator);
            }
            else
                throw UtilExceptions.NotSupportedMethod(exp.Method);

            generator._sqlBuilder.Append(")");
        }
        static void Method_String_IsNullOrEmpty(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_String_IsNullOrEmpty);

            DbExpression e = exp.Arguments.First();
            DbEqualExpression equalNullExpression = DbExpression.Equal(e, DbExpression.Constant(null, UtilConstants.TypeOfString));
            DbEqualExpression equalEmptyExpression = DbExpression.Equal(e, DbExpression.Constant(string.Empty));

            DbOrElseExpression orElseExpression = DbExpression.OrElse(equalNullExpression, equalEmptyExpression);

            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(orElseExpression, DbConstantExpression.One);

            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1);
            whenThenExps.Add(whenThenPair);

            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, DbConstantExpression.Zero, UtilConstants.TypeOfBoolean);

            var eqExp = DbExpression.Equal(caseWhenExpression, DbConstantExpression.One);
            eqExp.Accept(generator);
        }

        static void Method_Contains(DbMethodCallExpression exp, SqlGenerator generator)
        {
            MethodInfo method = exp.Method;

            if (method.DeclaringType == UtilConstants.TypeOfString)
            {
                Method_String_Contains(exp, generator);
                return;
            }

            List<DbExpression> exps = new List<DbExpression>();
            IEnumerable values = null;
            DbExpression arg = null;

            var declaringType = method.DeclaringType;

            if (typeof(IList).IsAssignableFrom(declaringType) || (declaringType.IsGenericType && typeof(ICollection<>).MakeGenericType(declaringType.GenericTypeArguments).IsAssignableFrom(declaringType)))
            {
                DbMemberExpression memberExp = exp.Object as DbMemberExpression;

                if (memberExp == null || !memberExp.CanEvaluate())
                    throw new NotSupportedException(exp.Object.ToString());

                values = DbExpressionExtensions.GetExpressionValue(memberExp) as IEnumerable; //Enumerable
                arg = exp.Arguments.First();
                goto constructInState;
            }
            if (method.IsStatic && declaringType == typeof(Enumerable) && exp.Arguments.Count == 2)
            {
                DbMemberExpression memberExp = exp.Arguments.First() as DbMemberExpression;

                if (memberExp == null || !memberExp.CanEvaluate())
                    throw new NotSupportedException(exp.Object.ToString());

                values = DbExpressionExtensions.GetExpressionValue(memberExp) as IEnumerable;
                arg = exp.Arguments.Skip(1).First();
                goto constructInState;
            }

            throw new NotSupportedException(exp.Object.ToString());

        constructInState:
            foreach (object value in values)
            {
                if (value == null)
                    exps.Add(DbExpression.Constant(null, arg.Type));
                else
                    exps.Add(DbExpression.Parameter(value));
            }
            In(generator, exps, arg);
        }


        static void In(SqlGenerator generator, List<DbExpression> elementExps, DbExpression arg)
        {
            if (elementExps.Count == 0)
            {
                generator._sqlBuilder.Append("1=0");
                return;
            }

            arg.Accept(generator);
            generator._sqlBuilder.Append(" IN (");

            var first = true;
            foreach (DbExpression ele in elementExps)
            {
                if (first)
                    first = false;
                else
                    generator._sqlBuilder.Append(",");

                ele.Accept(generator);
            }

            generator._sqlBuilder.Append(")");

            return;
        }

        static void Method_Count(DbMethodCallExpression exp, SqlGenerator generator)
        {
            Func_Count(generator);
        }
        static void Method_LongCount(DbMethodCallExpression exp, SqlGenerator generator)
        {
            Func_LongCount(generator);
        }
        static void Method_Sum(DbMethodCallExpression exp, SqlGenerator generator)
        {
            Func_Sum(exp.Arguments.First(), generator);
        }
        static void Method_Max(DbMethodCallExpression exp, SqlGenerator generator)
        {
            Func_Max(exp.Arguments.First(), generator);
        }
        static void Method_Min(DbMethodCallExpression exp, SqlGenerator generator)
        {
            Func_Min(exp.Arguments.First(), generator);
        }
        static void Method_Average(DbMethodCallExpression exp, SqlGenerator generator)
        {
            string dbTypeString;
            if (CSharpType_DbType_Mappings.TryGetValue(exp.Type, out dbTypeString))
            {
                generator._sqlBuilder.Append("CAST(");
                Func_Average(exp.Arguments.First(), generator);
                generator._sqlBuilder.Append(" AS ", dbTypeString, ")");
            }
            else
                Func_Average(exp.Arguments.First(), generator);
        }


        static void Method_DateTime_AddYears(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD("YEAR", exp, generator);
        }
        static void Method_DateTime_AddMonths(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD("MONTH", exp, generator);
        }
        static void Method_DateTime_AddDays(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD("DAY", exp, generator);
        }
        static void Method_DateTime_AddHours(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD("HOUR", exp, generator);
        }
        static void Method_DateTime_AddMinutes(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD("MINUTE", exp, generator);
        }
        static void Method_DateTime_AddSeconds(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD("SECOND", exp, generator);
        }
        static void Method_DateTime_AddMilliseconds(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethodDeclaringType(exp, UtilConstants.TypeOfDateTime);

            DbFunction_DATEADD("MILLISECOND", exp, generator);
        }

        static void Method_Parse(DbMethodCallExpression exp, SqlGenerator generator)
        {
            if (exp.Arguments.Count != 1)
                throw new NotSupportedException();

            DbExpression arg = exp.Arguments[0];
            if (arg.Type != UtilConstants.TypeOfString)
                throw new NotSupportedException();

            Type retType = exp.Method.ReturnType;
            EnsureMethodDeclaringType(exp, retType);

            DbExpression e = DbExpression.Convert(arg, retType);
            if (retType == UtilConstants.TypeOfBoolean)
            {
                e.Accept(generator);
                generator._sqlBuilder.Append(" = ");
                DbConstantExpression.True.Accept(generator);
            }
            else
                e.Accept(generator);
        }

        static void Method_Guid_NewGuid(DbMethodCallExpression exp, SqlGenerator generator)
        {
            EnsureMethod(exp, UtilConstants.MethodInfo_Guid_NewGuid);

            generator._sqlBuilder.Append("NEWID()");
        }

        #endregion

        #region FuncHandlers
        static Dictionary<string, Action<DbFunctionExpression, SqlGenerator>> InitFuncHandlers()
        {
            var funcHandlers = new Dictionary<string, Action<DbFunctionExpression, SqlGenerator>>();
            funcHandlers.Add("Count", Func_Count);
            funcHandlers.Add("LongCount", Func_LongCount);
            funcHandlers.Add("Sum", Func_Sum);
            funcHandlers.Add("Max", Func_Max);
            funcHandlers.Add("Min", Func_Min);
            funcHandlers.Add("Average", Func_Average);

            var ret = new Dictionary<string, Action<DbFunctionExpression, SqlGenerator>>(funcHandlers.Count, StringComparer.Ordinal);
            foreach (var item in funcHandlers)
            {
                ret.Add(item.Key, item.Value);
            }

            return ret;
        }

        static void Func_Count(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_Count(generator);
        }
        static void Func_LongCount(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_LongCount(generator);
        }
        static void Func_Sum(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_Sum(exp.Parameters.First(), generator);
        }
        static void Func_Max(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_Max(exp.Parameters.First(), generator);
        }
        static void Func_Min(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_Min(exp.Parameters.First(), generator);
        }
        static void Func_Average(DbFunctionExpression exp, SqlGenerator generator)
        {
            string dbTypeString;
            if (CSharpType_DbType_Mappings.TryGetValue(exp.Type, out dbTypeString))
            {
                generator._sqlBuilder.Append("CAST(");
                Func_Average(exp.Parameters.First(), generator);
                generator._sqlBuilder.Append(" AS ", dbTypeString, ")");
            }
            else
                Func_Average(exp.Parameters.First(), generator);
        }

        #endregion

        #region AggregateFunction
        static void Func_Count(SqlGenerator generator)
        {
            generator._sqlBuilder.Append("COUNT(1)");
        }
        static void Func_LongCount(SqlGenerator generator)
        {
            generator._sqlBuilder.Append("COUNT_BIG(1)");
        }
        static void Func_Sum(DbExpression exp, SqlGenerator generator)
        {
            generator._sqlBuilder.Append("SUM(");
            exp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Func_Max(DbExpression exp, SqlGenerator generator)
        {
            generator._sqlBuilder.Append("MAX(");
            exp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Func_Min(DbExpression exp, SqlGenerator generator)
        {
            generator._sqlBuilder.Append("MIN(");
            exp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Func_Average(DbExpression exp, SqlGenerator generator)
        {
            generator._sqlBuilder.Append("AVG(");
            exp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        #endregion


        static void DbFunction_DATEADD(string interval, DbMethodCallExpression exp, SqlGenerator generator)
        {
            generator._sqlBuilder.Append("DATEADD(");
            generator._sqlBuilder.Append(interval);
            generator._sqlBuilder.Append(",");
            exp.Arguments[0].Accept(generator);
            generator._sqlBuilder.Append(",");
            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        void DbFunction_DATEPART(string interval, DbExpression exp)
        {
            this._sqlBuilder.Append("DATEPART(");
            this._sqlBuilder.Append(interval);
            this._sqlBuilder.Append(",");
            exp.Accept(this);
            this._sqlBuilder.Append(")");
        }
        void DbFunction_DATEDIFF(string interval, DbExpression startDateTimeExp, DbExpression endDateTimeExp)
        {
            this._sqlBuilder.Append("DATEDIFF(");
            this._sqlBuilder.Append(interval);
            this._sqlBuilder.Append(",");
            startDateTimeExp.Accept(this);
            this._sqlBuilder.Append(",");
            endDateTimeExp.Accept(this);
            this._sqlBuilder.Append(")");
        }

        bool IsDbFunction_DATEPART(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member == UtilConstants.PropertyInfo_DateTime_Year)
            {
                this.DbFunction_DATEPART("YEAR", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Month)
            {
                this.DbFunction_DATEPART("MONTH", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Day)
            {
                this.DbFunction_DATEPART("DAY", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Hour)
            {
                this.DbFunction_DATEPART("HOUR", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Minute)
            {
                this.DbFunction_DATEPART("MINUTE", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Second)
            {
                this.DbFunction_DATEPART("SECOND", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_Millisecond)
            {
                this.DbFunction_DATEPART("MILLISECOND", exp.Expression);
                return true;
            }

            if (member == UtilConstants.PropertyInfo_DateTime_DayOfWeek)
            {
                this._sqlBuilder.Append("(");
                this.DbFunction_DATEPART("WEEKDAY", exp.Expression);
                this._sqlBuilder.Append(" - 1)");

                return true;
            }

            return false;
        }
        bool IsDbFunction_DATEDIFF(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member.DeclaringType == UtilConstants.TypeOfTimeSpan)
            {
                if (exp.Expression.NodeType == DbExpressionType.Call)
                {
                    DbMethodCallExpression dbMethodExp = (DbMethodCallExpression)exp.Expression;
                    if (dbMethodExp.Method == UtilConstants.MethodInfo_DateTime_Subtract_DateTime)
                    {
                        string interval = null;

                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalDays)
                        {
                            interval = "DAY";
                            goto appendDbFunction_DATEDIFF;
                        }

                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalHours)
                        {
                            interval = "HOUR";
                            goto appendDbFunction_DATEDIFF;
                        }

                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalMinutes)
                        {
                            interval = "MINUTE";
                            goto appendDbFunction_DATEDIFF;
                        }

                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalSeconds)
                        {
                            interval = "SECOND";
                            goto appendDbFunction_DATEDIFF;
                        }

                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalMilliseconds)
                        {
                            interval = "MILLISECOND";
                            goto appendDbFunction_DATEDIFF;
                        }

                        return false;

                    appendDbFunction_DATEDIFF:
                        this._sqlBuilder.Append("CAST(");
                        this.DbFunction_DATEDIFF(interval, dbMethodExp.Arguments[0], dbMethodExp.Object);
                        this._sqlBuilder.Append(" AS ");
                        this._sqlBuilder.Append("FLOAT");
                        this._sqlBuilder.Append(")");

                        return true;
                    }
                }
            }

            return false;
        }

        static void EnsureTrimCharArgumentIsSpaces(DbExpression exp)
        {
            var m = exp as DbMemberExpression;
            if (m == null)
                throw new NotSupportedException();

            DbParameterExpression p;
            if (!DbExpressionExtensions.TryParseToParameterExpression(m, out p))
            {
                throw new NotSupportedException();
            }

            var arg = p.Value;

            if (arg == null)
                throw new NotSupportedException();

            var chars = arg as char[];
            if (chars.Length != 1 || chars[0] != ' ')
            {
                throw new NotSupportedException();
            }
        }
    }
}
