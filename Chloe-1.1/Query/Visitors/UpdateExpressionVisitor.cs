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
using System.Threading.Tasks;

namespace Chloe.Query.Visitors
{
    public class UpdateColumnExpressionVisitor : ExpressionVisitor<Dictionary<DbColumn, DbExpression>>
    {
        MappingTypeDescriptor _typeDescriptor;
        ExpressionVisitorBase _visitor;

        public UpdateColumnExpressionVisitor(MappingTypeDescriptor typeDescriptor)
        {
            this._typeDescriptor = typeDescriptor;
            this._visitor = typeDescriptor.Visitor;
        }

        //public static Dictionary<DbColumn, DbExpression> VisitExpression(LambdaExpression exp, MappingTypeDescriptor typeDescriptor, ExpressionVisitorBase visitor)
        //{
        //    var visitor1 = new UpdateColumnExpressionVisitor(typeDescriptor);
        //    visitor1._visitor = visitor;
        //    return visitor1.Visit(exp);
        //}

        public override Dictionary<DbColumn, DbExpression> Visit(Expression exp)
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
        protected override Dictionary<DbColumn, DbExpression> VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }
        protected override Dictionary<DbColumn, DbExpression> VisitMemberInit(MemberInitExpression exp)
        {
            if (exp.NewExpression.Arguments.Count > 0)
                throw new NotSupportedException("不支持带参数构造函数");

            Dictionary<DbColumn, DbExpression> ret = new Dictionary<DbColumn, DbExpression>();

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
                if (memberDescriptor.IsPrimaryKey || memberDescriptor.IsAutoIncrement)
                {
                    throw new Exception(string.Format("成员 {0} 属于主键或自增列，无法对其进行更新操作", member.Name));
                }

                DbColumn column = dbColumnAccessExpression.Column;

                var valueExp = this._visitor.Visit(memberAssignment.Expression);

                ret.Add(column, valueExp);
            }

            return ret;
        }

    }
}
