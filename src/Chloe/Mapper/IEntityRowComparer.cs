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
        object _entity;
        object[] _keyValues;
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
            this._keyValues = new object[keys.Count];
        }

        public bool IsEntityRow(object entity, IDataReader reader)
        {
            object[] keyValues = this.GetKeyValues(entity);

            for (int i = 0; i < this._keys.Count; i++)
            {
                object keyValue = keyValues[i];

                if (keyValue == null)
                    return false;

                var tuple = this._keys[i];
                PropertyDescriptor propertyDescriptor = tuple.Item1;
                int ordinal = tuple.Item2;
                var valueGetter = tuple.Item3;
                object keyReaderValue = valueGetter(reader, ordinal);

                if (keyReaderValue == null || keyReaderValue == DBNull.Value)
                    return false;

                keyReaderValue = PublicHelper.ConvertObjectType(keyReaderValue, propertyDescriptor.PropertyType);

                if (!keyValue.Equals(keyReaderValue))
                    return false;
            }

            return true;
        }

        object[] GetKeyValues(object entity)
        {
            if (object.ReferenceEquals(entity, this._entity))
            {
                return this._keyValues;
            }

            for (int i = 0; i < this._keys.Count; i++)
            {
                var tuple = this._keys[i];
                PropertyDescriptor propertyDescriptor = tuple.Item1;
                object keyValue = propertyDescriptor.GetValue(entity);

                this._keyValues[i] = keyValue;
            }

            this._entity = entity;

            return this._keyValues;
        }
    }
}
