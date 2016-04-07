using Chloe.Query.DbExpressions;
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
        public ResultElement(TablePart tablePart)
        {
            this.TablePart = tablePart;
            this.OrderParts = new List<OrderPart>();
        }

        //public MappingMembers MappingMembers { get; set; }
        public IMappingObjectExpression MappingObjectExpression { get; set; }

        /// <summary>
        /// 表示当前 OrderParts 集合内的排序是否是从子query来的
        /// </summary>
        public bool IsFromSubQuery { get; set; }

        public List<OrderPart> OrderParts { get; private set; }

        /// <summary>
        /// 如 takequery 了以后，则 table 的 Expression 类似 (select T.Id.. from User as T),Alias 则为新生成的
        /// </summary>
        public TablePart TablePart { get; private set; }
        public DbExpression WhereExpression { get; private set; }

        public void UpdateWhereExpression(DbExpression whereExpression)
        {
            if (this.WhereExpression == null)
                this.WhereExpression = whereExpression;
            else if (whereExpression != null)
                this.WhereExpression = new DbAndExpression(this.WhereExpression, whereExpression);
        }
    }


    //public class ResultState
    //{
    //    public ResultState(TablePart tablePart)
    //    {
    //        this.TablePart = tablePart;
    //        this.OrderParts = new List<OrderPart>();
    //    }

    //    public IMappingObjectExpression MappingObjectExpression { get; set; }

    //    public bool IsFromSubQuery { get; set; }

    //    public List<OrderPart> OrderParts { get; private set; }

    //    /// <summary>
    //    /// 如 takequery 了以后，则 table 的 Expression 类似 (select T.Id.. from User as T),Alias 则为新生成的
    //    /// </summary>
    //    public TablePart TablePart { get; private set; }
    //    public DbExpression WhereExpression { get; private set; }

    //    public void UpdateWhereExpression(DbExpression whereExpression)
    //    {
    //        if (this.WhereExpression == null)
    //            this.WhereExpression = whereExpression;
    //        else if (whereExpression != null)
    //            this.WhereExpression = new DbAndExpression(this.WhereExpression, whereExpression);
    //    }

    //}
}
