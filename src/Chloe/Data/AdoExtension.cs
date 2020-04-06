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

        public static Task<object> ExecuteScalarAsyncEx(this IDbCommand cmd)
        {
            DbCommand dbCommand = cmd as DbCommand;
            if (dbCommand != null)
            {
                return dbCommand.ExecuteScalarAsync();
            }

            DbCommandDecorator dbCommandDecorator = cmd as DbCommandDecorator;
            if (dbCommandDecorator != null)
            {
                return dbCommandDecorator.ExecuteScalarAsync();
            }

            return Task.FromResult(cmd.ExecuteScalar());
        }

        public static Task<int> ExecuteNonQueryAsyncEx(this IDbCommand cmd)
        {
            DbCommand dbCommand = cmd as DbCommand;
            if (dbCommand != null)
            {
                return dbCommand.ExecuteNonQueryAsync();
            }

            DbCommandDecorator dbCommandDecorator = cmd as DbCommandDecorator;
            if (dbCommandDecorator != null)
            {
                return dbCommandDecorator.ExecuteNonQueryAsync();
            }

            return Task.FromResult(cmd.ExecuteNonQuery());
        }

        public static Task<bool> Read(this IDataReader dataReader, bool @async)
        {
            if (@async)
            {
                return ReadAsyncEx(dataReader);
            }

            return Task.FromResult(dataReader.Read());
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
