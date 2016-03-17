using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chloe.Core;
using Chloe.Extensions;
using Chloe.Utility;
using Chloe.Query.DbExpressions;
using Chloe.Query.Implementation;

namespace Chloe.Query
{
    internal abstract class SelectExpressionVisitor
    {
        protected BaseExpressionVisitor _visitor = null;
        protected SelectExpressionVisitor(BaseExpressionVisitor visitor)
        {
            Utils.CheckNull(visitor, "visitor");

            this._visitor = visitor;
        }

        public MappingMembers Visit(Expression exp)
        {
            Utils.CheckNull(exp);

            Expression body = exp;
            if (exp.NodeType == ExpressionType.Lambda)
            {
                body = ((LambdaExpression)exp).Body;
            }

            return this.VisitSelectBody(body);
        }

        MappingMembers VisitSelectBody(Expression exp)
        {
            NewExpression newExpression = exp as NewExpression;
            if (newExpression != null)
            {
                return this.VisitNewExpression(newExpression);
            }

            MemberInitExpression initExp = exp as MemberInitExpression;

            if (initExp != null)
            {
                return this.VisitMemberInit(initExp);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// 对于 NewExpression 只支持匿名类型，在这需要判定是否为匿名类型，不是则引发异常
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        protected virtual MappingMembers VisitNewExpression(NewExpression exp)
        {
            MappingMembers result = new MappingMembers(exp.Type);
            for (int i = 0; i < exp.Members.Count; i++)
            {
                PropertyInfo member = (PropertyInfo)exp.Members[i];
                Expression argExp = exp.Arguments[i];

                //是数据库映射类型
                if (Utils.IsMapType(member.PropertyType))
                {
                    DbExpression dbExpression = this.VisistExpression(argExp);

                    result.SelectedMembers.Add(member, dbExpression);
                }
                else if (argExp.NodeType == ExpressionType.MemberAccess)
                {
                    //访问了导航属性
                    MappingMembers subResult = this.VisitNavigationMember((MemberExpression)argExp);
                    result.SubResultEntities.Add(member, subResult);
                }
                else
                {
                    //对于非数据库映射类型，只支持 NewExpression 和 MemberInitExpression
                    MappingMembers subResult = this.VisitSelectBody(argExp);
                    result.SubResultEntities.Add(member, subResult);
                }
            }

            return result;
        }

        protected virtual MappingMembers VisitMemberInit(MemberInitExpression init)
        {
            if (init.NewExpression.Arguments.Count > 0)
                throw new NotSupportedException("有参构造函数");

            MappingMembers result = new MappingMembers(init.Type);

            NewExpression n = init.NewExpression;
            if (n.Arguments.Count > 0)
            {
                throw new NotSupportedException();
            }

            foreach (MemberBinding binding in init.Bindings)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }

                MemberAssignment memberAssignment = (MemberAssignment)binding;
                MemberInfo member = memberAssignment.Member;
                Type memberType = member.GetPropertyOrFieldType();

                //是数据库映射类型
                if (Utils.IsMapType(memberType))
                {
                    DbExpression dbExpression = this.VisistExpression(memberAssignment.Expression);
                    result.SelectedMembers.Add(member, dbExpression);
                }
                else if (memberAssignment.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    //访问了导航属性
                    MappingMembers subResult = this.VisitNavigationMember((MemberExpression)memberAssignment.Expression);
                    result.SubResultEntities.Add(member, subResult);
                }
                else
                {
                    //对于非数据库映射类型，只支持 NewExpression 和 MemberInitExpression
                    MappingMembers subResult = this.VisitSelectBody(memberAssignment.Expression);
                    result.SubResultEntities.Add(member, subResult);
                }
            }

            return result;
        }

        protected abstract MappingMembers VisitNavigationMember(MemberExpression exp);

        protected DbExpression VisistExpression(Expression exp)
        {
            //在这里解析表达式树
            return this._visitor.Visit(exp);
        }
    }
}
