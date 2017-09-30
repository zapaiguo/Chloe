using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public static class DbHelper
    {
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;

        public static SqlConnection CreateConnection()
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            return conn;
        }
        public static SqlConnection CreateConnection(string connString)
        {
            SqlConnection conn = new SqlConnection(connString);
            return conn;
        }

        public static DataTable FillDataTable(this IDataReader reader)
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
                    var val = reader[i];
                    dr[i] = val;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        public static DataSet FillDataSet(this IDataReader reader)
        {
            DataSet ds = new DataSet();
            var dt = FillDataTable(reader);
            ds.Tables.Add(dt);

            while (reader.NextResult())
            {
                dt = FillDataTable(reader);
                ds.Tables.Add(dt);
            }

            return ds;
        }

        public static void SqlBulkCopy<TModel>(List<TModel> modelList, int batchSize, string destinationTableName = null, SqlTransaction externalTransaction = null)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            {
                SqlBulkCopy<TModel>(conn, modelList, batchSize, destinationTableName, externalTransaction);
            }
        }
        public static void SqlBulkCopy<TModel>(SqlConnection conn, List<TModel> modelList, int batchSize, string destinationTableName = null, SqlTransaction externalTransaction = null)
        {
            bool shouldCloseConnection = false;

            DataTable dtToWrite = ToSqlBulkCopyDataTable(modelList, destinationTableName);

            SqlBulkCopy sbc = null;

            try
            {
                if (externalTransaction != null)
                    sbc = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, externalTransaction);
                else
                    sbc = new SqlBulkCopy(conn);

                using (sbc)
                {
                    sbc.BatchSize = batchSize;
                    sbc.DestinationTableName = destinationTableName;

                    if (conn.State != ConnectionState.Open)
                    {
                        shouldCloseConnection = true;
                        conn.Open();
                    }

                    sbc.WriteToServer(dtToWrite);
                }
            }
            finally
            {
                if (shouldCloseConnection && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }


        public static DataTable ToSqlBulkCopyDataTable<TModel>(List<TModel> modelList, string tableName = null)
        {
            DataTable dt = new DataTable();

            Type modelType = typeof(TModel);

            if (string.IsNullOrEmpty(tableName))
                tableName = modelType.Name;

            List<SysColumn> columns = GetTableColumns(tableName);
            List<PropertyInfo> mappingProps = new List<PropertyInfo>();

            var props = modelType.GetProperties();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                PropertyInfo mappingProp = props.Where(a => a.Name == column.Name).FirstOrDefault();
                if (mappingProp == null)
                    throw new Exception(string.Format("model 类型 '{0}'未定义与表 '{1}' 列名为 '{2}' 映射的属性", modelType.FullName, tableName, column.Name));

                mappingProps.Add(mappingProp);
                dt.Columns.Add(new DataColumn(column.Name));
            }

            foreach (var model in modelList)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < mappingProps.Count; i++)
                {
                    PropertyInfo prop = mappingProps[i];
                    object value = prop.GetValue(model);
                    dr[i] = value;
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }
        static List<SysColumn> GetTableColumns(string tableName)
        {
            string sql = string.Format("select * from syscolumns inner join sysobjects on syscolumns.id=sysobjects.id where sysobjects.xtype='U' and sysobjects.name='{0}' order by syscolumns.colid asc", tableName);

            List<SysColumn> columns = new List<SysColumn>();
            using (var conn = DbHelper.CreateConnection())
            {
                conn.Open();
                using (var reader = conn.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        SysColumn column = new SysColumn();
                        column.Name = reader.GetDbValue("name");
                        column.ColOrder = reader.GetDbValue("colorder");

                        columns.Add(column);
                    }
                }
                conn.Close();
            }

            return columns;
        }



        class SysColumn
        {
            public string Name { get; set; }
            public int ColOrder { get; set; }
        }
    }
}
