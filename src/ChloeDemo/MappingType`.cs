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
        Type _type;
        public MappingType()
        {
            this._type = typeof(T);
        }
        public MappingType(DbType dbType)
        {
            this._dbType = dbType;
            this._type = typeof(T);
        }
        public override Type Type
        {
            get
            {
                return this._type;
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
            var value = reader.GetValue(ordinal);

            if (value is DBNull)
                return null;

            //数据库字段类型与属性类型不一致，则转换类型
            if (value.GetType() != this.Type)
            {
                value = Convert.ChangeType(value, this.Type);
            }

            return value;
        }
    }
}
