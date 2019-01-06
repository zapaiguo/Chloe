using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Infrastructure
{
    public class MappingType<T> : MappingTypeBase
    {
        DbType _dbType;
        public MappingType()
        {
        }
        public MappingType(DbType dbType)
        {
            this._dbType = dbType;
        }
        public override Type Type
        {
            get
            {
                return typeof(T);
            }
        }
        public override DbType DbType
        {
            get
            {
                return this._dbType;
            }
        }
        public override IDbDataParameter CreateDataParameter(IDbCommand cmd, DbParam param)
        {
            return base.CreateDataParameter(cmd, param);
        }
        public override object ReadFromDataReader(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            var value = reader.GetValue(ordinal);

            //数据库字段类型与属性类型不一致，则转换类型
            if (value.GetType() != this.Type)
            {
                value = Convert.ChangeType(value, this.Type);
            }

            return value;
        }
    }
}
