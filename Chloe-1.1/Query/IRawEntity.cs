using Chloe.Query.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    /// <summary>
    /// 数据源表
    /// </summary>
    public interface IRawEntity
    {
        /// <summary>
        /// 获取表字段，memberExpression 必须是派生至参数，并且是一个表示映射字段的访问
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <returns></returns>
        DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter);
        /// <summary>
        /// Include 导航属性
        /// </summary>
        /// <param name="memberExpressionDeriveParameter"></param>
        IncludeMemberInfo IncludeNavigationMember(MemberExpression memberExpressionDeriveParameter);
    }
}
