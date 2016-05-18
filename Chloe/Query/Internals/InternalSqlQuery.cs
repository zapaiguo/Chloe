using Chloe.Core;
using Chloe.Descriptors;
using Chloe.Mapper;
using Chloe.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Chloe.Query.Internals
{
    class InternalSqlQuery<T> : IEnumerable<T>, IEnumerable
    {
        InternalDbSession _dbSession;
        string _sql;
        IDictionary<string, object> _parameters;

        public InternalSqlQuery(InternalDbSession dbSession, string sql, IDictionary<string, object> parameters)
        {
            this._dbSession = dbSession;
            this._sql = sql;
            this._parameters = parameters;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new QueryEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        struct QueryEnumerator : IEnumerator<T>
        {
            InternalSqlQuery<T> _internalSqlQuery;

            IDataReader _reader;
            IObjectActivator _objectActivator;

            T _current;
            bool _hasFinished;
            bool _disposed;
            public QueryEnumerator(InternalSqlQuery<T> internalSqlQuery)
            {
                this._internalSqlQuery = internalSqlQuery;
                this._reader = null;
                this._objectActivator = null;

                this._current = default(T);
                this._hasFinished = false;
                this._disposed = false;
            }

            public T Current { get { return this._current; } }

            object IEnumerator.Current { get { return this._current; } }

            public bool MoveNext()
            {
                if (this._hasFinished || this._disposed)
                    return false;

                if (this._reader == null)
                {
                    this.Prepare();
                }

                if (this._reader.Read())
                {
                    this._current = (T)this._objectActivator.CreateInstance(this._reader);
                    return true;
                }
                else
                {
                    this._reader.Close();
                    this._internalSqlQuery._dbSession.Complete();
                    this._current = default(T);
                    this._hasFinished = true;
                    return false;
                }
            }

            public void Dispose()
            {
                if (this._disposed)
                    return;

                if (this._reader != null)
                {
                    if (!this._reader.IsClosed)
                        this._reader.Close();
                    this._reader.Dispose();
                    this._reader = null;
                }

                if (!this._hasFinished)
                {
                    this._internalSqlQuery._dbSession.Complete();
                    this._hasFinished = true;
                }

                this._current = default(T);
                this._disposed = true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            void Prepare()
            {
                Type type = typeof(T);

                EntityConstructorDescriptor constructorDescriptor = EntityConstructorDescriptor.GetInstance(type.GetConstructor(Type.EmptyTypes));
                EntityMemberMapper mapper = constructorDescriptor.GetEntityMemberMapper();
                Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> instanceCreator = constructorDescriptor.GetInstanceCreator();

                this._reader = this._internalSqlQuery._dbSession.ExecuteReader(this._internalSqlQuery._sql, this._internalSqlQuery._parameters, CommandBehavior.Default, CommandType.Text);

#if DEBUG
                Debug.WriteLine(this._internalSqlQuery._sql);
#endif

                this._objectActivator = TryGetObjectActivator(type, this._reader, mapper, instanceCreator, this._internalSqlQuery._sql) ?? GetObjectActivator(type, this._reader, mapper, instanceCreator);
            }

            static ObjectActivator GetObjectActivator(Type type, IDataReader reader, EntityMemberMapper mapper, Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> instanceCreator)
            {
                List<IValueSetter> memberSetters = PrepareValueSetters(type, reader, mapper);
                return new ObjectActivator(instanceCreator, null, null, memberSetters, null);
            }
            static List<IValueSetter> PrepareValueSetters(Type type, IDataReader reader, EntityMemberMapper mapper)
            {
                List<IValueSetter> memberSetters = new List<IValueSetter>(reader.FieldCount);

                MemberInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                MemberInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField);
                List<MemberInfo> members = new List<MemberInfo>(properties.Length + fields.Length);
                members.AddRange(properties);
                members.AddRange(fields);

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    var member = members.Where(a => a.Name == name).FirstOrDefault();
                    if (member == null)
                    {
                        member = members.Where(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (member == null)
                            continue;
                    }

                    IMRM mMapper = mapper.GetMemberMapper(member);
                    if (mMapper == null)
                        continue;

                    MappingMemberBinder memberBinder = new MappingMemberBinder(mMapper, i);
                    memberSetters.Add(memberBinder);
                }

                return memberSetters;
            }

            static ObjectActivator TryGetObjectActivator(Type type, IDataReader reader, EntityMemberMapper mapper, Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> instanceCreator, string sql)
            {
                ObjectActivator activator;
                Dictionary<string, List<CacheInfo>> dic;
                if (!ObjectActivatorCache.TryGetValue(type, out dic))
                {
                    if (!Monitor.TryEnter(type))
                        return null;

                    try
                    {
                        if (!ObjectActivatorCache.TryGetValue(type, out dic))
                        {
                            dic = new Dictionary<string, List<CacheInfo>>(1);
                            ObjectActivatorCache.TryAdd(type, dic);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(type);
                    }
                }

                List<CacheInfo> caches;
                if (!dic.TryGetValue(sql, out caches))
                {
                    lock (dic)
                    {
                        if (!dic.TryGetValue(sql, out caches))
                        {
                            caches = new List<CacheInfo>(1);
                            dic[sql] = caches;
                        }
                    }
                }

                CacheInfo cache = null;
                for (int i = 0; i < caches.Count; i++)
                {
                    var item = caches[i];
                    if (item.IsTheSameFieldTypes(reader))
                    {
                        cache = item;
                        break;
                    }
                }

                if (cache != null)
                    return cache.ObjectActivator;

                activator = GetObjectActivator(type, reader, mapper, instanceCreator);
                cache = new CacheInfo(activator, reader);
                caches.Add(cache);
                caches.TrimExcess();

                return cache.ObjectActivator;
            }
            static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Dictionary<string, List<CacheInfo>>> ObjectActivatorCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, Dictionary<string, List<CacheInfo>>>();
        }

        public class CacheInfo
        {
            public CacheInfo(ObjectActivator activator, IDataReader reader)
            {
                this.ObjectActivator = activator;

                var readerFieldTypes = new Type[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    readerFieldTypes[i] = reader.GetFieldType(i);
                }

                this.ReaderFieldTypes = readerFieldTypes;
            }

            public Type[] ReaderFieldTypes { get; private set; }
            public ObjectActivator ObjectActivator { get; private set; }

            public bool IsTheSameFieldTypes(IDataReader reader)
            {
                if (reader.FieldCount != this.ReaderFieldTypes.Length)
                    return false;

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetFieldType(i) != this.ReaderFieldTypes[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

    }

}
