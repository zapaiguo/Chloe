using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.SqlClient
{
    public static class SqlConnectionExtension1
    {
        static Dictionary<string, SysType> SysTypes = new Dictionary<string, SysType>();
        static SqlConnectionExtension1()
        {
            List<SysType> sysTypes = new List<SysType>();
            sysTypes.Add(new SysType<Byte[]>("image"));
            sysTypes.Add(new SysType<string>("text"));
            sysTypes.Add(new SysType<Guid>("uniqueidentifier"));
            sysTypes.Add(new SysType<DateTime>("date"));
            sysTypes.Add(new SysType<TimeSpan>("time"));
            sysTypes.Add(new SysType<DateTime>("datetime2"));
            //sysTypes.Add(new SysType<string>("datetimeoffset"));
            sysTypes.Add(new SysType<byte>("tinyint"));
            sysTypes.Add(new SysType<Int16>("smallint"));
            sysTypes.Add(new SysType<int>("int"));
            sysTypes.Add(new SysType<DateTime>("smalldatetime"));
            sysTypes.Add(new SysType<float>("real"));
            sysTypes.Add(new SysType<decimal>("money"));
            sysTypes.Add(new SysType<DateTime>("datetime"));
            sysTypes.Add(new SysType<double>("float"));
            //sysTypes.Add(new SysType<string>("sql_variant"));
            sysTypes.Add(new SysType<string>("ntext"));
            sysTypes.Add(new SysType<bool>("bit"));
            sysTypes.Add(new SysType<decimal>("decimal"));
            sysTypes.Add(new SysType<decimal>("numeric"));
            sysTypes.Add(new SysType<decimal>("smallmoney"));
            sysTypes.Add(new SysType<long>("bigint"));
            //sysTypes.Add(new SysType<string>("hierarchyid"));
            //sysTypes.Add(new SysType<string>("geometry"));
            //sysTypes.Add(new SysType<string>("geography"));
            sysTypes.Add(new SysType<Byte[]>("varbinary"));
            sysTypes.Add(new SysType<string>("varchar"));
            sysTypes.Add(new SysType<Byte[]>("binary"));
            sysTypes.Add(new SysType<string>("char"));
            sysTypes.Add(new SysType<Byte[]>("timestamp"));
            sysTypes.Add(new SysType<string>("nvarchar"));
            sysTypes.Add(new SysType<string>("nchar"));
            sysTypes.Add(new SysType<string>("xml"));
            sysTypes.Add(new SysType<string>("sysname"));

            SysTypes = sysTypes.ToDictionary(a => a.TypeName, a => a);
        }


        /// <summary>
        /// 使用 SqlBulkCopy 向 destinationTableName 表插入数据
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="conn"></param>
        /// <param name="modelList"></param>
        /// <param name="batchSize"></param>
        /// <param name="destinationTableName">如果为 null，则使用 TModel 名称作为 destinationTableName</param>
        /// <param name="bulkCopyTimeout"></param>
        /// <param name="externalTransaction"></param>
        public static void BulkCopy1<TModel>(this SqlConnection conn, List<TModel> modelList, int batchSize, string destinationTableName = null, int? bulkCopyTimeout = null, SqlTransaction externalTransaction = null, bool keepIdentity = false)
        {
            bool shouldCloseConnection = false;

            if (string.IsNullOrEmpty(destinationTableName))
                destinationTableName = typeof(TModel).Name;

            DataTable dtToWrite = ToSqlBulkCopyDataTable(modelList, conn, destinationTableName);

            SqlBulkCopy sbc = null;

            try
            {
                SqlBulkCopyOptions sqlBulkCopyOptions = keepIdentity ? (SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.KeepIdentity) : SqlBulkCopyOptions.KeepNulls;
                sbc = new SqlBulkCopy(conn, sqlBulkCopyOptions, externalTransaction);

                using (sbc)
                {
                    //sbc.BatchSize = batchSize;
                    sbc.DestinationTableName = destinationTableName;

                    if (bulkCopyTimeout != null)
                        sbc.BulkCopyTimeout = bulkCopyTimeout.Value;

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


        public static DataTable ToSqlBulkCopyDataTable<TModel>(List<TModel> modelList, SqlConnection conn, string tableName)
        {
            DataTable dt = new DataTable();

            Type modelType = typeof(TModel);

            List<SysColumn> columns = GetTableColumns(conn, tableName);
            List<ColumnMapping> columnMappings = new List<ColumnMapping>();

            var props = modelType.GetProperties();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                PropertyInfo mappingProp = props.Where(a => a.Name == column.Name).FirstOrDefault();
                ColumnMapping columnMapping = new ColumnMapping(column);
                Type dataType;
                if (mappingProp == null)
                {
                    /*
                     * 由于 SqlBulkCopy 要求传入的列必须与表列一一对应，因此，如果 model 中没有与列对应的属性，则使用列数据类型的默认值
                     */

                    SysType sysType = GetSysTypeByTypeName(column.TypeName);
                    columnMapping.DefaultValue = column.IsNullable ? null : sysType.DetaultValue;
                    dataType = sysType.CSharpType;
                }
                else
                {
                    columnMapping.MapProperty = mappingProp;
                    dataType = GetUnderlyingType(mappingProp.PropertyType);
                    if (dataType.IsEnum)
                        dataType = typeof(int);
                }

                columnMappings.Add(columnMapping);
                dt.Columns.Add(new DataColumn(column.Name, dataType));
            }

            foreach (var model in modelList)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < columnMappings.Count; i++)
                {
                    ColumnMapping columnMapping = columnMappings[i];
                    PropertyInfo prop = columnMapping.MapProperty;
                    object value = null;
                    if (prop == null)
                    {
                        value = columnMapping.DefaultValue;
                    }
                    else
                    {
                        value = prop.GetValue(model);
                        if (GetUnderlyingType(prop.PropertyType).IsEnum)
                        {
                            if (value != null)
                                value = (int)value;
                        }
                    }

                    dr[i] = value ?? DBNull.Value;
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }
        static List<SysColumn> GetTableColumns(SqlConnection sourceConn, string tableName)
        {
            string sql = string.Format("select syscolumns.name,syscolumns.colorder,syscolumns.isnullable,systypes.xusertype,systypes.name as typename from syscolumns inner join systypes on syscolumns.xusertype=systypes.xusertype inner join sysobjects on syscolumns.id = sysobjects.id where sysobjects.xtype = 'U' and sysobjects.name = '{0}' order by syscolumns.colid asc", tableName);

            List<SysColumn> columns = new List<SysColumn>();
            using (SqlConnection conn = (SqlConnection)((ICloneable)sourceConn).Clone())
            {
                conn.Open();
                using (var reader = conn.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        SysColumn column = new SysColumn();
                        column.Name = reader.GetValue<string>("name");
                        column.ColOrder = reader.GetValue<int>("colorder");
                        column.XUserType = reader.GetValue<int>("xusertype");
                        column.TypeName = reader.GetValue<string>("typename");
                        column.IsNullable = reader.GetValue<bool>("isnullable");

                        columns.Add(column);
                    }
                }
                conn.Close();
            }

            return columns;
        }

        static Type GetUnderlyingType(Type type)
        {
            Type unType = Nullable.GetUnderlyingType(type); ;
            if (unType == null)
                unType = type;

            return unType;
        }
        static SysType GetSysTypeByTypeName(string typeName)
        {
            SysType sysType;
            if (SysTypes.TryGetValue(typeName, out sysType))
            {
                return sysType;
            }

            throw new NotSupportedException(string.Format("Does not Support systype '{0}'", typeName));
        }
        static T GetValue<T>(this IDataReader reader, string name)
        {
            object val = reader.GetValue(reader.GetOrdinal(name));
            if (val == DBNull.Value)
            {
                val = null;
                return (T)val;
            }

            return (T)Convert.ChangeType(val, typeof(T).GetUnderlyingType());
        }


        class SysType<TCSharpType> : SysType
        {
            public SysType(string typeName)
            {
                this.TypeName = typeName;
                this.CSharpType = typeof(TCSharpType);
                this.DetaultValue = default(TCSharpType);
            }
        }
        class SysType
        {
            public string TypeName { get; set; }
            public Type CSharpType { get; set; }
            public object DetaultValue { get; set; }
        }
        class SysColumn
        {
            public string Name { get; set; }
            public int ColOrder { get; set; }
            public int XUserType { get; set; }
            public string TypeName { get; set; }
            public bool IsNullable { get; set; }
            public override string ToString()
            {
                return this.Name;
            }
        }
        class ColumnMapping
        {
            public ColumnMapping(SysColumn column)
            {
                this.Column = column;
            }
            public SysColumn Column { get; set; }
            public PropertyInfo MapProperty { get; set; }
            public object DefaultValue { get; set; }
        }
    }
}
