using Chloe.Mapper;
using Chloe.Descriptors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Chloe.Infrastructure;
using Chloe.Mapper.Binders;
using Chloe.Mapper.Activators;

namespace Chloe.Query.Mapping
{
    public class ComplexObjectActivatorCreator : IObjectActivatorCreator
    {
        public ComplexObjectActivatorCreator(ConstructorDescriptor constructorDescriptor)
        {
            this.ConstructorDescriptor = constructorDescriptor;
            this.ConstructorParameters = new Dictionary<ParameterInfo, int>();
            this.ConstructorEntityParameters = new Dictionary<ParameterInfo, IObjectActivatorCreator>();
            this.PrimitiveMembers = new Dictionary<MemberInfo, int>();
            this.ComplexMembers = new Dictionary<MemberInfo, IObjectActivatorCreator>();
            this.CollectionMembers = new Dictionary<MemberInfo, IObjectActivatorCreator>();
        }

        public Type ObjectType { get { return this.ConstructorDescriptor.ConstructorInfo.DeclaringType; } }
        public bool IsRoot { get; set; }
        public int? CheckNullOrdinal { get; set; }
        public ConstructorDescriptor ConstructorDescriptor { get; private set; }
        public Dictionary<ParameterInfo, int> ConstructorParameters { get; private set; }
        public Dictionary<ParameterInfo, IObjectActivatorCreator> ConstructorEntityParameters { get; private set; }

        /// <summary>
        /// 映射成员集合。以 MemberInfo 为 key，读取 DataReader 时的 Ordinal 为 value
        /// </summary>
        public Dictionary<MemberInfo, int> PrimitiveMembers { get; private set; }
        /// <summary>
        /// 复杂类型成员集合。
        /// </summary>
        public Dictionary<MemberInfo, IObjectActivatorCreator> ComplexMembers { get; private set; }
        public Dictionary<MemberInfo, IObjectActivatorCreator> CollectionMembers { get; private set; }


        public IObjectActivator CreateObjectActivator()
        {
            return this.CreateObjectActivator(null);
        }
        public IObjectActivator CreateObjectActivator(IDbContext dbContext)
        {
            ObjectMemberMapper mapper = this.ConstructorDescriptor.GetEntityMemberMapper();
            List<IValueSetter> memberSetters = new List<IValueSetter>(this.PrimitiveMembers.Count + this.ComplexMembers.Count);
            foreach (var kv in this.PrimitiveMembers)
            {
                IMRM mMapper = mapper.TryGetMappingMemberMapper(kv.Key);
                PrimitiveMemberBinder binder = new PrimitiveMemberBinder(mMapper, kv.Value);
                memberSetters.Add(binder);
            }

            foreach (var kv in this.ComplexMembers)
            {
                Action<object, object> del = mapper.FindComplexMemberSetter(kv.Key);
                IObjectActivator memberActivtor = kv.Value.CreateObjectActivator(dbContext);
                ComplexMemberBinder binder = new ComplexMemberBinder(del, memberActivtor);
                memberSetters.Add(binder);
            }

            foreach (var kv in this.CollectionMembers)
            {
                Action<object, object> del = mapper.FindComplexMemberSetter(kv.Key);
                IObjectActivator memberActivtor = kv.Value.CreateObjectActivator(dbContext);
                CollectionMemberBinder binder = new CollectionMemberBinder(del, memberActivtor);
                memberSetters.Add(binder);
            }

            Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> instanceCreator = this.ConstructorDescriptor.GetInstanceCreator();

            List<int> readerOrdinals = this.ConstructorParameters.Select(a => a.Value).ToList();
            List<IObjectActivator> objectActivators = this.ConstructorEntityParameters.Select(a => a.Value.CreateObjectActivator()).ToList();

            IObjectActivator objectActivator;
            if (dbContext != null)
                objectActivator = new ObjectActivatorWithTracking(instanceCreator, readerOrdinals, objectActivators, memberSetters, this.CheckNullOrdinal, dbContext);
            else
                objectActivator = new ComplexObjectActivator(instanceCreator, readerOrdinals, objectActivators, memberSetters, this.CheckNullOrdinal);

            if (this.IsRoot && this.HasMany())
            {
                TypeDescriptor entityTypeDescriptor = EntityTypeContainer.GetDescriptor(this.ObjectType);
                List<Tuple<PropertyDescriptor, int>> keys = new List<Tuple<PropertyDescriptor, int>>(entityTypeDescriptor.PrimaryKeys.Count);
                foreach (PrimitivePropertyDescriptor primaryKey in entityTypeDescriptor.PrimaryKeys)
                {
                    keys.Add(new Tuple<PropertyDescriptor, int>(primaryKey, this.PrimitiveMembers[primaryKey.Definition.Property]));
                }

                IEntityRowCompare entityRowCompare = new EntityRowCompare(keys);
                objectActivator = new RootEntityActivator(objectActivator, this.CreateFitter(dbContext), entityRowCompare);
            }

            return objectActivator;
        }

        public IFitter CreateFitter(IDbContext dbContext)
        {
            List<Tuple<PropertyDescriptor, IFitter>> includings = new List<Tuple<PropertyDescriptor, IFitter>>();
            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(this.ConstructorDescriptor.ConstructorInfo.DeclaringType);
            foreach (var item in this.ComplexMembers.Concat(this.CollectionMembers))
            {
                IFitter propFitter = item.Value.CreateFitter(dbContext);
                includings.Add(new Tuple<PropertyDescriptor, IFitter>(typeDescriptor.GetPropertyDescriptor(item.Key), propFitter));
            }

            ComplexObjectFitter fitter = new ComplexObjectFitter(includings);
            return fitter;
        }

        public bool HasMany()
        {
            if (this.CollectionMembers.Count > 0)
                return true;

            foreach (var kv in this.ComplexMembers)
            {
                ComplexObjectActivatorCreator activatorCreator = kv.Value as ComplexObjectActivatorCreator;
                if (activatorCreator.HasMany())
                    return true;
            }

            return false;
        }
    }
}
