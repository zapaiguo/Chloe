using Chloe.Annotations;
using Chloe.Core;
using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Entity;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.Oracle
{
    public partial class OracleContext : DbContext
    {
        DatabaseProvider _databaseProvider;
        public OracleContext(IDbConnectionFactory dbConnectionFactory)
        {
            PublicHelper.CheckNull(dbConnectionFactory);

            this._databaseProvider = new DatabaseProvider(dbConnectionFactory, this);
        }


        /// <summary>
        /// 是否将 sql 中的表名/字段名转成大写。默认为 true。
        /// </summary>
        public bool ConvertToUppercase { get; set; } = true;
        public override IDatabaseProvider DatabaseProvider
        {
            get { return this._databaseProvider; }
        }

        public override TEntity Insert<TEntity>(TEntity entity, string table)
        {
            PublicHelper.CheckNull(entity);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entity.GetType());

            List<PropertyDescriptor> outputColumns = new List<PropertyDescriptor>();
            Dictionary<PropertyDescriptor, DbExpression> insertColumns = new Dictionary<PropertyDescriptor, DbExpression>();
            foreach (PropertyDescriptor propertyDescriptor in typeDescriptor.PropertyDescriptors)
            {
                if (propertyDescriptor.HasSequence())
                {
                    DbMethodCallExpression getNextValueForSequenceExp = PublicHelper.MakeNextValueForSequenceDbExpression(propertyDescriptor);
                    insertColumns.Add(propertyDescriptor, getNextValueForSequenceExp);
                    outputColumns.Add(propertyDescriptor);
                    continue;
                }

                object val = propertyDescriptor.GetValue(entity);

                DbExpression valExp = DbExpression.Parameter(val, propertyDescriptor.PropertyType, propertyDescriptor.Column.DbType);
                insertColumns.Add(propertyDescriptor, valExp);
            }

            DbTable dbTable = table == null ? typeDescriptor.Table : new DbTable(table, typeDescriptor.Table.Schema);
            DbInsertExpression e = new DbInsertExpression(dbTable);

            foreach (var kv in insertColumns)
            {
                e.InsertColumns.Add(kv.Key.Column, kv.Value);
            }

            e.Returns.AddRange(outputColumns.Select(a => a.Column));

            List<DbParam> parameters;
            this.ExecuteSqlCommand(e, out parameters);

            List<DbParam> outputParams = parameters.Where(a => a.Direction == ParamDirection.Output).ToList();

            for (int i = 0; i < outputColumns.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = outputColumns[i];
                string putputColumnName = Utils.GenOutputColumnParameterName(propertyDescriptor.Column.Name);
                DbParam outputParam = outputParams.Where(a => a.Name == putputColumnName).First();
                outputColumns[i].SetValue(entity, outputParam.Value);
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

            var sequencePropertyDescriptors = typeDescriptor.PropertyDescriptors.Where(a => a.HasSequence());
            foreach (PropertyDescriptor sequencePropertyDescriptor in sequencePropertyDescriptors)
            {
                DbMethodCallExpression getNextValueForSequenceExp = PublicHelper.MakeNextValueForSequenceDbExpression(sequencePropertyDescriptor);
                insertExp.InsertColumns.Add(sequencePropertyDescriptor.Column, getNextValueForSequenceExp);

                if (sequencePropertyDescriptor.IsPrimaryKey)
                {
                    insertExp.Returns.Add(sequencePropertyDescriptor.Column);
                }
            }

            if (keyPropertyDescriptor != null && !keyPropertyDescriptor.HasSequence() && keyVal == null)
            {
                throw new ChloeException(string.Format("The primary key '{0}' could not be null.", keyPropertyDescriptor.Property.Name));
            }

            List<DbParam> parameters;
            this.ExecuteSqlCommand(insertExp, out parameters);

            if (keyPropertyDescriptor != null && keyPropertyDescriptor.HasSequence())
            {
                string putputColumnName = Utils.GenOutputColumnParameterName(keyPropertyDescriptor.Column.Name);
                DbParam outputParam = parameters.Where(a => a.Direction == ParamDirection.Output && a.Name == putputColumnName).First();
                keyVal = PublicHelper.ConvertObjType(outputParam.Value, keyPropertyDescriptor.PropertyType);
            }

            return keyVal; /* It will return null if an entity does not define primary key. */
        }
        public override void InsertRange<TEntity>(List<TEntity> entities, bool keepIdentity = false, string table = null)
        {
            /*
             * 将 entities 分批插入数据库
             * 每批生成 insert into TableName(...) select ... from dual union all select ... from dual...
             * 对于 oracle，貌似速度提升不了...- -
             * #期待各码友的优化建议#
             */

            PublicHelper.CheckNull(entities);
            if (entities.Count == 0)
                return;

            int maxParameters = 1000;
            int batchSize = 40; /* 每批实体大小，此值通过测试得出相对插入速度比较快的一个值 */

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(TEntity));

            List<PropertyDescriptor> mappingPropertyDescriptors = typeDescriptor.PropertyDescriptors.ToList();
            int maxDbParamsCount = maxParameters - mappingPropertyDescriptors.Count; /* 控制一个 sql 的参数个数 */

            DbTable dbTable = string.IsNullOrEmpty(table) ? typeDescriptor.Table : new DbTable(table, typeDescriptor.Table.Schema);
            string sqlTemplate = AppendInsertRangeSqlTemplate(dbTable, mappingPropertyDescriptors, keepIdentity);

            Action insertAction = () =>
            {
                int batchCount = 0;
                List<DbParam> dbParams = new List<DbParam>();
                StringBuilder sqlBuilder = new StringBuilder();
                for (int i = 0; i < entities.Count; i++)
                {
                    var entity = entities[i];

                    if (batchCount > 0)
                        sqlBuilder.Append(" UNION ALL ");

                    sqlBuilder.Append(" SELECT ");
                    for (int j = 0; j < mappingPropertyDescriptors.Count; j++)
                    {
                        if (j > 0)
                            sqlBuilder.Append(",");

                        PropertyDescriptor mappingPropertyDescriptor = mappingPropertyDescriptors[j];

                        if (keepIdentity == false && mappingPropertyDescriptor.HasSequence())
                        {
                            sqlBuilder.AppendFormat("1");
                            continue;
                        }

                        object val = mappingPropertyDescriptor.GetValue(entity);
                        if (val == null)
                        {
                            sqlBuilder.Append("NULL");
                            sqlBuilder.Append(" C").Append(j.ToString());
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
                        }
                        else if (val is bool)
                        {
                            if ((bool)val == true)
                                sqlBuilder.AppendFormat("1");
                            else
                                sqlBuilder.AppendFormat("0");
                        }
                        else
                        {
                            string paramName = UtilConstants.ParameterNamePrefix + dbParams.Count.ToString();
                            DbParam dbParam = new DbParam(paramName, val) { DbType = mappingPropertyDescriptor.Column.DbType };
                            dbParams.Add(dbParam);
                            sqlBuilder.Append(paramName);
                        }

                        sqlBuilder.Append(" C").Append(j.ToString());
                    }

                    sqlBuilder.Append(" FROM DUAL");

                    batchCount++;

                    if ((batchCount >= 20 && dbParams.Count >= 400/*参数个数太多也会影响速度*/) || dbParams.Count >= maxDbParamsCount || batchCount >= batchSize || (i + 1) == entities.Count)
                    {
                        sqlBuilder.Insert(0, sqlTemplate);

                        if (keepIdentity == false)
                        {
                            sqlBuilder.Append(") T");
                        }

                        string sql = sqlBuilder.ToString();
                        this.Session.ExecuteNonQuery(sql, dbParams.ToArray());

                        sqlBuilder.Clear();
                        dbParams.Clear();
                        batchCount = 0;
                    }
                }
            };

            Action fAction = insertAction;

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


        public override int Update<TEntity>(TEntity entity, string table)
        {
            PublicHelper.CheckNull(entity);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entity.GetType());
            PublicHelper.EnsureHasPrimaryKey(typeDescriptor);

            Dictionary<PropertyDescriptor, object> keyValueMap = CreateKeyValueMap(typeDescriptor);

            IEntityState entityState = this.TryGetTrackedEntityState(entity);
            Dictionary<PropertyDescriptor, DbExpression> updateColumns = new Dictionary<PropertyDescriptor, DbExpression>();
            foreach (PropertyDescriptor propertyDescriptor in typeDescriptor.PropertyDescriptors)
            {
                if (keyValueMap.ContainsKey(propertyDescriptor))
                {
                    keyValueMap[propertyDescriptor] = propertyDescriptor.GetValue(entity);
                    continue;
                }

                bool hasSequence = propertyDescriptor.HasSequence();
                if (hasSequence)
                    continue;

                object val = propertyDescriptor.GetValue(entity);

                if (entityState != null && !entityState.HasChanged(propertyDescriptor, val))
                    continue;

                DbExpression valExp = DbExpression.Parameter(val, propertyDescriptor.PropertyType, propertyDescriptor.Column.DbType);
                updateColumns.Add(propertyDescriptor, valExp);
            }

            if (updateColumns.Count == 0)
                return 0;

            DbTable dbTable = table == null ? typeDescriptor.Table : new DbTable(table, typeDescriptor.Table.Schema);
            DbExpression conditionExp = MakeCondition(keyValueMap, dbTable);
            DbUpdateExpression e = new DbUpdateExpression(dbTable, conditionExp);

            foreach (var item in updateColumns)
            {
                e.UpdateColumns.Add(item.Key.Column, item.Value);
            }

            int ret = this.ExecuteSqlCommand(e);
            if (entityState != null)
                entityState.Refresh();
            return ret;
        }
        public override int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            PublicHelper.CheckNull(condition);
            PublicHelper.CheckNull(content);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(TEntity));

            Dictionary<MemberInfo, Expression> updateColumns = InitMemberExtractor.Extract(content);

            DbTable explicitDbTable = null;
            if (table != null)
                explicitDbTable = new DbTable(table, typeDescriptor.Table.Schema);
            DefaultExpressionParser expressionParser = typeDescriptor.GetExpressionParser(explicitDbTable);

            DbExpression conditionExp = expressionParser.ParseFilterPredicate(condition);

            DbUpdateExpression e = new DbUpdateExpression(explicitDbTable ?? typeDescriptor.Table, conditionExp);

            foreach (var kv in updateColumns)
            {
                MemberInfo key = kv.Key;
                PropertyDescriptor propertyDescriptor = typeDescriptor.TryGetPropertyDescriptor(key);

                if (propertyDescriptor == null)
                    throw new ChloeException(string.Format("The member '{0}' does not map any column.", key.Name));

                if (propertyDescriptor.IsPrimaryKey)
                    throw new ChloeException(string.Format("Could not update the primary key '{0}'.", propertyDescriptor.Column.Name));

                bool hasSequence = propertyDescriptor.HasSequence();
                if (hasSequence)
                    throw new ChloeException(string.Format("Could not update the column '{0}', because it's mapping member has define a sequence.", propertyDescriptor.Column.Name));

                e.UpdateColumns.Add(propertyDescriptor.Column, expressionParser.Parse(kv.Value));
            }

            if (e.UpdateColumns.Count == 0)
                return 0;

            return this.ExecuteSqlCommand(e);
        }

        int ExecuteSqlCommand(DbExpression e)
        {
            List<DbParam> parameters;
            return this.ExecuteSqlCommand(e, out parameters);
        }
        int ExecuteSqlCommand(DbExpression e, out List<DbParam> parameters)
        {
            IDbExpressionTranslator translator = this.DatabaseProvider.CreateDbExpressionTranslator();
            string cmdText = translator.Translate(e, out parameters);

            int r = this.Session.ExecuteNonQuery(cmdText, parameters.ToArray());
            return r;
        }

        string AppendInsertRangeSqlTemplate(DbTable table, List<PropertyDescriptor> mappingPropertyDescriptors, bool keepIdentity)
        {
            StringBuilder sqlBuilder = new StringBuilder();

            sqlBuilder.Append("INSERT INTO ");
            sqlBuilder.Append(this.AppendTableName(table));
            sqlBuilder.Append("(");

            for (int i = 0; i < mappingPropertyDescriptors.Count; i++)
            {
                PropertyDescriptor mappingPropertyDescriptor = mappingPropertyDescriptors[i];
                if (i > 0)
                    sqlBuilder.Append(",");
                sqlBuilder.Append(this.QuoteName(mappingPropertyDescriptor.Column.Name));
            }

            sqlBuilder.Append(")");

            if (keepIdentity == false)
            {
                sqlBuilder.Append(" SELECT ");
                for (int i = 0; i < mappingPropertyDescriptors.Count; i++)
                {
                    PropertyDescriptor mappingPropertyDescriptor = mappingPropertyDescriptors[i];
                    if (i > 0)
                        sqlBuilder.Append(",");

                    if (mappingPropertyDescriptor.HasSequence())
                        sqlBuilder.AppendFormat("{0}.{1}", this.QuoteName(mappingPropertyDescriptor.Definition.SequenceName), this.QuoteName("NEXTVAL"));
                    else
                    {
                        sqlBuilder.Append("C").Append(i.ToString());
                    }
                }
                sqlBuilder.Append(" FROM (");
            }

            string sqlTemplate = sqlBuilder.ToString();
            return sqlTemplate;
        }
        string AppendTableName(DbTable table)
        {
            if (string.IsNullOrEmpty(table.Schema))
                return this.QuoteName(table.Name);

            return string.Format("{0}.{1}", this.QuoteName(table.Schema), this.QuoteName(table.Name));
        }
        string QuoteName(string name)
        {
            if (this.ConvertToUppercase)
                return string.Concat("\"", name.ToUpper(), "\"");

            return string.Concat("\"", name, "\"");
        }
    }
}
