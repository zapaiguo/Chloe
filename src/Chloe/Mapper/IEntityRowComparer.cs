using Chloe.Descriptors;
using Chloe.Extensions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.Mapper
{
    public interface IEntityRowComparer
    {
        bool IsEntityRow(object entity, IDataReader reader);
    }
    public class EntityRowComparer : IEntityRowComparer
    {
        List<Tuple<PropertyDescriptor, int, Func<IDataReader, int, object>>> _keys;
        public EntityRowComparer(List<Tuple<PropertyDescriptor, int>> keys)
        {
            List<Tuple<PropertyDescriptor, int, Func<IDataReader, int, object>>> keyList = new List<Tuple<PropertyDescriptor, int, Func<IDataReader, int, object>>>(keys.Count);
            for (int i = 0; i < keys.Count; i++)
            {
                var tuple = keys[i];
                Func<IDataReader, int, object> valueGetter = DataReaderConstant.GetGetValueHandler(tuple.Item1.PropertyType);

                keyList.Add(new Tuple<PropertyDescriptor, int, Func<IDataReader, int, object>>(tuple.Item1, tuple.Item2, valueGetter));
            }

            this._keys = keyList;
        }

        public bool IsEntityRow(object entity, IDataReader reader)
        {
            for (int i = 0; i < this._keys.Count; i++)
            {
                var tuple = this._keys[i];
                PropertyDescriptor propertyDescriptor = tuple.Item1;
                int ordinal = tuple.Item2;
                var valueGetter = tuple.Item3;
                object keyReaderValue = valueGetter(reader, ordinal);
                keyReaderValue = keyReaderValue == DBNull.Value ? null : keyReaderValue;

                if (keyReaderValue == null)
                    return false;

                keyReaderValue = PublicHelper.ConvertObjectType(keyReaderValue, propertyDescriptor.PropertyType);
                object keyValue = propertyDescriptor.GetValue(entity);

                if (keyValue == null)
                    return false;

                if (!keyValue.Equals(keyReaderValue))
                    return false;
            }

            return true;
        }
    }
}
