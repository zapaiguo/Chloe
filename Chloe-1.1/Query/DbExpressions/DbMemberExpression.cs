using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chloe.Extensions;

namespace Chloe.Query.DbExpressions
{
    public class DbMemberExpression : DbExpression
    {
        MemberInfo _member;
        DbExpression _exp;
        public DbMemberExpression(MemberInfo member, DbExpression exp)
            : base(DbExpressionType.MemberAccess)
        {
            if (member.MemberType != MemberTypes.Property || member.MemberType != MemberTypes.Field)
                throw new ArgumentException();

            this._member = member;
            this._exp = exp;
        }

        public override Type Type
        {
            get
            {
                return this._member.GetPropertyOrFieldType();
            }
        }

        public MemberInfo Member
        {
            get { return this._member; }
        }

        /// <summary>
        /// 字段或属性的包含对象
        /// </summary>
        public DbExpression Expression
        {
            get { return this._exp; }
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
