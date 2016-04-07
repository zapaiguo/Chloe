using Chloe.Query.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chloe.Extensions;

namespace Chloe.Query
{
    public class SelectEntity : IRawEntity
    {
        //ITable _table;
        ResultElement _resultEntity;

        public SelectEntity(ResultElement resultEntity)
        {
            //this._table = table;
            this._resultEntity = resultEntity;
        }

        public DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter)
        {
            Stack<MemberExpression> memberExpressions = memberExpressionDeriveParameter.Reverse();
            IMappingObjectExpression resultEntity = this._resultEntity.MappingObjectExpression;

            DbExpression ret = null;

            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;

                if (ret != null)
                {
                    ret = DbExpression.MemberAccess(member, ret);
                    continue;
                }

                DbExpression sqlExpression = resultEntity.GetMemberExpression(member);
                if (sqlExpression != null)
                {
                    ret = sqlExpression;
                    continue;
                }
                else
                {
                    IMappingObjectExpression subResultEntity = resultEntity.GetNavMemberExpression(member);
                    if (subResultEntity == null)
                    {
                        throw new Exception(string.Format("属性：{0}", memberExpression.ToString()));
                    }

                    resultEntity = subResultEntity;
                }
            }

            if (ret == null)
                throw new Exception(memberExpressionDeriveParameter.ToString());

            return ret;
        }
        public IncludeMemberInfo IncludeNavigationMember(MemberExpression memberExpressionDeriveParameter)
        {
            throw new NotSupportedException();
        }

    }
}
