using Chloe.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace System.Data
{
    internal static class AdoExtension
    {
        public static async Task OpenAsyncEx(this IDbConnection conn)
        {
            DbConnection dbConnection = conn as DbConnection;
            if (dbConnection != null)
            {
                await dbConnection.OpenAsync();
                return;
            }

            DbConnectionDecorator dbConnectionDecorator = conn as DbConnectionDecorator;
            if (dbConnectionDecorator != null)
            {
                await dbConnectionDecorator.OpenAsync();
                return;
            }

            conn.Open();
        }

        public static async Task<IDataReader> ExecuteReaderAsyncEx(this IDbCommand cmd)
        {
            DbCommand dbCommand = cmd as DbCommand;
            if (dbCommand != null)
            {
                return await dbCommand.ExecuteReaderAsync();
            }

            DbCommandDecorator dbCommandDecorator = cmd as DbCommandDecorator;
            if (dbCommandDecorator != null)
            {
                return await dbCommandDecorator.ExecuteReaderAsync();
            }

            return cmd.ExecuteReader();
        }
        public static async Task<IDataReader> ExecuteReaderAsyncEx(this IDbCommand cmd, CommandBehavior behavior)
        {
            DbCommand dbCommand = cmd as DbCommand;
            if (dbCommand != null)
            {
                return await dbCommand.ExecuteReaderAsync(behavior);
            }

            DbCommandDecorator dbCommandDecorator = cmd as DbCommandDecorator;
            if (dbCommandDecorator != null)
            {
                return await dbCommandDecorator.ExecuteReaderAsync(behavior);
            }

            return cmd.ExecuteReader(behavior);
        }

        public static async Task<object> ExecuteScalarAsyncEx(this IDbCommand cmd)
        {
            DbCommand dbCommand = cmd as DbCommand;
            if (dbCommand != null)
            {
                return await dbCommand.ExecuteScalarAsync();
            }

            DbCommandDecorator dbCommandDecorator = cmd as DbCommandDecorator;
            if (dbCommandDecorator != null)
            {
                return await dbCommandDecorator.ExecuteScalarAsync();
            }

            return cmd.ExecuteScalar();
        }

        public static async Task<int> ExecuteNonQueryAsyncEx(this IDbCommand cmd)
        {
            DbCommand dbCommand = cmd as DbCommand;
            if (dbCommand != null)
            {
                return await dbCommand.ExecuteNonQueryAsync();
            }

            DbCommandDecorator dbCommandDecorator = cmd as DbCommandDecorator;
            if (dbCommandDecorator != null)
            {
                return await dbCommandDecorator.ExecuteNonQueryAsync();
            }

            return cmd.ExecuteNonQuery();
        }

        public static async Task<bool> Read(this IDataReader dataReader, bool @async)
        {
            if (@async)
            {
                return await ReadAsyncEx(dataReader);
            }

            return dataReader.Read();
        }
        public static async Task<bool> ReadAsyncEx(this IDataReader dataReader)
        {
            DbDataReader dbDataReader = dataReader as DbDataReader;
            if (dbDataReader != null)
            {
                return await dbDataReader.ReadAsync();
            }

            DataReaderDecorator dataReaderDecorator = dataReader as DataReaderDecorator;
            if (dataReaderDecorator != null)
            {
                return await dataReaderDecorator.ReadAsync();
            }

            return dataReader.Read();
        }
    }
}
