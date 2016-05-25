using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Extensions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.Core.Visitors
{
    public class UpdateBodyExpressionVisitor : ExpressionVisitor<Dictionary<MappingMemberDescriptor, DbExpression>>
    {
        TypeDescriptor _typeDescriptor;
        ExpressionVisitorBase _visitor;

        public UpdateBodyExpressionVisitor(TypeDescriptor typeDescriptor)
        {
            this._typeDescriptor = typeDescriptor;
            this._visitor = typeDescriptor.Visitor;
        }
        public override Dictionary<MappingMemberDescriptor, DbExpression> Visit(Expression exp)
        {
            if (exp == null)
                return null;

            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }
        protected override Dictionary<MappingMemberDescriptor, DbExpression> VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }
        protected override Dictionary<MappingMemberDescriptor, DbExpression> VisitMemberInit(MemberInitExpression exp)
        {
            if (exp.NewExpression.Arguments.Count > 0)
                throw new NotSupportedException("不支持带参数构造函数");

            Dictionary<MappingMemberDescriptor, DbExpression> ret = new Dictionary<MappingMemberDescriptor, DbExpression>();

            Dictionary<MemberInfo, MappingMemberDescriptor> mappingMemberDescriptors = this._typeDescriptor.MappingMemberDescriptors;
            Dictionary<MemberInfo, DbColumnAccessExpression> memberColumnMap = this._typeDescriptor.MemberColumnMap;

            foreach (MemberBinding binding in exp.Bindings)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }

                MemberAssignment memberAssignment = (MemberAssignment)binding;
                MemberInfo member = memberAssignment.Member;
                Type memberType = ReflectionExtensions.GetPropertyOrFieldType(member);

                DbColumnAccessExpression dbColumnAccessExpression;
                if (!memberColumnMap.TryGetValue(member, out dbColumnAccessExpression))
                {
                    throw new Exception(string.Format("成员 {0} 未映射任何列", member.Name));
                }

                MappingMemberDescriptor memberDescriptor = mappingMemberDescriptors[member];

                DbColumn column = dbColumnAccessExpression.Column;

                var valueExp = this._visitor.Visit(memberAssignment.Expression);

                ret.Add(memberDescriptor, valueExp);
            }

            return ret;
        }

    }
}
