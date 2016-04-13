using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    /// <summary>
    /// 用于 querystate 与 querystate 之间传递信息，当一个 querystate 传到下个 querystate 时，用该对象传递源表的信息，仅仅包含选取了哪些字段信息和导航属性，以供 querystate 生成相应的 sql 或 mappingcontext 之类的
    /// 
    /// </summary>
    public class ResultElement
    {
        public ResultElement()
        {
            this.OrderSegments = new List<DbOrderSegmentExpression>();
        }

        public IMappingObjectExpression MappingObjectExpression { get; set; }

        /// <summary>
        /// 表示当前 OrderParts 集合内的排序是否是从上个 query 继承来的
        /// </summary>
        public bool IsFromSubQuery { get; set; }

        public List<DbOrderSegmentExpression> OrderSegments { get; private set; }

        /// <summary>
        /// 如 takequery 了以后，则 table 的 Expression 类似 (select T.Id.. from User as T),Alias 则为新生成的
        /// </summary>
        public DbFromTableExpression FromTable { get; set; }
        public DbExpression Where { get; private set; }

        public void UpdateCondition(DbExpression whereExpression)
        {
            if (this.Where == null)
                this.Where = whereExpression;
            else
                this.Where = new DbAndExpression(this.Where, whereExpression);
        }

        public string GenerateUniqueTableAlias(string prefix = "T")
        {
            if (this.FromTable == null)
                return prefix;

            string alias = prefix;
            int i = 0;
            while (this.FromTable.ExistTableAlias(alias))
            {
                alias = prefix + i.ToString();
                i++;
            }

            return alias;
        }

    }
}
