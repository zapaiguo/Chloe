using Chloe.Extensions;
using Chloe.InternalExtensions;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Query.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    public class ComplexObjectModel : IObjectModel
    {
        public ComplexObjectModel(Type objectType) : this(objectType.GetConstructor(Type.EmptyTypes))
        {
        }
        public ComplexObjectModel(ConstructorInfo constructor)
            : this(EntityConstructorDescriptor.GetInstance(constructor))
        {
        }
        public ComplexObjectModel(EntityConstructorDescriptor constructorDescriptor)
        {
            this.ObjectType = constructorDescriptor.ConstructorInfo.DeclaringType;
            this.ConstructorDescriptor = constructorDescriptor;
            this.PrimitiveConstructorParameters = new Dictionary<ParameterInfo, DbExpression>();
            this.ComplexConstructorParameters = new Dictionary<ParameterInfo, IObjectModel>();
            this.PrimitiveMembers = new Dictionary<MemberInfo, DbExpression>();
            this.ComplexMembers = new Dictionary<MemberInfo, IObjectModel>();
        }

        public Type ObjectType { get; private set; }
        public DbExpression PrimaryKey { get; set; }
        public DbExpression NullChecking { get; set; }

        public DbExpression Condition { get; set; }
        public DbExpression Filter { get; set; }

        /// <summary>
        /// 返回类型
        /// </summary>
        public EntityConstructorDescriptor ConstructorDescriptor { get; private set; }
        public Dictionary<ParameterInfo, DbExpression> PrimitiveConstructorParameters { get; private set; }
        public Dictionary<ParameterInfo, IObjectModel> ComplexConstructorParameters { get; private set; }
        public Dictionary<MemberInfo, DbExpression> PrimitiveMembers { get; protected set; }
        public Dictionary<MemberInfo, IObjectModel> ComplexMembers { get; protected set; }

        public void AddConstructorParameter(ParameterInfo p, DbExpression primitiveExp)
        {
            this.PrimitiveConstructorParameters.Add(p, primitiveExp);
        }
        public void AddConstructorParameter(ParameterInfo p, IObjectModel complexModel)
        {
            this.ComplexConstructorParameters.Add(p, complexModel);
        }
        public void AddPrimitiveMember(MemberInfo memberInfo, DbExpression exp)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ConstructorDescriptor.ConstructorInfo.DeclaringType);
            this.PrimitiveMembers.Add(memberInfo, exp);
        }
        public void AddComplexMember(MemberInfo memberInfo, IObjectModel model)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ConstructorDescriptor.ConstructorInfo.DeclaringType);
            this.ComplexMembers.Add(memberInfo, model);
        }
        /// <summary>
        /// 考虑匿名函数构造函数参数和其只读属性的对应
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public DbExpression GetPrimitiveMember(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ConstructorDescriptor.ConstructorInfo.DeclaringType);
            DbExpression ret = null;
            if (!this.PrimitiveMembers.TryGetValue(memberInfo, out ret))
            {
                ParameterInfo p = null;
                if (!this.ConstructorDescriptor.MemberParameterMap.TryGetValue(memberInfo, out p))
                {
                    return null;
                }

                if (!this.PrimitiveConstructorParameters.TryGetValue(p, out ret))
                {
                    return null;
                }
            }

            return ret;
        }
        public IObjectModel GetComplexMember(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ConstructorDescriptor.ConstructorInfo.DeclaringType);
            IObjectModel ret = null;
            if (!this.ComplexMembers.TryGetValue(memberInfo, out ret))
            {
                //从构造函数中查
                ParameterInfo p = null;
                if (!this.ConstructorDescriptor.MemberParameterMap.TryGetValue(memberInfo, out p))
                {
                    return null;
                }

                if (!this.ComplexConstructorParameters.TryGetValue(p, out ret))
                {
                    return null;
                }
            }

            return ret;
        }
        public DbExpression GetDbExpression(MemberExpression memberExpressionDeriveFromParameter)
        {
            Stack<MemberExpression> memberExpressions = ExpressionExtension.Reverse(memberExpressionDeriveFromParameter);

            DbExpression ret = null;
            IObjectModel model = this;
            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo accessedMember = memberExpression.Member;

                if (model == null && ret != null)
                {
                    /* a.F_DateTime.Value.Date */
                    ret = DbExpression.MemberAccess(accessedMember, ret);
                    continue;
                }

                /* **.accessedMember */
                DbExpression e = model.GetPrimitiveMember(accessedMember);
                if (e == null)
                {
                    /* Indicate current accessed member is not mapping member, then try get complex member like 'a.Order' */
                    model = model.GetComplexMember(accessedMember);

                    if (model == null)
                    {
                        if (ret == null)
                        {
                            /*
                             * If run here,the member access expression must be like 'a.xx',
                             * and member 'xx' is neither mapping member nor complex member,in this case,we not supported.
                             */
                            throw new InvalidOperationException(memberExpressionDeriveFromParameter.ToString());
                        }
                        else
                        {
                            /* Non mapping member is not found also, then convert linq MemberExpression to DbMemberExpression */
                            ret = DbExpression.MemberAccess(accessedMember, ret);
                            continue;
                        }
                    }

                    if (ret != null)
                    {
                        /* This case and case #110 will not appear in normal, if you meet,please email me(so_while@163.com) or call 911 for help. */
                        throw new InvalidOperationException(memberExpressionDeriveFromParameter.ToString());
                    }
                }
                else
                {
                    if (ret != null)//Case: #110
                        throw new InvalidOperationException(memberExpressionDeriveFromParameter.ToString());

                    ret = e;
                }
            }

            if (ret == null)
            {
                /*
                 * If run here,the input argument 'memberExpressionDeriveFromParameter' expression must be like 'a.xx','a.**.xx','a.**.**.xx' ...and so on,
                 * and the last accessed member 'xx' is not mapping member, in this case, we not supported too.
                 */
                throw new InvalidOperationException(memberExpressionDeriveFromParameter.ToString());
            }

            return ret;
        }
        public IObjectModel GetComplexMember(MemberExpression memberExpressionDeriveParameter)
        {
            Stack<MemberExpression> memberExpressions = ExpressionExtension.Reverse(memberExpressionDeriveParameter);

            if (memberExpressions.Count == 0)
                throw new Exception();

            IObjectModel ret = this;
            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;

                ret = ret.GetComplexMember(member);
                if (ret == null)
                {
                    throw new NotSupportedException(memberExpressionDeriveParameter.ToString());
                }
            }

            return ret;
        }
        public IObjectActivatorCreator GenarateObjectActivatorCreator(DbSqlQueryExpression sqlQuery)
        {
            MappingEntity mappingEntity = new MappingEntity(this.ConstructorDescriptor);

            foreach (var kv in this.PrimitiveConstructorParameters)
            {
                ParameterInfo pi = kv.Key;
                DbExpression exp = kv.Value;

                int ordinal;
                ordinal = ObjectModelHelper.TryGetOrAddColumn(sqlQuery, exp, pi.Name).Value;

                if (exp == this.NullChecking)
                    mappingEntity.CheckNullOrdinal = ordinal;

                mappingEntity.ConstructorParameters.Add(pi, ordinal);
            }

            foreach (var kv in this.ComplexConstructorParameters)
            {
                ParameterInfo pi = kv.Key;
                IObjectModel val = kv.Value;

                IObjectActivatorCreator complexMappingMember = val.GenarateObjectActivatorCreator(sqlQuery);
                mappingEntity.ConstructorEntityParameters.Add(pi, complexMappingMember);
            }

            foreach (var kv in this.PrimitiveMembers)
            {
                MemberInfo member = kv.Key;
                DbExpression exp = kv.Value;

                int ordinal;
                ordinal = ObjectModelHelper.TryGetOrAddColumn(sqlQuery, exp, member.Name).Value;

                if (exp == this.NullChecking)
                    mappingEntity.CheckNullOrdinal = ordinal;

                mappingEntity.MappingMembers.Add(member, ordinal);
            }

            foreach (var kv in this.ComplexMembers)
            {
                MemberInfo member = kv.Key;
                IObjectModel val = kv.Value;

                IObjectActivatorCreator complexMappingMember = val.GenarateObjectActivatorCreator(sqlQuery);
                mappingEntity.ComplexMembers.Add(kv.Key, complexMappingMember);
            }

            if (mappingEntity.CheckNullOrdinal == null)
                mappingEntity.CheckNullOrdinal = ObjectModelHelper.TryGetOrAddColumn(sqlQuery, this.NullChecking);

            return mappingEntity;
        }

        public IObjectModel ToNewObjectModel(DbSqlQueryExpression sqlQuery, DbTable table)
        {
            ComplexObjectModel newModel = new ComplexObjectModel(this.ConstructorDescriptor);

            foreach (var kv in this.PrimitiveConstructorParameters)
            {
                ParameterInfo pi = kv.Key;
                DbExpression exp = kv.Value;

                DbColumnAccessExpression cae = null;
                cae = ObjectModelHelper.ParseColumnAccessExpression(sqlQuery, table, exp, pi.Name);

                newModel.AddConstructorParameter(pi, cae);
            }

            foreach (var kv in this.ComplexConstructorParameters)
            {
                ParameterInfo pi = kv.Key;
                IObjectModel val = kv.Value;

                IObjectModel complexMappingMember = val.ToNewObjectModel(sqlQuery, table);
                newModel.AddConstructorParameter(pi, complexMappingMember);
            }

            foreach (var kv in this.PrimitiveMembers)
            {
                MemberInfo member = kv.Key;
                DbExpression exp = kv.Value;

                DbColumnAccessExpression cae = null;
                cae = ObjectModelHelper.ParseColumnAccessExpression(sqlQuery, table, exp, member.Name);

                newModel.AddPrimitiveMember(member, cae);

                if (exp == this.PrimaryKey)
                {
                    newModel.PrimaryKey = cae;
                    if (this.NullChecking == this.PrimaryKey)
                        newModel.NullChecking = cae;
                }
            }

            foreach (var kv in this.ComplexMembers)
            {
                MemberInfo member = kv.Key;
                IObjectModel val = kv.Value;

                IObjectModel complexMappingMember = val.ToNewObjectModel(sqlQuery, table);
                newModel.AddComplexMember(member, complexMappingMember);
            }

            if (newModel.NullChecking == null)
                newModel.NullChecking = ObjectModelHelper.TryGetOrAddNullChecking(sqlQuery, table, this.NullChecking);

            return newModel;
        }

        public void SetNullChecking(DbExpression exp)
        {
            if (this.NullChecking == null)
            {
                if (this.PrimaryKey != null)
                    this.NullChecking = this.PrimaryKey;
                else
                    this.NullChecking = exp;
            }

            foreach (var item in this.ComplexConstructorParameters.Values)
            {
                item.SetNullChecking(exp);
            }

            foreach (var item in this.ComplexMembers.Values)
            {
                item.SetNullChecking(exp);
            }
        }
    }
}
