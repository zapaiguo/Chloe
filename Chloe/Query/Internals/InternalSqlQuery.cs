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
using System.Threading.Tasks;

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

                EntityConstructorDescriptor constructorDescriptor = EntityConstructorDescriptor.GetInstance(type.GetConstructor(UtilConstants.EmptyTypeArray));
                EntityMemberMapper mapper = constructorDescriptor.GetEntityMemberMapper();
                Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> instanceCreator = constructorDescriptor.GetInstanceCreator();

                this._reader = this._internalSqlQuery._dbSession.ExecuteReader(this._internalSqlQuery._sql, this._internalSqlQuery._parameters, CommandBehavior.Default, CommandType.Text);

#if DEBUG
                Debug.WriteLine(this._internalSqlQuery._sql);
#endif

                List<IValueSetter> memberSetters = PrepareValueSetters(type, _reader, mapper);
                this._objectActivator = new ObjectActivator(instanceCreator, null, null, memberSetters, null);
            }

            static List<IValueSetter> PrepareValueSetters(Type type, IDataReader reader, EntityMemberMapper mapper)
            {
                List<IValueSetter> memberSetters = new List<IValueSetter>(reader.FieldCount);

                MemberInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                MemberInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField);
                var members = properties.Concat(fields).ToList();
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

                    Action<object, IDataReader, int> setter = mapper.GetMemberSetter(member);
                    if (setter == null)
                        continue;

                    MappingMemberBinder memberBinder = new MappingMemberBinder(setter, i);
                    memberSetters.Add(memberBinder);
                }

                return memberSetters;
            }
        }

    }
}
