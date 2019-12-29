using Chloe.Core;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using Chloe.Infrastructure.Interception;
using Chloe.InternalExtensions;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Data
{
    class InnerAdoSession : IDisposable
    {
        IAdoSession _adoSession;
        IAdoSession _internalAdoSession;
        IAdoSession _externalAdoSession;

        List<IDbCommandInterceptor> _dbCommandInterceptors;
        IDbCommandInterceptor[] _globalInterceptors;

        public InnerAdoSession(IDbConnection conn)
        {
            this._internalAdoSession = new InternalAdoSession(conn);
            this._adoSession = this._internalAdoSession;
            this.InitEvents(this._internalAdoSession);
        }
        void InitEvents(IAdoSession adoSession)
        {
            adoSession.OnReaderExecuting += this.OnReaderExecuting;
            adoSession.OnReaderExecuted += this.OnReaderExecuted;
            adoSession.OnNonQueryExecuting += this.OnNonQueryExecuting;
            adoSession.OnNonQueryExecuted += this.OnNonQueryExecuted;
            adoSession.OnScalarExecuting += this.OnScalarExecuting;
            adoSession.OnScalarExecuted += this.OnScalarExecuted;
        }

        public IDbConnection DbConnection { get { return this._adoSession.DbConnection; } }
        public IDbTransaction DbTransaction { get { return this._adoSession.DbTransaction; } }
        public bool IsInTransaction { get { return this._adoSession.IsInTransaction; } }
        public int CommandTimeout { get { return this._adoSession.CommandTimeout; } set { this._adoSession.CommandTimeout = value; } }
        public List<IDbCommandInterceptor> DbCommandInterceptors
        {
            get
            {
                if (this._dbCommandInterceptors == null)
                    this._dbCommandInterceptors = new List<IDbCommandInterceptor>();

                return this._dbCommandInterceptors;
            }
        }
        IDbCommandInterceptor[] GlobalInterceptors
        {
            get
            {
                if (this._globalInterceptors == null)
                    this._globalInterceptors = DbInterception.GetInterceptors();

                return this._globalInterceptors;
            }
        }

        /// <summary>
        /// 使用外部事务。
        /// </summary>
        /// <param name="dbTransaction"></param>
        public void UseExternalTransaction(IDbTransaction dbTransaction)
        {
            if (dbTransaction == null)
            {
                this._adoSession = this._internalAdoSession;
                this._externalAdoSession = null;
                return;
            }


            if (this._adoSession == this._internalAdoSession && this._internalAdoSession.IsInTransaction)
            {
                throw new NotSupportedException("当前回话已经开启事务，已开启的事务未提交或回滚前无法使用外部事务。");
            }
            if (this._externalAdoSession != null)
            {
                throw new NotSupportedException("当前回话已经使用了一个外部事务，无法再次使用另一个外部事务。");
            }

            this._externalAdoSession = new ExternalAdoSession(dbTransaction);
            this._adoSession = this._externalAdoSession;
            this.InitEvents(this._externalAdoSession);
        }

        public void BeginTransaction(IsolationLevel? il)
        {
            this._adoSession.BeginTransaction(il);
        }
        public void CommitTransaction()
        {
            this._adoSession.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            this._adoSession.RollbackTransaction();
        }

        public IDataReader ExecuteReader(string cmdText, DbParam[] parameters, CommandType cmdType)
        {
            return this._adoSession.ExecuteReader(cmdText, parameters, cmdType);
        }
        public IDataReader ExecuteReader(string cmdText, DbParam[] parameters, CommandType cmdType, CommandBehavior behavior)
        {
            return this._adoSession.ExecuteReader(cmdText, parameters, cmdType, behavior);
        }
        public int ExecuteNonQuery(string cmdText, DbParam[] parameters, CommandType cmdType)
        {
            return this._adoSession.ExecuteNonQuery(cmdText, parameters, cmdType);
        }
        public object ExecuteScalar(string cmdText, DbParam[] parameters, CommandType cmdType)
        {
            return this._adoSession.ExecuteScalar(cmdText, parameters, cmdType);
        }

        public void Dispose()
        {
            this._internalAdoSession.Dispose();
        }

        #region DbInterception
        void OnReaderExecuting(IDbCommand cmd, DbCommandInterceptionContext<IDataReader> dbCommandInterceptionContext)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.ReaderExecuting(cmd, dbCommandInterceptionContext);
            });
        }
        void OnReaderExecuted(IDbCommand cmd, DbCommandInterceptionContext<IDataReader> dbCommandInterceptionContext)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.ReaderExecuted(cmd, dbCommandInterceptionContext);
            });
        }
        void OnNonQueryExecuting(IDbCommand cmd, DbCommandInterceptionContext<int> dbCommandInterceptionContext)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.NonQueryExecuting(cmd, dbCommandInterceptionContext);
            });
        }
        void OnNonQueryExecuted(IDbCommand cmd, DbCommandInterceptionContext<int> dbCommandInterceptionContext)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.NonQueryExecuted(cmd, dbCommandInterceptionContext);
            });
        }
        void OnScalarExecuting(IDbCommand cmd, DbCommandInterceptionContext<object> dbCommandInterceptionContext)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.ScalarExecuting(cmd, dbCommandInterceptionContext);
            });
        }
        void OnScalarExecuted(IDbCommand cmd, DbCommandInterceptionContext<object> dbCommandInterceptionContext)
        {
            this.ExecuteDbCommandInterceptors((dbCommandInterceptor) =>
            {
                dbCommandInterceptor.ScalarExecuted(cmd, dbCommandInterceptionContext);
            });
        }

        void ExecuteDbCommandInterceptors(Action<IDbCommandInterceptor> act)
        {
            IDbCommandInterceptor[] globalInterceptors = this.GlobalInterceptors;
            for (int i = 0; i < globalInterceptors.Length; i++)
            {
                act(globalInterceptors[i]);
            }

            if (this._dbCommandInterceptors != null)
            {
                for (int i = 0; i < this._dbCommandInterceptors.Count; i++)
                {
                    act(this._dbCommandInterceptors[i]);
                }
            }
        }
        #endregion

        public static string AppendDbCommandInfo(string cmdText, DbParam[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (parameters != null)
            {
                foreach (DbParam param in parameters)
                {
                    if (param == null)
                        continue;

                    string typeName = null;
                    object value = null;
                    Type parameterType;
                    if (param.Value == null || param.Value == DBNull.Value)
                    {
                        parameterType = param.Type;
                        value = "NULL";
                    }
                    else
                    {
                        value = param.Value;
                        parameterType = param.Value.GetType();

                        if (parameterType == typeof(string) || parameterType == typeof(DateTime))
                            value = "'" + value + "'";
                    }

                    if (parameterType != null)
                        typeName = GetTypeName(parameterType);

                    sb.AppendFormat("{0} {1} = {2};", typeName, param.Name, value);
                    sb.AppendLine();
                }
            }

            sb.AppendLine(cmdText);

            return sb.ToString();
        }
        static string GetTypeName(Type type)
        {
            Type underlyingType;
            if (ReflectionExtension.IsNullable(type, out underlyingType))
            {
                return string.Format("Nullable<{0}>", GetTypeName(underlyingType));
            }

            return type.Name;
        }
    }
}
