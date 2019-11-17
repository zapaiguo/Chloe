using Chloe.Descriptors;
using Chloe.Extensions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.Mapper
{
    public interface IEntityRowCompare
    {
        bool IsEntityRow(object entity, IDataReader reader);
    }
    public class EntityRowCompare : IEntityRowCompare
    {
        List<Tuple<PropertyDescriptor, int>> _keys;
        public EntityRowCompare(List<Tuple<PropertyDescriptor, int>> keys)
        {
            this._keys = keys;
        }

        public bool IsEntityRow(object entity, IDataReader reader)
        {
            for (int i = 0; i < this._keys.Count; i++)
            {
                var kv = this._keys[i];
                object keyReaderValue = DataReaderConstant.GetGetValueHandler(kv.Item1.PropertyType)(reader, kv.Item2);
                keyReaderValue = keyReaderValue == DBNull.Value ? null : keyReaderValue;

                if (keyReaderValue == null)
                    return false;

                keyReaderValue = PublicHelper.ConvertObjectType(keyReaderValue, kv.Item1.PropertyType);
                object keyValue = kv.Item1.GetValue(entity);

                if (keyValue == null)
                    return false;

                if (!keyValue.Equals(keyReaderValue))
                    return false;
            }

            return true;
        }
    }
}
