using Chloe.Extensions;
using Chloe.Query.DbExpressions;
using Chloe.Query.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Chloe.Query
{
    public interface IMappingObjectExpression
    {
        IObjectActivtorCreator GenarateObjectActivtorCreator(DbSqlQueryExpression sqlQuery);
        void AddConstructorParameter(ParameterInfo p, DbExpression exp);
        void AddConstructorEntityParameter(ParameterInfo p, IMappingObjectExpression exp);
        void AddMemberExpression(MemberInfo p, DbExpression exp);
        void AddNavMemberExpression(MemberInfo p, IMappingObjectExpression exp);
        DbExpression GetMemberExpression(MemberInfo memberInfo);
        IMappingObjectExpression GetNavMemberExpression(MemberInfo memberInfo);
        DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter);
        IMappingObjectExpression GetNavMemberExpression(MemberExpression exp);

    }

    public class MappingFieldExpression : IMappingObjectExpression
    {
        Type _type;
        DbExpression _exp;
        public MappingFieldExpression(Type type, DbExpression exp)
        {
            this._type = type;
            this._exp = exp;
        }
        public void AddConstructorParameter(ParameterInfo p, DbExpression exp)
        {
            throw new NotSupportedException();
        }
        public void AddConstructorEntityParameter(ParameterInfo p, IMappingObjectExpression exp)
        {
            throw new NotSupportedException();
        }
        public void AddMemberExpression(MemberInfo p, DbExpression exp)
        {
            throw new NotSupportedException();
        }
        public void AddNavMemberExpression(MemberInfo p, IMappingObjectExpression exp)
        {
            throw new NotSupportedException();
        }
        public DbExpression GetMemberExpression(MemberInfo memberInfo)
        {
            throw new NotSupportedException();
        }
        public IMappingObjectExpression GetNavMemberExpression(MemberInfo memberInfo)
        {
            throw new NotSupportedException();
        }
        public DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter)
        {
            Stack<MemberExpression> memberExpressions = ExpressionExtensions.Reverse(memberExpressionDeriveParameter);

            if (memberExpressions.Count == 0)
                throw new Exception();

            DbExpression ret = this._exp;

            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;
                ret = DbExpression.MemberAccess(member, ret);
            }

            if (ret == null)
                throw new Exception(memberExpressionDeriveParameter.ToString());

            return ret;
        }

        public IObjectActivtorCreator GenarateObjectActivtorCreator(DbSqlQueryExpression sqlQuery)
        {
            List<DbColumnExpression> columnList = sqlQuery.Columns;
            string alias = sqlQuery.GenerateUniqueColumnAlias();
            DbColumnExpression columnExp = new DbColumnExpression(this._type, this._exp, alias);
            columnList.Add(columnExp);
            int ordinal = columnList.Count - 1;
            MappingField ac = new MappingField(this._type, ordinal);
            return ac;
        }

        public IMappingObjectExpression GetNavMemberExpression(MemberExpression exp)
        {
            throw new NotSupportedException();
        }
    }

    public class MappingObjectExpression : IMappingObjectExpression
    {
        public MappingObjectExpression(ConstructorInfo constructor)
        {
            this.Constructor = constructor;
            this.SelectedMembers = new Dictionary<MemberInfo, DbExpression>();
            this.SubResultEntities = new Dictionary<MemberInfo, IMappingObjectExpression>();
        }
        /// <summary>
        /// 返回类型
        /// </summary>
        public ConstructorInfo Constructor { get; protected set; }
        public Dictionary<ParameterInfo, DbExpression> ConstructorParameters { get; private set; }
        public Dictionary<ParameterInfo, IMappingObjectExpression> ConstructorEntityParameters { get; private set; }
        public Dictionary<MemberInfo, DbExpression> SelectedMembers { get; protected set; }
        public Dictionary<MemberInfo, IMappingObjectExpression> SubResultEntities { get; protected set; }

        public void AddConstructorParameter(ParameterInfo p, DbExpression exp)
        {
            this.ConstructorParameters.Add(p, exp);
        }
        public void AddConstructorEntityParameter(ParameterInfo p, IMappingObjectExpression exp)
        {
            this.ConstructorEntityParameters.Add(p, exp);
        }
        public void AddMemberExpression(MemberInfo m, DbExpression exp)
        {
            this.SelectedMembers.Add(m, exp);
        }
        public void AddNavMemberExpression(MemberInfo p, IMappingObjectExpression exp)
        {
            this.SubResultEntities.Add(p, exp);
        }
        public DbExpression GetMemberExpression(MemberInfo memberInfo)
        {
            DbExpression ret = null;
            if (!this.SelectedMembers.TryGetValue(memberInfo, out ret))
            {
                return null;
            }

            return ret;
        }
        public IMappingObjectExpression GetNavMemberExpression(MemberInfo memberInfo)
        {
            IMappingObjectExpression ret = null;
            if (!this.SubResultEntities.TryGetValue(memberInfo, out ret))
            {
                return null;
            }

            return ret;
        }
        public DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter)
        {
            Stack<MemberExpression> memberExpressions = ExpressionExtensions.Reverse(memberExpressionDeriveParameter);

            DbExpression ret = null;
            IMappingObjectExpression moe = this;
            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;

                DbExpression e = moe.GetMemberExpression(member);
                if (e == null)
                {
                    moe = moe.GetNavMemberExpression(member);
                    if (moe == null)
                    {
                        if (ret == null)
                            throw new Exception();
                        else
                        {
                            ret = DbExpression.MemberAccess(member, ret);
                            continue;
                        }
                    }

                    if (ret != null)
                        throw new NotSupportedException(memberExpressionDeriveParameter.ToString());
                }
                else
                {
                    if (ret != null)
                        throw new NotSupportedException(memberExpressionDeriveParameter.ToString());

                    ret = e;
                }
            }

            if (ret == null)
                throw new Exception(memberExpressionDeriveParameter.ToString());

            return ret;
        }
        public IMappingObjectExpression GetNavMemberExpression(MemberExpression memberExpressionDeriveParameter)
        {
            Stack<MemberExpression> memberExpressions = ExpressionExtensions.Reverse(memberExpressionDeriveParameter);

            if (memberExpressions.Count == 0)
                throw new Exception();

            IMappingObjectExpression ret = this;
            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;

                ret = ret.GetNavMemberExpression(member);
                if (ret == null)
                {
                    throw new NotSupportedException(memberExpressionDeriveParameter.ToString());
                }
            }

            return ret;
        }
        public IObjectActivtorCreator GenarateObjectActivtorCreator(DbSqlQueryExpression sqlQuery)
        {
            List<DbColumnExpression> columnList = sqlQuery.Columns;
            MappingEntity mappingEntity = new MappingEntity(this.Constructor);
            MappingObjectExpression mappingMembers = this;
            foreach (var kv in mappingMembers.SelectedMembers)
            {
                MemberInfo member = kv.Key;
                DbExpression exp = kv.Value;

                string alias = sqlQuery.GenerateUniqueColumnAlias(member.Name);
                DbColumnExpression columnExp = new DbColumnExpression(exp.Type, exp, alias);
                columnList.Add(columnExp);

                int ordinal = columnList.Count - 1;
                mappingEntity.Members.Add(member, ordinal);
            }

            foreach (var kv in mappingMembers.SubResultEntities)
            {
                MemberInfo member = kv.Key;
                IMappingObjectExpression val = kv.Value;

                IObjectActivtorCreator navMappingMember = val.GenarateObjectActivtorCreator(sqlQuery);
                mappingEntity.EntityMembers.Add(kv.Key, navMappingMember);
            }

            return mappingEntity;
        }
    }
}
