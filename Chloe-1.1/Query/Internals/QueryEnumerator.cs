using Chloe.Database;
using Chloe.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Internals
{
    static class QueryEnumeratorCreator
    {
        public static IEnumerator<T> CreateEnumerator<T>(InternalDbSession dbSession, QueryFactor queryFactor)
        {
            return new QueryEnumerator<T>(dbSession, queryFactor);
        }
        internal struct QueryEnumerator<T> : IEnumerator<T>
        {
            InternalDbSession _dbSession;
            QueryFactor _queryFactor;
            IObjectActivtor _objectActivtor;

            IDataReader _reader;

            T _current;
            bool _hasFinished;
            bool _disposed;
            public QueryEnumerator(InternalDbSession dbSession, QueryFactor queryFactor)
            {
                this._dbSession = dbSession;
                this._queryFactor = queryFactor;
                this._objectActivtor = queryFactor.ObjectActivtor;

                this._reader = null;

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
                    //TODO 执行 sql 语句，获取 DataReader
                    this._reader = this._dbSession.ExecuteReader(this._queryFactor.CmdText, this._queryFactor.Parameters, CommandBehavior.Default, CommandType.Text);
                }

                if (this._reader.Read())
                {
                    this._current = (T)this._objectActivtor.CreateInstance(this._reader);
                    return true;
                }
                else
                {
                    this._reader.Close();
                    this._dbSession.Complete();
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
                    this._dbSession.Complete();
                    this._hasFinished = true;
                }

                this._current = default(T);
                this._disposed = true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }
    }
}
