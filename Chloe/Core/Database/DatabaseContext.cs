using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Database
{
    public class DatabaseContext : IDisposable
    {
        private IDbConnection _dbConnection;
        private IDbTransaction _dbTransaction;
        private bool _isInTransaction;
        private bool disposabled = false;

        IDbCommand _dbCommand;

        public DatabaseContext(IDbConnection conn)
        {
            this._dbConnection = conn;
        }

        public IDbConnection DbConnection
        {
            get { return _dbConnection; }
        }
        public IDbCommand DbCommand
        {
            get
            {
                if (_dbCommand == null)
                    _dbCommand = this.DbConnection.CreateCommand();
                return _dbCommand;
            }
        }
        public IDbTransaction DbTransaction
        {
            get { return _dbTransaction; }
        }
        public bool IsInTransaction
        {
            get { return _isInTransaction; }
            private set { _isInTransaction = value; }
        }

        public void Activate()
        {
            if (this.disposabled)
            {
                throw new Exception("对象已销毁");
            }
            if (_dbConnection.State == ConnectionState.Broken)
            {
                _dbConnection.Close();
            }

            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }
        }
        public void Complete()
        {
            //在事务中的话不关闭连接  交给CommitTransaction()或者RollbackTransaction()
            if (!_isInTransaction)
            {
                if (_dbConnection.State == ConnectionState.Open)
                {
                    _dbConnection.Close();
                }
            }
        }

        public void BeginTransaction()
        {
            this.Activate();
            _dbTransaction = _dbConnection.BeginTransaction();
            IsInTransaction = true;
        }
        public void BeginTransaction(IsolationLevel il)
        {
            this.Activate();
            _dbTransaction = _dbConnection.BeginTransaction(il);
            IsInTransaction = true;
        }
        public void CommitTransaction()
        {
            if (!IsInTransaction)
            {
                throw new Exception("未在当前上下文中开启事务");
            }
            _dbTransaction.Commit();
            _dbTransaction.Dispose();
            IsInTransaction = false;
            this.Complete();
        }
        public void RollbackTransaction()
        {
            if (!IsInTransaction)
            {
                throw new Exception("未在当前上下文中开启事务");
            }
            _dbTransaction.Rollback();
            _dbTransaction.Dispose();
            IsInTransaction = false;
            this.Complete();
        }

        public IDataReader ExecuteReader(string cmdText, IDictionary<string, object> parameters)
        {
            return this.ExecuteReader(cmdText, parameters, CommandBehavior.Default, CommandType.Text);
        }
        public IDataReader ExecuteReader(string cmdText, IDictionary<string, object> parameters, CommandType cmdType)
        {
            return this.ExecuteReader(cmdText, parameters, CommandBehavior.Default, cmdType);
        }
        public IDataReader ExecuteReader(string cmdText, IDictionary<string, object> parameters, CommandBehavior behavior)
        {
            return this.ExecuteReader(cmdText, parameters, behavior, CommandType.Text);
        }
        public IDataReader ExecuteReader(string cmdText, IDictionary<string, object> parameters, CommandBehavior behavior, CommandType cmdType)
        {
            IDbCommand cmd = this.DbCommand;

            PrepareCommand(cmd, cmdText, parameters, cmdType);

            this.Activate();

            IDataReader reader = cmd.ExecuteReader(behavior);
            cmd.Parameters.Clear();
            return reader;
        }

        public int ExecuteNonQuery(string cmdText, IDictionary<string, object> parameters)
        {
            return this.ExecuteNonQuery(cmdText, parameters, CommandType.Text);
        }
        public int ExecuteNonQuery(string cmdText, IDictionary<string, object> parameters, CommandType cmdType)
        {
            try
            {
                IDbCommand cmd = this.DbCommand;

                PrepareCommand(cmd, cmdText, parameters, cmdType);

                this.Activate();
                int r = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return r;
            }
            finally
            {
                this.Complete();
            }
        }

        public object ExecuteScalar(string cmdText, IDictionary<string, object> parameters)
        {
            return this.ExecuteScalar(cmdText, parameters, CommandType.Text);
        }
        public object ExecuteScalar(string cmdText, IDictionary<string, object> parameters, CommandType cmdType)
        {
            try
            {
                IDbCommand cmd = this.DbCommand;

                PrepareCommand(cmd, cmdText, parameters, cmdType);

                this.Activate();
                object r = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return r;
            }
            finally
            {
                this.Complete();
            }
        }

        public DataTable ExecuteDataTable(string cmdText, IDictionary<string, object> parameters)
        {
            return this.ExecuteDataTable(cmdText, parameters, CommandType.Text);
        }
        public DataTable ExecuteDataTable(string cmdText, IDictionary<string, object> parameters, CommandType cmdType)
        {
            IDataReader reader = null;
            try
            {
                reader = this.ExecuteReader(cmdText, parameters, cmdType);
                return this.FillDataTable(reader);
            }
            finally
            {
                if (reader != null && !reader.IsClosed) reader.Close();
                this.Complete();
            }
        }
        public DataSet ExecuteDataSet(string cmdText, IDictionary<string, object> parameters)
        {
            return this.ExecuteDataSet(cmdText, parameters, CommandType.Text);
        }
        public DataSet ExecuteDataSet(string cmdText, IDictionary<string, object> parameters, CommandType cmdType)
        {
            DataSet ds = null;
            IDataReader reader = null;
            try
            {
                ds = new DataSet();
                reader = this.ExecuteReader(cmdText, parameters, cmdType);

                var dt = this.FillDataTable(reader);
                ds.Tables.Add(dt);

                while (reader.NextResult())
                {
                    dt = this.FillDataTable(reader);
                    ds.Tables.Add(dt);
                }

                return ds;
            }
            finally
            {
                if (reader != null && !reader.IsClosed) reader.Close();
                this.Complete();
            }
        }

        internal IDataReader ExecuteInternalReader(CommandType cmdType, string cmdText, IDictionary<string, object> parameters)
        {
            IDbCommand cmd = this.DbCommand;

            PrepareCommand(cmd, cmdText, parameters, cmdType);

            this.Activate();

            IDataReader reader = cmd.ExecuteReader();
            cmd.Parameters.Clear();

            return new InternalDataReader(this, reader);
        }
        public void Dispose()
        {
            if (this._isInTransaction)
            {
                _dbTransaction.Rollback();
                _dbTransaction.Dispose();
                _isInTransaction = false;
            }

            this.Complete();

            this._dbCommand.Dispose();
            this._dbCommand = null;
            this._dbTransaction = null;
            //this._connection = null;
            disposabled = true;
        }


        private void PrepareCommand(IDbCommand cmd, string cmdText, IDictionary<string, object> parameters, CommandType cmdType)
        {
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;

            if (this.IsInTransaction)
                cmd.Transaction = this.DbTransaction;

            if (parameters != null)
            {
                foreach (var kv in parameters)
                {
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = kv.Key;
                    parameter.Value = kv.Value == null ? DBNull.Value : kv.Value;
                    cmd.Parameters.Add(parameter);
                }
            }
        }
        private DataTable FillDataTable(IDataReader reader)
        {
            DataTable dt = new DataTable();
            int fieldCount = reader.FieldCount;
            for (int i = 0; i < fieldCount; i++)
            {
                DataColumn dc = new DataColumn(reader.GetName(i), reader.GetFieldType(i));
                dt.Columns.Add(dc);
            }
            while (reader.Read())
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < fieldCount; i++)
                {
                    dr[i] = reader[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

    }
}
