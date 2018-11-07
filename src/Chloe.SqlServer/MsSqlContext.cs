using Chloe.Core;
using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Entity;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using Chloe.InternalExtensions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.SqlServer
{
    public partial class MsSqlContext : DbContext
    {
        DatabaseProvider _databaseProvider;
        public MsSqlContext(string connString)
            : this(new DefaultDbConnectionFactory(connString))
        {
        }

        public MsSqlContext(IDbConnectionFactory dbConnectionFactory)
        {
            PublicHelper.CheckNull(dbConnectionFactory);

            this.PagingMode = PagingMode.ROW_NUMBER;
            this._databaseProvider = new DatabaseProvider(dbConnectionFactory, this);
        }

        static Dictionary<string, SysType> SysTypes;

        static MsSqlContext()
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
        /// 分页模式。
        /// </summary>
        public PagingMode PagingMode { get; set; }
        public override IDatabaseProvider DatabaseProvider
        {
            get { return this._databaseProvider; }
        }

        public override TEntity Insert<TEntity>(TEntity entity, string table)
        {
            PublicHelper.CheckNull(entity);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entity.GetType());

            Dictionary<PropertyDescriptor, object> keyValueMap = CreateKeyValueMap(typeDescriptor);

            Dictionary<PropertyDescriptor, DbExpression> insertColumns = new Dictionary<PropertyDescriptor, DbExpression>();
            foreach (PropertyDescriptor propertyDescriptor in typeDescriptor.PropertyDescriptors)
            {
                if (propertyDescriptor.IsAutoIncrement)
                    continue;

                if (propertyDescriptor.HasSequence())
                {
                    DbMethodCallExpression getNextValueForSequenceExp = PublicHelper.MakeNextValueForSequenceDbExpression(propertyDescriptor);
                    insertColumns.Add(propertyDescriptor, getNextValueForSequenceExp);
                    continue;
                }

                object val = propertyDescriptor.GetValue(entity);

                if (propertyDescriptor.IsPrimaryKey)
                {
                    keyValueMap[propertyDescriptor] = val;
                }

                DbExpression valExp = DbExpression.Parameter(val, propertyDescriptor.PropertyType, propertyDescriptor.Column.DbType);
                insertColumns.Add(propertyDescriptor, valExp);
            }

            PropertyDescriptor nullValueKey = keyValueMap.Where(a => a.Value == null && !a.Key.IsAutoIncrement).Select(a => a.Key).FirstOrDefault();
            if (nullValueKey != null)
            {
                /* 主键为空并且主键又不是自增列 */
                throw new ChloeException(string.Format("The primary key '{0}' could not be null.", nullValueKey.Property.Name));
            }

            DbTable dbTable = table == null ? typeDescriptor.Table : new DbTable(table, typeDescriptor.Table.Schema);
            DbInsertExpression insertExp = new DbInsertExpression(dbTable);

            foreach (var kv in insertColumns)
            {
                insertExp.InsertColumns.Add(kv.Key.Column, kv.Value);
            }

            List<PropertyDescriptor> outputColumns = typeDescriptor.PropertyDescriptors.Where(a => a.IsAutoIncrement || a.HasSequence()).ToList();

            if (outputColumns.Count == 0)
            {
                this.ExecuteSqlCommand(insertExp);
                return entity;
            }

            List<Action<TEntity, IDataReader>> mappers = new List<Action<TEntity, IDataReader>>();
            IDbExpressionTranslator translator = this.DatabaseProvider.CreateDbExpressionTranslator();
            List<DbParam> parameters;
            string sql = null;
            if (outputColumns.Count == 1 && outputColumns[0].IsAutoIncrement)
            {
                sql = translator.Translate(insertExp, out parameters);

                /* 自增 id 不能用 output  inserted.Id 输出，因为如果表设置了触发器的话会报错 */
                sql = string.Concat(sql, ";", this.GetSelectLastInsertIdClause());
                mappers.Add(GetMapper<TEntity>(outputColumns[0], 0));
            }
            else
            {
                foreach (PropertyDescriptor outputColumn in outputColumns)
                {
                    mappers.Add(GetMapper<TEntity>(outputColumn, insertExp.Returns.Count));
                    insertExp.Returns.Add(outputColumn.Column);
                }

                sql = translator.Translate(insertExp, out parameters);
            }

            IDataReader dataReader = this.Session.ExecuteReader(sql, parameters.ToArray());
            using (dataReader)
            {
                dataReader.Read();
                foreach (var mapper in mappers)
                {
                    mapper(entity, dataReader);
                }
            }

            return entity;
        }
        public override object Insert<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            PublicHelper.CheckNull(content);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(TEntity));

            if (typeDescriptor.PrimaryKeys.Count > 1)
            {
                /* 对于多主键的实体，暂时不支持调用这个方法进行插入 */
                throw new NotSupportedException(string.Format("Can not call this method because entity '{0}' has multiple keys.", typeDescriptor.Definition.Type.FullName));
            }

            PropertyDescriptor keyPropertyDescriptor = typeDescriptor.PrimaryKeys.FirstOrDefault();

            Dictionary<MemberInfo, Expression> insertColumns = InitMemberExtractor.Extract(content);

            DbTable explicitDbTable = null;
            if (table != null)
                explicitDbTable = new DbTable(table, typeDescriptor.Table.Schema);
            DefaultExpressionParser expressionParser = typeDescriptor.GetExpressionParser(explicitDbTable);
            DbInsertExpression insertExp = new DbInsertExpression(explicitDbTable ?? typeDescriptor.Table);

            object keyVal = null;

            foreach (var kv in insertColumns)
            {
                MemberInfo key = kv.Key;
                PropertyDescriptor propertyDescriptor = typeDescriptor.TryGetPropertyDescriptor(key);

                if (propertyDescriptor == null)
                    throw new ChloeException(string.Format("The member '{0}' does not map any column.", key.Name));

                if (propertyDescriptor.IsAutoIncrement)
                    throw new ChloeException(string.Format("Could not insert value into the identity column '{0}'.", propertyDescriptor.Column.Name));

                if (propertyDescriptor.HasSequence())
                    throw new ChloeException(string.Format("Can not insert value into the column '{0}', because it's mapping member has define a sequence.", propertyDescriptor.Column.Name));

                if (propertyDescriptor.IsPrimaryKey)
                {
                    object val = ExpressionEvaluator.Evaluate(kv.Value);
                    if (val == null)
                        throw new ChloeException(string.Format("The primary key '{0}' could not be null.", propertyDescriptor.Property.Name));
                    else
                    {
                        keyVal = val;
                        insertExp.InsertColumns.Add(propertyDescriptor.Column, DbExpression.Parameter(keyVal, propertyDescriptor.PropertyType, propertyDescriptor.Column.DbType));
                        continue;
                    }
                }

                insertExp.InsertColumns.Add(propertyDescriptor.Column, expressionParser.Parse(kv.Value));
            }

            foreach (var item in typeDescriptor.PropertyDescriptors.Where(a => a.HasSequence()))
            {
                DbMethodCallExpression getNextValueForSequenceExp = PublicHelper.MakeNextValueForSequenceDbExpression(item);
                insertExp.InsertColumns.Add(item.Column, getNextValueForSequenceExp);
            }

            if (keyPropertyDescriptor != null)
            {
                //主键为空并且主键又不是自增列
                if (keyVal == null && !keyPropertyDescriptor.IsAutoIncrement && !keyPropertyDescriptor.HasSequence())
                {
                    throw new ChloeException(string.Format("The primary key '{0}' could not be null.", keyPropertyDescriptor.Property.Name));
                }
            }

            if (keyPropertyDescriptor == null)
            {
                this.ExecuteSqlCommand(insertExp);
                return keyVal; /* It will return null if an entity does not define primary key. */
            }
            if (!keyPropertyDescriptor.IsAutoIncrement && !keyPropertyDescriptor.HasSequence())
            {
                this.ExecuteSqlCommand(insertExp);
                return keyVal;
            }

            IDbExpressionTranslator translator = this.DatabaseProvider.CreateDbExpressionTranslator();
            List<DbParam> parameters;
            string sql = translator.Translate(insertExp, out parameters);

            if (keyPropertyDescriptor.IsAutoIncrement)
            {
                /* 自增 id 不能用 output  inserted.Id 输出，因为如果表设置了触发器的话会报错 */
                sql = string.Concat(sql, ";", this.GetSelectLastInsertIdClause());
            }
            else if (keyPropertyDescriptor.HasSequence())
            {
                insertExp.Returns.Add(keyPropertyDescriptor.Column);
            }

            object ret = this.Session.ExecuteScalar(sql, parameters.ToArray());
            if (ret == null || ret == DBNull.Value)
            {
                throw new ChloeException("Unable to get the identity/sequence value.");
            }

            ret = PublicHelper.ConvertObjType(ret, typeDescriptor.AutoIncrement.PropertyType);
            return ret;
        }

        public override void InsertRange<TEntity>(List<TEntity> entities, bool keepIdentity = false, string table = null)
        {
            /*
             * 将 entities 分批插入数据库
             * 每批生成 insert into TableName(...) values(...),(...)... 
             * 该方法相对循环一条一条插入，速度提升 2/3 这样
             */

            PublicHelper.CheckNull(entities);
            if (entities.Count == 0)
                return;

            int maxParameters = 2100;
            int batchSize = 50; /* 每批实体大小，此值通过测试得出相对插入速度比较快的一个值 */

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(TEntity));

            var e = typeDescriptor.PropertyDescriptors as IEnumerable<PropertyDescriptor>;
            if (keepIdentity == false)
                e = e.Where(a => a.IsAutoIncrement == false);
            List<PropertyDescriptor> mappingPropertyDescriptors = e.ToList();
            int maxDbParamsCount = maxParameters - mappingPropertyDescriptors.Count; /* 控制一个 sql 的参数个数 */

            DbTable dbTable = string.IsNullOrEmpty(table) ? typeDescriptor.Table : new DbTable(table, typeDescriptor.Table.Schema);
            string sqlTemplate = AppendInsertRangeSqlTemplate(dbTable, mappingPropertyDescriptors);

            Action insertAction = () =>
            {
                int batchCount = 0;
                List<DbParam> dbParams = new List<DbParam>();
                StringBuilder sqlBuilder = new StringBuilder();
                for (int i = 0; i < entities.Count; i++)
                {
                    var entity = entities[i];

                    if (batchCount > 0)
                        sqlBuilder.Append(",");

                    sqlBuilder.Append("(");
                    for (int j = 0; j < mappingPropertyDescriptors.Count; j++)
                    {
                        if (j > 0)
                            sqlBuilder.Append(",");

                        PropertyDescriptor mappingPropertyDescriptor = mappingPropertyDescriptors[j];
                        object val = mappingPropertyDescriptor.GetValue(entity);
                        if (val == null)
                        {
                            sqlBuilder.Append("NULL");
                            continue;
                        }

                        Type valType = val.GetType();
                        if (valType.IsEnum)
                        {
                            val = Convert.ChangeType(val, Enum.GetUnderlyingType(valType));
                            valType = val.GetType();
                        }

                        if (Utils.IsToStringableNumericType(valType))
                        {
                            sqlBuilder.Append(val.ToString());
                            continue;
                        }

                        if (val is bool)
                        {
                            if ((bool)val == true)
                                sqlBuilder.AppendFormat("1");
                            else
                                sqlBuilder.AppendFormat("0");
                            continue;
                        }

                        string paramName = UtilConstants.ParameterNamePrefix + dbParams.Count.ToString();
                        DbParam dbParam = new DbParam(paramName, val) { DbType = mappingPropertyDescriptor.Column.DbType };
                        dbParams.Add(dbParam);
                        sqlBuilder.Append(paramName);
                    }
                    sqlBuilder.Append(")");

                    batchCount++;

                    if ((batchCount >= 20 && dbParams.Count >= 120/*参数个数太多也会影响速度*/) || dbParams.Count >= maxDbParamsCount || batchCount >= batchSize || (i + 1) == entities.Count)
                    {
                        sqlBuilder.Insert(0, sqlTemplate);
                        string sql = sqlBuilder.ToString();
                        this.Session.ExecuteNonQuery(sql, dbParams.ToArray());

                        sqlBuilder.Clear();
                        dbParams.Clear();
                        batchCount = 0;
                    }
                }
            };

            Action fAction = () =>
            {
                bool shouldTurnOff_IDENTITY_INSERT = false;
                string tableName = null;
                if (keepIdentity == true)
                {
                    tableName = AppendTableName(typeDescriptor.Table);
                    this.Session.ExecuteNonQuery(string.Format("SET IDENTITY_INSERT {0} ON ", tableName));
                    shouldTurnOff_IDENTITY_INSERT = true;
                }

                insertAction();
                if (shouldTurnOff_IDENTITY_INSERT == true)
                    this.Session.ExecuteNonQuery(string.Format("SET IDENTITY_INSERT {0} OFF ", tableName));
            };

            if (this.Session.IsInTransaction)
            {
                fAction();
            }
            else
            {
                /* 因为分批插入，所以需要开启事务保证数据一致性 */
                this.Session.BeginTransaction();
                try
                {
                    fAction();
                    this.Session.CommitTransaction();
                }
                catch
                {
                    if (this.Session.IsInTransaction)
                        this.Session.RollbackTransaction();
                    throw;
                }
            }
        }

        /// <summary>
        /// 利用 SqlBulkCopy 批量插入数据。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <param name="table"></param>
        /// <param name="batchSize">设置 SqlBulkCopy.BatchSize 的值</param>
        /// <param name="bulkCopyTimeout">设置 SqlBulkCopy.BulkCopyTimeout 的值</param>
        /// <param name="keepIdentity">是否保留源自增值。false 由数据库分配自增值</param>
        public virtual void BulkInsert<TEntity>(List<TEntity> entities, string table = null, int? batchSize = null, int? bulkCopyTimeout = null, bool keepIdentity = false)
        {
            PublicHelper.CheckNull(entities);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(TEntity));

            DataTable dtToWrite = ToSqlBulkCopyDataTable(entities, typeDescriptor, table ?? typeDescriptor.Table.Name);

            SqlBulkCopy sbc = null;

            bool shouldCloseConnection = false;
            SqlConnection conn = this.Session.CurrentConnection as SqlConnection;
            try
            {
                SqlTransaction externalTransaction = null;
                if (this.Session.IsInTransaction)
                {
                    externalTransaction = this.Session.CurrentTransaction as SqlTransaction;
                }

                SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.FireTriggers;
                if (keepIdentity)
                    sqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity | sqlBulkCopyOptions;

                sbc = new SqlBulkCopy(conn, sqlBulkCopyOptions, externalTransaction);

                using (sbc)
                {
                    if (batchSize != null)
                        sbc.BatchSize = batchSize.Value;

                    DbTable dbTable = string.IsNullOrEmpty(table) ? typeDescriptor.Table : new DbTable(table, typeDescriptor.Table.Schema);
                    sbc.DestinationTableName = AppendTableName(dbTable);

                    if (bulkCopyTimeout != null)
                        sbc.BulkCopyTimeout = bulkCopyTimeout.Value;

                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                        shouldCloseConnection = true;
                    }

                    sbc.WriteToServer(dtToWrite);
                }
            }
            finally
            {
                if (conn != null)
                {
                    if (shouldCloseConnection && conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }
        }

        DataTable ToSqlBulkCopyDataTable<TModel>(List<TModel> modelList, TypeDescriptor typeDescriptor, string tableName)
        {
            DataTable dt = new DataTable();

            List<SysColumn> columns = GetTableColumns(tableName);
            List<ColumnMapping> columnMappings = new List<ColumnMapping>();

            var mappingPropertyDescriptors = typeDescriptor.PropertyDescriptors.ToList();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                PropertyDescriptor mappingPropertyDescriptor = mappingPropertyDescriptors.Find(a => string.Equals(a.Column.Name, column.Name));
                if (mappingPropertyDescriptor == null)
                    mappingPropertyDescriptor = mappingPropertyDescriptors.Find(a => string.Equals(a.Column.Name, column.Name, StringComparison.OrdinalIgnoreCase));

                ColumnMapping columnMapping = new ColumnMapping(column);
                Type dataType;
                if (mappingPropertyDescriptor == null)
                {
                    /*
                     * 由于 SqlBulkCopy 要求传入的列必须与表列一一对应，因此，如果 model 中没有与列对应的属性，则使用列数据类型的默认值
                     */

                    SysType sysType = GetSysTypeByTypeName(column.TypeName);
                    columnMapping.DefaultValue = column.IsNullable ? null : sysType.DefaultValue;
                    dataType = sysType.CSharpType;
                }
                else
                {
                    columnMapping.MapMember = mappingPropertyDescriptor.Property;
                    dataType = mappingPropertyDescriptor.PropertyType.GetUnderlyingType();
                    if (dataType.IsEnum)
                        dataType = Enum.GetUnderlyingType(dataType);
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
                    MemberInfo member = columnMapping.MapMember;
                    object value = null;
                    if (member == null)
                    {
                        value = columnMapping.DefaultValue;
                    }
                    else
                    {
                        value = member.GetMemberValue(model);
                        if (member.GetMemberType().GetUnderlyingType().IsEnum)
                        {
                            if (value != null)
                                value = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
                        }
                    }

                    dr[i] = value ?? DBNull.Value;
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }
        List<SysColumn> GetTableColumns(string tableName)
        {
            string sql = "select syscolumns.name,syscolumns.colorder,syscolumns.isnullable,systypes.xusertype,systypes.name as typename from syscolumns inner join systypes on syscolumns.xusertype=systypes.xusertype inner join sysobjects on syscolumns.id = sysobjects.id where sysobjects.xtype = 'U' and sysobjects.name = @TableName order by syscolumns.colid asc";

            List<SysColumn> columns = new List<SysColumn>();

            using (var reader = this.Session.ExecuteReader(sql, new DbParam("@TableName", tableName)))
            {
                while (reader.Read())
                {
                    SysColumn column = new SysColumn();
                    column.Name = GetValue<string>(reader, "name");
                    column.ColOrder = GetValue<int>(reader, "colorder");
                    column.XUserType = GetValue<int>(reader, "xusertype");
                    column.TypeName = GetValue<string>(reader, "typename");
                    column.IsNullable = GetValue<bool>(reader, "isnullable");

                    columns.Add(column);
                }

                reader.Close();
            }

            return columns;
        }

        int ExecuteSqlCommand(DbExpression e)
        {
            IDbExpressionTranslator translator = this.DatabaseProvider.CreateDbExpressionTranslator();
            List<DbParam> parameters;
            string cmdText = translator.Translate(e, out parameters);

            int r = this.Session.ExecuteNonQuery(cmdText, CommandType.Text, parameters.ToArray());
            return r;
        }

        class SysType<TCSharpType> : SysType
        {
            public SysType(string typeName)
            {
                this.TypeName = typeName;
                this.CSharpType = typeof(TCSharpType);
                this.DefaultValue = default(TCSharpType);
            }
        }
        class SysType
        {
            public string TypeName { get; set; }
            public Type CSharpType { get; set; }
            public object DefaultValue { get; set; }
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
            public MemberInfo MapMember { get; set; }
            public object DefaultValue { get; set; }
        }
    }
}
