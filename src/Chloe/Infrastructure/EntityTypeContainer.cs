using Chloe.Descriptors;
using Chloe.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chloe.Infrastructure
{
    public class EntityTypeContainer
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, TypeDescriptor> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, TypeDescriptor>();

        public static TypeDescriptor GetDescriptor(Type type)
        {
            Utils.CheckNull(type, nameof(type));

            TypeDescriptor instance;
            if (!InstanceCache.TryGetValue(type, out instance))
            {
                lock (type)
                {
                    if (!InstanceCache.TryGetValue(type, out instance))
                    {
                        IEntityTypeBuilder entityTypeBuilder = Activator.CreateInstance(typeof(InternalEntityTypeBuilder<>).MakeGenericType(type)) as IEntityTypeBuilder;
                        instance = new TypeDescriptor(entityTypeBuilder.EntityType.MakeDefinition());
                        InstanceCache.GetOrAdd(type, instance);
                    }
                }
            }

            return instance;
        }

        public static void UseBuilders(params IEntityTypeBuilder[] entityTypeBuilders)
        {
            Utils.CheckNull(entityTypeBuilders, nameof(entityTypeBuilders));

            Configure(entityTypeBuilders.Select(a => a.EntityType.MakeDefinition()).ToArray());
        }
        public static void UseBuilders(params Type[] entityTypeBuilderTypes)
        {
            Utils.CheckNull(entityTypeBuilderTypes, nameof(entityTypeBuilderTypes));

            List<TypeDefinition> typeDefinitions = new List<TypeDefinition>(entityTypeBuilderTypes.Length);

            foreach (Type entityTypeBuilderType in entityTypeBuilderTypes)
            {
                IEntityTypeBuilder entityTypeBuilder = Activator.CreateInstance(entityTypeBuilderType) as IEntityTypeBuilder;
                typeDefinitions.Add(entityTypeBuilder.EntityType.MakeDefinition());
            }

            Configure(typeDefinitions.ToArray());
        }
        public static void Configure(params TypeDefinition[] typeDefinitions)
        {
            Utils.CheckNull(typeDefinitions, nameof(typeDefinitions));

            foreach (var typeDefinition in typeDefinitions)
            {
                InstanceCache.AddOrUpdate(typeDefinition.Type, key =>
                {
                    return new TypeDescriptor(typeDefinition);
                }, (key, oldValue) =>
                {
                    return new TypeDescriptor(typeDefinition);
                });
            }
        }
    }
}
