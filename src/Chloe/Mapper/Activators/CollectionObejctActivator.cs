using Chloe.Core.Emit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

namespace Chloe.Mapper.Activators
{
    public class CollectionObejctActivator : IObjectActivator
    {
        Type _collectionType;
        Func<object> _activator;

        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object>> ActivatorCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object>>();

        static Func<object> GetActivator(Type collectionType)
        {
            Func<object> activator = ActivatorCache.GetOrAdd(collectionType, type =>
           {
               var typeDefinition = type.GetGenericTypeDefinition();
               var listType = typeof(List<>);
               if (typeDefinition.IsAssignableFrom(listType))
               {
                   return DelegateGenerator.CreateInstanceActivator(listType.MakeGenericType(type.GetGenericArguments()[0]));
               }

               var cType = typeof(Collection<>);
               if (typeDefinition.IsAssignableFrom(cType))
               {
                   return DelegateGenerator.CreateInstanceActivator(cType.MakeGenericType(type.GetGenericArguments()[0]));
               }

               throw new NotSupportedException($"Not supported collection type '{type.Name}'");
           });

            return activator;
        }

        public CollectionObejctActivator(Type collectionType)
        {
            this._collectionType = collectionType;
            this._activator = GetActivator(collectionType);
        }

        public object CreateInstance(IDataReader reader)
        {
            return this._activator();
        }
    }
}
