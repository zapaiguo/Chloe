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
            if (exp.NodeType == ExpressionType.New)
            {
                return this.VisitNewExpression((NewExpression)exp);
            }

            if (exp.NodeType == ExpressionType.MemberInit)
            {
                return this.VisitMemberInit((MemberInitExpression)exp);
            }

            if (exp.NodeType == ExpressionType.MemberAccess)
            {

                return this.VisitNavigationMember((MemberExpression)exp);
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
            MappingMembers result = new MappingMembers(exp.Constructor);
            ParameterInfo[] parames = exp.Constructor.GetParameters();
            for (int i = 0; i < parames.Length; i++)
            {
                ParameterInfo pi = parames[i];
                Expression argExp = exp.Arguments[i];
                if (Utils.IsMapType(pi.ParameterType))
                {
                    DbExpression dbExpression = this.VisistExpression(argExp);
                    result.ConstructorParameters.Add(pi, dbExpression);
                }
                //else if (argExp.NodeType == ExpressionType.MemberAccess)
                //{
                //    //访问了导航属性
                //    MappingMembers subResult = this.VisitNavigationMember((MemberExpression)argExp);
                //    result.ConstructorEntityParameters.Add(pi, subResult);
                //}
                else
                {
                    MappingMembers subResult = this.VisitSelectBody(argExp);
                    result.ConstructorEntityParameters.Add(pi, subResult);
                }
            }

            return result;
        }

        protected virtual MappingMembers VisitMemberInit(MemberInitExpression init)
        {
            MappingMembers result = this.VisitNewExpression(init.NewExpression);

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
                //else if (memberAssignment.Expression.NodeType == ExpressionType.MemberAccess)
                //{
                //    //访问了导航属性
                //    MappingMembers subResult = this.VisitNavigationMember((MemberExpression)memberAssignment.Expression);
                //    result.SubResultEntities.Add(member, subResult);
                //}
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
