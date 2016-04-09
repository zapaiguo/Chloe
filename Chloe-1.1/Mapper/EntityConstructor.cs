using Chloe.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivtorEnumerator, object> fn = DelegateCreateManage.CreateObjectGenerator(this.ConstructorInfo);
            this.InstanceCreator = fn;
            //throw new NotImplementedException();
        }

        public ConstructorInfo ConstructorInfo { get; private set; }
        public Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivtorEnumerator, object> InstanceCreator { get; private set; }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, EntityConstructor> _instanceCache = new System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, EntityConstructor>();

        public static EntityConstructor GetInstance(ConstructorInfo constructorInfo)
        {
            EntityConstructor instance;
            if (!_instanceCache.TryGetValue(constructorInfo, out instance))
            {
                instance = new EntityConstructor(constructorInfo);
                instance = _instanceCache.GetOrAdd(constructorInfo, instance);
            }

            return instance;
        }

        static object T(IDataReader reader, ReaderOrdinalEnumerator roe, ObjectActivtorEnumerator oae)
        {
            //reader.GetInt32(roe.Next());
            //reader.GetString(roe.Next());
            //oae.Next().CreateInstance(reader);//as T
            User u = new User(reader.GetInt32(roe.Next()), reader.GetString(roe.Next()), oae.Next().CreateInstance(reader) as User);

            return u;
        }

        class User
        {
            public User(int id, string name, User u)
            {

            }
            public int Id { get; set; }
            public string Name { get; set; }
            public User U { get; set; }
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
            if (this._next > this._readerOrdinals.Count - 1)
                throw new Exception();

            int ret = this._readerOrdinals[this._next];
            this._next++;
            return ret;
        }
    }
    public struct ObjectActivtorEnumerator
    {
        List<IObjectActivtor> _objectActivtors;
        int _next;

        public static readonly MethodInfo NextMethodInfo;
        static ObjectActivtorEnumerator()
        {
            MethodInfo method = typeof(ObjectActivtorEnumerator).GetMethod("Next");
            NextMethodInfo = method;
        }

        public ObjectActivtorEnumerator(List<IObjectActivtor> objectActivtors)
        {
            this._objectActivtors = objectActivtors;
            this._next = 0;
        }
        public IObjectActivtor Next()
        {
            if (this._next > this._objectActivtors.Count - 1)
                throw new Exception();

            IObjectActivtor ret = this._objectActivtors[this._next];
            this._next++;
            return ret;
        }
    }

}
