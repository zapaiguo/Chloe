
namespace System.Data
{
    public static class DbConnectionExtension
    {
        public static object ExecuteScalar(this IDbConnection conn, string cmdText, params DataParam[] dbParams)
        {
            return conn.ExecuteScalar(cmdText, CommandType.Text, dbParams);
        }
        public static object ExecuteScalar(this IDbConnection conn, string cmdText, CommandType cmdType, params DataParam[] dbParams)
        {
            return conn.ExecuteScalar(cmdText, cmdType, null, dbParams);
        }
        public static object ExecuteScalar(this IDbConnection conn, string cmdText, IDbTransaction tran, params DataParam[] dbParams)
        {
            return conn.ExecuteScalar(cmdText, CommandType.Text, tran, dbParams);
        }
        public static object ExecuteScalar(this IDbConnection conn, string cmdText, CommandType cmdType, IDbTransaction tran, params DataParam[] dbParams)
        {
            return conn.Execute(cmdText, cmdType, tran, dbParams, cmd =>
            {
                return cmd.ExecuteScalar();
            });
        }

        public static int ExecuteNonQuery(this IDbConnection conn, string cmdText, params DataParam[] dbParams)
        {
            return conn.ExecuteNonQuery(cmdText, CommandType.Text, dbParams);
        }
        public static int ExecuteNonQuery(this IDbConnection conn, string cmdText, CommandType cmdType, params DataParam[] dbParams)
        {
            return conn.ExecuteNonQuery(cmdText, cmdType, null, dbParams);
        }
        public static int ExecuteNonQuery(this IDbConnection conn, string cmdText, IDbTransaction tran, params DataParam[] dbParams)
        {
            return conn.ExecuteNonQuery(cmdText, CommandType.Text, tran, dbParams);
        }
        public static int ExecuteNonQuery(this IDbConnection conn, string cmdText, CommandType cmdType, IDbTransaction tran, params DataParam[] dbParams)
        {
            return conn.Execute(cmdText, cmdType, tran, dbParams, cmd =>
            {
                return cmd.ExecuteNonQuery();
            });
        }

        public static IDataReader ExecuteReader(this IDbConnection conn, string cmdText, params DataParam[] dbParams)
        {
            return conn.ExecuteReader(cmdText, CommandType.Text, dbParams);
        }
        public static IDataReader ExecuteReader(this IDbConnection conn, string cmdText, CommandType cmdType, params DataParam[] dbParams)
        {
            return conn.ExecuteReader(cmdText, cmdType, null, dbParams);
        }
        public static IDataReader ExecuteReader(this IDbConnection conn, string cmdText, IDbTransaction tran, params DataParam[] dbParams)
        {
            return conn.ExecuteReader(cmdText, CommandType.Text, tran, dbParams);
        }
        public static IDataReader ExecuteReader(this IDbConnection conn, string cmdText, CommandType cmdType, IDbTransaction tran, params DataParam[] dbParams)
        {
            if (conn.State != ConnectionState.Open)
                throw new Exception("调用 ExecuteReader 请先确保 conn 保持 Open 状态");

            return conn.Execute(cmdText, cmdType, tran, dbParams, cmd =>
            {
                return cmd.ExecuteReader();
            });
        }

        static T Execute<T>(this IDbConnection conn, string cmdText, CommandType cmdType, IDbTransaction tran, DataParam[] dbParams, Func<IDbCommand, T> action)
        {
            bool shouldCloseConnection = false;

            try
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    SetupCommand(cmd, cmdText, cmdType, dbParams, tran);

                    if (conn.State != ConnectionState.Open)
                    {
                        shouldCloseConnection = true;
                        conn.Open();
                    }

                    return action(cmd);
                }
            }
            finally
            {
                if (shouldCloseConnection && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        static void SetupCommand(IDbCommand cmd, string cmdText, CommandType cmdType, DataParam[] dbParams, IDbTransaction tran)
        {
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;

            if (tran != null)
            {
                cmd.Transaction = tran;
            }

            if (dbParams != null)
            {
                foreach (var dbParam in dbParams)
                {
                    IDbDataParameter dataParameter = cmd.CreateParameter();
                    dataParameter.Value = dbParam.Value;
                    cmd.Parameters.Add(dataParameter);
                }
            }
        }
    }
    public class DataParam
    {
        string _name;
        object _value;
        Type _type;

        public DataParam()
        {
        }
        public DataParam(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
        public DataParam(string name, object value, Type type)
        {
            this.Name = name;
            this.Value = value;
            this.Type = type;
        }

        public string Name { get { return this._name; } set { this._name = value; } }
        public object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                if (value != null)
                    this._type = value.GetType();
            }
        }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }
        public int? Size { get; set; }
        public Type Type { get { return this._type; } set { this._type = value; } }

        public static DataParam Create<T>(string name, T value)
        {
            var param = new DataParam(name, value);
            if (value == null)
                param.Type = typeof(T);
            return param;
        }
        public static DataParam Create(string name, object value)
        {
            return new DataParam(name, value);
        }
        public static DataParam Create(string name, object value, Type type)
        {
            return new DataParam(name, value, type);
        }
    }

    public static class DataReaderExtension
    {
        public static DbValue GetDbValue(this IDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.GetDbValue(ordinal);
        }
        public static DbValue GetDbValue(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return new DbValue();
            }

            return new DbValue(reader.GetValue(ordinal));
        }
    }
    public class DbValue
    {
        object _value;

        public DbValue()
        {
        }
        public DbValue(object value)
        {
            if (value == DBNull.Value)
                this._value = null;
            else
                this._value = value;
        }

        public object Value { get { return this._value; } }

        public static implicit operator byte(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(byte))
                return (byte)dbValue._value;

            return Convert.ToByte(dbValue._value);
        }
        public static implicit operator byte?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            byte ret = dbValue;
            return ret;
        }
        public static implicit operator short(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(short))
                return (short)dbValue._value;

            return Convert.ToInt16(dbValue._value);
        }
        public static implicit operator short?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            short ret = dbValue;
            return ret;
        }
        public static implicit operator int(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(int))
                return (int)dbValue._value;

            return Convert.ToInt32(dbValue._value);
        }
        public static implicit operator int?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            int ret = dbValue;
            return ret;
        }
        public static implicit operator long(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(long))
                return (long)dbValue._value;

            return Convert.ToInt64(dbValue._value);
        }
        public static implicit operator long?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            long ret = dbValue;
            return ret;
        }

        public static implicit operator float(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(float))
                return (float)dbValue._value;

            return Convert.ToSingle(dbValue._value);
        }
        public static implicit operator float?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            float ret = dbValue;
            return ret;
        }
        public static implicit operator double(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(double))
                return (double)dbValue._value;

            return Convert.ToDouble(dbValue._value);
        }
        public static implicit operator double?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            double ret = dbValue;
            return ret;
        }
        public static implicit operator decimal(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(decimal))
                return (decimal)dbValue._value;

            return Convert.ToDecimal(dbValue._value);
        }
        public static implicit operator decimal?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            decimal ret = dbValue;
            return ret;
        }

        public static implicit operator Guid(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            var valType = dbValue._value.GetType();

            if (valType == typeof(Guid))
                return (Guid)dbValue._value;

            if (valType == typeof(string))
                return Guid.Parse((string)dbValue._value);

            if (valType == typeof(byte[]))
                return new Guid((byte[])dbValue._value);

            throw new InvalidCastException();
        }
        public static implicit operator Guid?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            Guid ret = dbValue;
            return ret;
        }

        public static implicit operator DateTime(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(DateTime))
                return (DateTime)dbValue._value;

            return Convert.ToDateTime(dbValue._value);
        }
        public static implicit operator DateTime?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            DateTime ret = dbValue;
            return ret;
        }

        public static implicit operator bool(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(bool))
                return (bool)dbValue._value;

            return Convert.ToBoolean(dbValue._value);
        }
        public static implicit operator bool?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            bool ret = dbValue;
            return ret;
        }

        public static implicit operator char(DbValue dbValue)
        {
            EnsureNullable(dbValue);

            if (dbValue._value.GetType() == typeof(char))
                return (char)dbValue._value;

            return Convert.ToChar(dbValue._value);
        }
        public static implicit operator char?(DbValue dbValue)
        {
            if (dbValue._value == null)
                return null;

            char ret = dbValue;
            return ret;
        }
        public static implicit operator string(DbValue dbValue)
        {
            return (string)dbValue._value;
        }


        static void EnsureNullable(DbValue dbValue)
        {
            if (dbValue._value == null || dbValue._value == DBNull.Value)
                throw new InvalidCastException();
        }
    }
}
