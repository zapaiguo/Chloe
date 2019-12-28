using Chloe.Core;
using Chloe.Core.Emit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Mapper
{
    public class ObjectConstructor
    {
        ObjectConstructor(ConstructorInfo constructorInfo)
        {
            if (constructorInfo.DeclaringType.IsAbstract)
                throw new ArgumentException("The type can not be abstract class.");

            this.ConstructorInfo = constructorInfo;
            this.Init();
        }

        void Init()
        {
            ConstructorInfo constructor = this.ConstructorInfo;
            InstanceCreator instanceCreator = DelegateGenerator.CreateObjectGenerator(constructor);
            this.InstanceCreator = instanceCreator;
        }

        public ConstructorInfo ConstructorInfo { get; private set; }
        public InstanceCreator InstanceCreator { get; private set; }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, ObjectConstructor> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, ObjectConstructor>();

        public static ObjectConstructor GetInstance(ConstructorInfo constructorInfo)
        {
            ObjectConstructor instance;
            if (!InstanceCache.TryGetValue(constructorInfo, out instance))
            {
                lock (constructorInfo)
                {
                    if (!InstanceCache.TryGetValue(constructorInfo, out instance))
                    {
                        instance = new ObjectConstructor(constructorInfo);
                        InstanceCache.GetOrAdd(constructorInfo, instance);
                    }
                }
            }

            return instance;
        }
    }
}
