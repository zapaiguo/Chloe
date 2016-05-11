using Chloe.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Mapper
{
    public class EntityConstructor
    {
        EntityConstructor(ConstructorInfo constructorInfo)
        {
            this.ConstructorInfo = constructorInfo;
            this.Init();
        }

        void Init()
        {
            ConstructorInfo constructor = this.ConstructorInfo;
            Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> fn = DelegateGenerator.CreateObjectGenerator(constructor);
            this.InstanceCreator = fn;
        }

        public ConstructorInfo ConstructorInfo { get; private set; }
        public Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> InstanceCreator { get; private set; }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, EntityConstructor> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, EntityConstructor>();

        public static EntityConstructor GetInstance(ConstructorInfo constructorInfo)
        {
            EntityConstructor instance;
            if (!InstanceCache.TryGetValue(constructorInfo, out instance))
            {
                lock (constructorInfo)
                {
                    if (!InstanceCache.TryGetValue(constructorInfo, out instance))
                    {
                        instance = new EntityConstructor(constructorInfo);
                        InstanceCache.GetOrAdd(constructorInfo, instance);
                    }
                }
            }

            return instance;
        }
    }

    public struct ReaderOrdinalEnumerator
    {
        public static readonly MethodInfo NextMethodInfo;
        static ReaderOrdinalEnumerator()
        {
            MethodInfo method = typeof(ReaderOrdinalEnumerator).GetMethod("Next");
            NextMethodInfo = method;
        }

        List<int> _readerOrdinals;
        int _next;
        public ReaderOrdinalEnumerator(List<int> readerOrdinals)
        {
            this._readerOrdinals = readerOrdinals;
            this._next = 0;
        }
        public int Next()
        {
            int ret = this._readerOrdinals[this._next];
            this._next++;
            return ret;
        }
    }
    public struct ObjectActivatorEnumerator
    {
        List<IObjectActivator> _objectActivators;
        int _next;

        public static readonly MethodInfo NextMethodInfo;
        static ObjectActivatorEnumerator()
        {
            MethodInfo method = typeof(ObjectActivatorEnumerator).GetMethod("Next");
            NextMethodInfo = method;
        }

        public ObjectActivatorEnumerator(List<IObjectActivator> objectActivators)
        {
            this._objectActivators = objectActivators;
            this._next = 0;
        }
        public IObjectActivator Next()
        {
            IObjectActivator ret = this._objectActivators[this._next];
            this._next++;
            return ret;
        }
    }

}
