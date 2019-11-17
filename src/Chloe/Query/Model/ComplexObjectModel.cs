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
using Chloe.Query.QueryExpressions;
using Chloe.Infrastructure;
using Chloe.Query.Visitors;

namespace Chloe.Query
{
    public class ComplexObjectModel : IObjectModel
    {
        public ComplexObjectModel(Type objectType) : this(objectType.GetConstructor(Type.EmptyTypes))
        {
        }
        public ComplexObjectModel(ConstructorInfo constructor)
            : this(ConstructorDescriptor.GetInstance(constructor))
        {
        }
        public ComplexObjectModel(ConstructorDescriptor constructorDescriptor)
        {
            this.ObjectType = constructorDescriptor.ConstructorInfo.DeclaringType;
            this.ConstructorDescriptor = constructorDescriptor;
            this.PrimitiveConstructorParameters = new Dictionary<ParameterInfo, DbExpression>();
            this.ComplexConstructorParameters = new Dictionary<ParameterInfo, ComplexObjectModel>();
            this.PrimitiveMembers = new Dictionary<MemberInfo, DbExpression>();
            this.ComplexMembers = new Dictionary<MemberInfo, ComplexObjectModel>();
            this.CollectionMembers = new Dictionary<MemberInfo, CollectionObjectModel>();
        }

        public Type ObjectType { get; private set; }
        public TypeKind TypeKind { get { return TypeKind.Complex; } }
        public DbExpression PrimaryKey { get; set; }
        public DbExpression NullChecking { get; set; }

        public DbExpression Condition { get; set; }
        public DbExpression Filter { get; set; }

        /// <summary>
        /// 返回类型
        /// </summary>
        public ConstructorDescriptor ConstructorDescriptor { get; private set; }
        public Dictionary<ParameterInfo, DbExpression> PrimitiveConstructorParameters { get; private set; }
        public Dictionary<ParameterInfo, ComplexObjectModel> ComplexConstructorParameters { get; private set; }
        public Dictionary<MemberInfo, DbExpression> PrimitiveMembers { get; protected set; }
        public Dictionary<MemberInfo, ComplexObjectModel> ComplexMembers { get; protected set; }
        public Dictionary<MemberInfo, CollectionObjectModel> CollectionMembers { get; protected set; }

        public bool IsNavModel { get; set; }
        public bool IsRootObject { get; set; }
        public bool HasOneToMany
        {
            get
            {
                if (this.CollectionMembers.Count > 0)
                    return true;

                foreach (var item in this.ComplexMembers)
                {
                    if (item.Value.HasOneToMany)
                        return true;
                }

                return false;
            }
        }

        public void AddConstructorParameter(ParameterInfo p, DbExpression primitiveExp)
        {
            this.PrimitiveConstructorParameters.Add(p, primitiveExp);
        }
        public void AddConstructorParameter(ParameterInfo p, ComplexObjectModel complexModel)
        {
            this.ComplexConstructorParameters.Add(p, complexModel);
        }
        public void AddPrimitiveMember(MemberInfo memberInfo, DbExpression exp)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ObjectType);
            this.PrimitiveMembers.Add(memberInfo, exp);
        }

        /// <summary>
        /// 考虑匿名函数构造函数参数和其只读属性的对应
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public DbExpression GetPrimitiveMember(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ObjectType);
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

        public void AddComplexMember(MemberInfo memberInfo, ComplexObjectModel model)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ObjectType);
            this.ComplexMembers.Add(memberInfo, model);
        }
        public ComplexObjectModel GetComplexMember(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ObjectType);
            ComplexObjectModel ret = null;
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

            return ret as ComplexObjectModel;
        }

        public void AddCollectionMember(MemberInfo memberInfo, CollectionObjectModel model)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ObjectType);
            this.CollectionMembers.Add(memberInfo, model);
        }
        public CollectionObjectModel GetCollectionMember(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.ObjectType);
            CollectionObjectModel ret = this.CollectionMembers.FindValue(memberInfo);

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
            ComplexObjectActivatorCreator activatorCreator = new ComplexObjectActivatorCreator(this.ConstructorDescriptor);

            foreach (var kv in this.PrimitiveConstructorParameters)
            {
                ParameterInfo pi = kv.Key;
                DbExpression exp = kv.Value;

                int ordinal;
                ordinal = ObjectModelHelper.TryGetOrAddColumn(sqlQuery, exp, pi.Name).Value;

                if (exp == this.NullChecking)
                    activatorCreator.CheckNullOrdinal = ordinal;

                activatorCreator.ConstructorParameters.Add(pi, ordinal);
            }

            foreach (var kv in this.ComplexConstructorParameters)
            {
                ParameterInfo pi = kv.Key;
                IObjectModel val = kv.Value;

                IObjectActivatorCreator complexMappingMember = val.GenarateObjectActivatorCreator(sqlQuery);
                activatorCreator.ConstructorEntityParameters.Add(pi, complexMappingMember);
            }

            foreach (var kv in this.PrimitiveMembers)
            {
                MemberInfo member = kv.Key;
                DbExpression exp = kv.Value;

                int ordinal;
                ordinal = ObjectModelHelper.TryGetOrAddColumn(sqlQuery, exp, member.Name).Value;

                if (exp == this.NullChecking)
                    activatorCreator.CheckNullOrdinal = ordinal;

                activatorCreator.MappingMembers.Add(member, ordinal);
            }

            foreach (var kv in this.ComplexMembers)
            {
                MemberInfo member = kv.Key;
                IObjectModel val = kv.Value;

                IObjectActivatorCreator complexMappingMember = val.GenarateObjectActivatorCreator(sqlQuery);
                activatorCreator.ComplexMembers.Add(kv.Key, complexMappingMember);
            }

            if (activatorCreator.CheckNullOrdinal == null)
                activatorCreator.CheckNullOrdinal = ObjectModelHelper.TryGetOrAddColumn(sqlQuery, this.NullChecking);

            return activatorCreator;
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
                newModel.AddConstructorParameter(pi, (ComplexObjectModel)complexMappingMember);
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
                newModel.AddComplexMember(member, (ComplexObjectModel)complexMappingMember);
            }

            if (newModel.NullChecking == null)
                newModel.NullChecking = ObjectModelHelper.TryGetOrAddNullChecking(sqlQuery, table, this.NullChecking);

            return newModel;
        }

        public void Include(NavigationNode navigationNode, QueryModel queryModel)
        {
            TypeDescriptor descriptor = EntityTypeContainer.GetDescriptor(this.ObjectType);
            PropertyDescriptor navigationDescriptor = descriptor.GetPropertyDescriptor(navigationNode.Property);

            if (navigationDescriptor.Definition.Kind == TypeKind.Primitive)
            {
                throw new NotSupportedException($"'{navigationNode.Property.Name}' is not navigation property.");
            }

            ComplexObjectModel objectModel = null;
            if (navigationDescriptor.Definition.Kind == TypeKind.Complex)
            {
                objectModel = this.GetComplexMember(navigationNode.Property);

                if (objectModel == null)
                {
                    objectModel = this.GenComplexObjectModel(navigationDescriptor.PropertyType, navigationNode, queryModel);
                    this.AddComplexMember(navigationNode.Property, objectModel);
                }
                else
                {
                    objectModel.AppendCondition(FilterPredicateParser.Parse(navigationNode.Condition, new ScopeParameterDictionary(1) { { navigationNode.Condition.Parameters[0], objectModel } }, queryModel.ScopeTables));
                }
            }
            else
            {
                CollectionObjectModel navModel = this.GetCollectionMember(navigationNode.Property);

                if (navModel == null)
                {
                    Type collectionType = navigationDescriptor.PropertyType;
                    ComplexObjectModel elementModel = this.GenComplexObjectModel(collectionType.GetGenericArguments()[0], navigationNode, queryModel);
                    navModel = new CollectionObjectModel(this.ObjectType, navigationNode.Property, elementModel);
                    this.AddCollectionMember(navigationNode.Property, navModel);
                }

                objectModel = navModel.ElementModel;
            }

            if (navigationNode.Next != null)
            {
                objectModel.Include(navigationNode.Next, queryModel);
            }
        }
        ComplexObjectModel GenComplexObjectModel(Type objectType, NavigationNode navigationNode, QueryModel queryModel)
        {
            TypeDescriptor navigationTypeDescriptor = EntityTypeContainer.GetDescriptor(objectType);
            DbTable table = new DbTable(queryModel.GenerateUniqueTableAlias(navigationTypeDescriptor.Table.Name));
            ComplexObjectModel objectModel = navigationTypeDescriptor.GenObjectModel(table);

            objectModel.Condition = FilterPredicateParser.Parse(navigationNode.Condition, new ScopeParameterDictionary(1) { { navigationNode.Condition.Parameters[0], objectModel } }, queryModel.ScopeTables);
            objectModel.Filter = FilterPredicateParser.Parse(navigationNode.Filter, new ScopeParameterDictionary(1) { { navigationNode.Filter.Parameters[0], objectModel } }, queryModel.ScopeTables);

            return objectModel;
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

        public void AppendCondition(DbExpression condition)
        {
            if (this.Condition == null)
                this.Condition = condition;
            else
                this.Condition = new DbAndExpression(this.Condition, condition);
        }
    }
}
