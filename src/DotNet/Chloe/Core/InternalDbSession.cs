using Chloe.Core;
using Chloe.Exceptions;
using Chloe.Infrastructure.Interception;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Core
{
    class InternalDbSession : IDisposable
    {
        IDbConnection _dbConnection;
        IDbTransaction _dbTransaction;
        bool _isInTransaction;
        int _commandTimeout = 30;/* seconds */

        List<IDbCommandInterceptor> _dbCommandInterceptors;

        bool _disposed = false;

        public InternalDbSession(IDbConnection conn)
        {
            this._dbConnection = conn;
        }

        public bool IsInTransaction { get { return this._isInTransaction; } }
        public int CommandTimeout { get { return this._commandTimeout; } set { this._commandTimeout = value; } }
        public List<IDbCommandInterceptor> DbCommandInterceptors
        {
            get
            {
                if (this._dbCommandInterceptors == null)
                    this._dbCommandInterceptors = new List<IDbCommandInterceptor>();

                return this._dbCommandInterceptors;
            }
        }


        void Activate()
        {
            this.CheckDisposed();

            if (this._dbConnection.State == ConnectionState.Broken)
            {
                this._dbConnection.Close();
            }

            if (this._dbConnection.State == ConnectionState.Closed)
            {
                this._dbConnection.Open();
            }
        }
        /// <summary>
        /// 表示一次查询完成。在事务中的话不关闭连接，交给 CommitTransaction() 或者 RollbackTransaction() 控制，否则调用 IDbConnection.Close() 关闭连接
        /// </summary>
        public void Complete()
        {
            //在事务中的话不关闭连接  交给CommitTransaction()或者RollbackTransaction()
            if (!this._isInTransaction)
            {
                if (this._dbConnection.State == ConnectionState.Open)
                {
                    this._dbConnection.Close();
                }
            }
        }

        public void BeginTransaction()
        {
            this.Activate();
            this._dbTransaction = _dbConnection.BeginTransaction();
            this._isInTransaction = true;
        }
        public void BeginTransaction(IsolationLevel il)
        {
            this.Activate();
            this._dbTransaction = this._dbConnection.BeginTransaction(il);
            this._isInTransaction = true;
        }
        public void CommitTransaction()
        {
            if (!this._isInTransaction)
            {
                throw new ChloeException("Current session does not open a transaction.");
            }
            this._dbTransaction.Commit();
            this.EndTransaction();
        }
        public void RollbackTransaction()
        {
            if (!this._isInTransaction)
            {
                throw new ChloeException("Current session does not open a transaction.");
            }
            this._dbTransaction.Rollback();
            this.EndTransaction();
        }

        public IDataReader ExecuteReader(string cmdText, DbParam[] parameters, CommandType cmdType)
        {
            return this.ExecuteReader(cmdText, parameters, cmdType, CommandBehavior.Default);
        }
        public IDataReader ExecuteReader(string cmdText, DbParam[] parameters, CommandType cmdType, CommandBehavior behavior)
        {
            this.CheckDisposed();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(AppendDbCommandInfo(cmdText, parameters));
#endif

            List<OutputParameter> outputParameters;
            IDbCommand cmd = this.PrepareCommand(cmdText, parameters, cmdType, out outputParameters);
            DbCommandInterceptionContext<IDataReader> dbCommandInterceptionContext = new DbCommandInterceptionContext<IDataReader>();

            this.Activate();
            this.OnReaderExecuting(cmd, dbCommandInterceptionContext);

            IDataReader reader;
            try
            {
                reader = new InternalDataReader(this, cmd.ExecuteReader(behavior), cmd, outputParameters);
            }
            catch (Exception ex)
            {
                dbCommandInterceptionContext.Exception = ex;
                this.OnReaderExecuted(cmd, dbCommandInterceptionContext);

                throw WrapException(ex);
            }

            dbCommandInterceptionContext.Result = reader;
            this.OnReaderExecuted(cmd, dbCommandInterceptionContext);

            return reader;
        }
        public int ExecuteNonQuery(string cmdText, DbParam[] parameters, CommandType cmdType)
        {
            this.CheckDisposed();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(AppendDbCommandInfo(cmdText, parameters));
#endif

            IDbCommand cmd = null;
            try
            {
                List<OutputParameter> outputParameters;
                cmd = this.PrepareCommand(cmdText, parameters, cmdType, out outputParameters);
                DbCommandInterceptionContext<int> dbCommandInterceptionContext = new DbCommandInterceptionContext<int>();

                this.Activate();
                this.OnNonQueryExecuting(cmd, dbCommandInterceptionContext);

                int affectedRows;
                try
                {
                    affectedRows = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    dbCommandInterceptionContext.Exception = ex;
                    this.OnNonQueryExecuted(cmd, dbCommandInterceptionContext);

                    throw WrapException(ex);
                }

                dbCommandInterceptionContext.Result = affectedRows;
                this.OnNonQueryExecuted(cmd, dbCommandInterceptionContext);
                OutputParameter.CallMapValue(outputParameters);

                return affectedRows;
            }
            finally
            {
                this.Complete();
                if (cmd != null)
                    cmd.Dispose();
            }
        }
        public object ExecuteScalar(string cmdText, DbParam[] parameters, CommandType cmdType)
        {
            this.CheckDisposed();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(AppendDbCommandInfo(cmdText, parameters));
#endif

            IDbCommand cmd = null;
            try
            {
                List<OutputParameter> outputParameters;
                cmd = this.PrepareCommand(cmdText, parameters, cmdType, out outputParameters);
                DbCommandInterceptionContext<object> dbCommandInterceptionContext = new DbCommandInterceptionContext<object>();

                this.Activate();
                this.OnScalarExecuting(cmd, dbCommandInterceptionContext);

                object ret;
                try
                {
                    ret = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    dbCommandInterceptionContext.Exception = ex;
                    this.OnScalarExecuted(cmd, dbCommandInterceptionContext);

                    throw WrapException(ex);
                }

                dbCommandInterceptionContext.Result = ret;
                this.OnScalarExecuted(cmd, dbCommandInterceptionContext);
                OutputParameter.CallMapValue(outputParameters);

                return ret;
            }
            finally
            {
                this.Complete();
                if (cmd != null)
                    cmd.Dispose();
            }
        }


        public void Dispose()
        {
            if (this._disposed)
                return;

            if (this._dbTransaction != null)
            {
                if (this._isInTransaction)
                {
                    try
                    {
                        this._dbTransaction.Rollback();
                    }
                    catch
                    {
                    }
                }

                this._dbTransaction.Dispose();
                this._dbTransaction = null;
                this._isInTransaction = false;
            }

            if (this._dbConnection != null)
            {
                this._dbConnection.Dispose();
            }

            this._disposed = true;
        }

        IDbCommand PrepareCommand(string cmdText, DbParam[] parameters, CommandType cmdType, out List<OutputParameter> outputParameters)
        {
            outputParameters = null;

            IDbCommand cmd = this._dbConnection.CreateCommand();

            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = this._commandTimeout;
            if (this.IsInTransaction)
                cmd.Transaction = this._dbTransaction;

            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    if (param == null)
                        continue;

                    if (param.ExplicitParameter != null)/* 如果存在创建好了的 IDbDataParameter，则直接用它。同时也忽视了 DbParam 的其他属性 */
                    {
                        cmd.Parameters.Add(param.ExplicitParameter);
                        continue;
                    }

                    IDbDataParameter parameter = cmd.CreateParameter();
                    parameter.ParameterName = param.Name;

                    Type parameterType;
                    if (param.Value == null || param.Value == DBNull.Value)
                    {
                        parameter.Value = DBNull.Value;
                        parameterType = param.Type;
                    }
                    else
                    {
                        parameter.Value = param.Value;
                        parameterType = param.Value.GetType();
                    }

                    if (param.Precision != null)
                        parameter.Precision = param.Precision.Value;

                    if (param.Scale != null)
                        parameter.Scale = param.Scale.Value;

                    if (param.Size != null)
                        parameter.Size = param.Size.Value;

                    DbType? dbType = Utils.TryGetDbType(parameterType);
                    if (dbType != null)
                        parameter.DbType = dbType.Value;

                    const int defaultSizeOfStringOutputParameter = 8000;/* 当一个 string 类型输出参数未显示指定 Size 时使用的默认大小。如果有需要更大或者该值不足以满足需求，需显示指定 DbParam.Size 值 */

                    OutputParameter outputParameter = null;
                    if (param.Direction == ParamDirection.Input)
                    {
                        parameter.Direction = ParameterDirection.Input;
                    }
                    else if (param.Direction == ParamDirection.Output)
                    {
                        parameter.Direction = ParameterDirection.Output;
                        param.Value = null;
                        if (param.Size == null && param.Type == UtilConstants.TypeOfString)
                        {
                            parameter.Size = defaultSizeOfStringOutputParameter;
                        }
                        outputParameter = new OutputParameter(param, parameter);
                    }
                    else if (param.Direction == ParamDirection.InputOutput)
                    {
                        parameter.Direction = ParameterDirection.InputOutput;
                        if (param.Size == null && param.Type == UtilConstants.TypeOfString)
                        {
                            parameter.Size = defaultSizeOfStringOutputParameter;
                        }
                        outputParameter = new OutputParameter(param, parameter);
                    }
                    else
                        throw new NotSupportedException(string.Format("ParamDirection '{0}' is not supported.", param.Direction));

                    cmd.Parameters.Add(parameter);

                    if (outputParameter != null)
                    {
                        if (outputParameters == null)
                            outputParameters = new List<OutputParameter>();
                        outputParameters.Add(outputParameter);
                    }
                }
            }

            return cmd;
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
            var globalInterceptorEnumerator = DbInterception.GetEnumerator();
            while (globalInterceptorEnumerator.MoveNext())
            {
                act(globalInterceptorEnumerator.Current);
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


        void EndTransaction()
        {
            this._dbTransaction.Dispose();
            this._isInTransaction = false;
            this.Complete();
        }


        void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }


        public static string AppendDbCommandInfo(string cmdText, DbParam[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (parameters != null)
            {
                foreach (var param in parameters)
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
            Type unType;
            if (Utils.IsNullable(type, out unType))
            {
                return string.Format("Nullable<{0}>", GetTypeName(unType));
            }

            return type.Name;
        }

        static ChloeException WrapException(Exception ex)
        {
            return new ChloeException("An exception occurred while executing DbCommand.For details please see the inner exception.", ex);
        }
    }
}
