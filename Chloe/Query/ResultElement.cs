using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Query
{
    public class ResultElement
    {
        public const string DefaultTableAlias = "T";

        public ResultElement()
        {
            this.OrderSegments = new List<DbOrderSegment>();
            this.GroupSegments = new List<DbExpression>();
        }

        public IMappingObjectExpression MappingObjectExpression { get; set; }

        /// <summary>
        /// 表示当前 OrderParts 集合内的排序是否是从上个 query 继承来的
        /// </summary>
        public bool IsOrderSegmentsFromSubQuery { get; set; }

        public List<DbOrderSegment> OrderSegments { get; private set; }
        public List<DbExpression> GroupSegments { get; private set; }

        /// <summary>
        /// 如 takequery 了以后，则 table 的 Expression 类似 (select T.Id.. from User as T),Alias 则为新生成的
        /// </summary>
        public DbFromTableExpression FromTable { get; set; }
        public DbExpression Condition { get; set; }
        public DbExpression HavingCondition { get; set; }

        public void AppendCondition(DbExpression condition)
        {
            if (this.Condition == null)
                this.Condition = condition;
            else
                this.Condition = new DbAndExpression(this.Condition, condition);
        }
        public void AppendHavingCondition(DbExpression condition)
        {
            if (this.HavingCondition == null)
                this.HavingCondition = condition;
            else
                this.HavingCondition = new DbAndExpression(this.HavingCondition, condition);
        }


        public string GenerateUniqueTableAlias(string prefix = DefaultTableAlias)
        {
            if (this.FromTable == null)
                return prefix;

            string alias = prefix;
            int i = 0;
            DbFromTableExpression fromTable = this.FromTable;
            while (ExistTableAlias(fromTable, alias))
            {
                alias = prefix + i.ToString();
                i++;
            }

            return alias;
        }

        static bool ExistTableAlias(DbFromTableExpression fromTable, string alias)
        {
            if (string.Equals(fromTable.Table.Alias, alias, StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (var item in fromTable.JoinTables)
            {
                if (ExistTableAlias(item, alias))
                    return true;
            }

            return false;
        }
        static bool ExistTableAlias(DbJoinTableExpression joinTable, string alias)
        {
            if (string.Equals(joinTable.Table.Alias, alias, StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (var item in joinTable.JoinTables)
            {
                if (ExistTableAlias(item, alias))
                    return true;
            }

            return false;
        }
    }
}
