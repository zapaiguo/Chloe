using Chloe.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Mapper
{
    public class MappingTypeConstructor
    {
        MappingTypeConstructor(Type type)
        {
            this.Type = type;
            this.Init();
        }

        void Init()
        {

            Func<IDataReader, int, object> fn = DelegateGenerator.CreateMappingTypeGenerator(this.Type);
            this.InstanceCreator = fn;
        }

        public Type Type { get; private set; }
        public Func<IDataReader, int, object> InstanceCreator { get; private set; }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, MappingTypeConstructor> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, MappingTypeConstructor>();

        public static MappingTypeConstructor GetInstance(Type type)
        {
            MappingTypeConstructor instance;
            if (!InstanceCache.TryGetValue(type, out instance))
            {
                instance = new MappingTypeConstructor(type);
                instance = InstanceCache.GetOrAdd(type, instance);
            }

            return instance;
        }
    }
}
