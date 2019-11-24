using Chloe.Descriptors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.Mapper
{
    /// <summary>
    /// 集合填充器
    /// </summary>
    public interface IFitter
    {
        void Fill(object obj, object owner, IDataReader reader);
    }

    public class ComplexObjectFitter : IFitter
    {
        List<Tuple<PropertyDescriptor, IFitter>> _includings;

        public ComplexObjectFitter(List<Tuple<PropertyDescriptor, IFitter>> includings)
        {
            this._includings = includings;
        }

        public void Fill(object entity, object owner, IDataReader reader)
        {
            for (int i = 0; i < this._includings.Count; i++)
            {
                var kv = this._includings[i];

                var propertyValue = kv.Item1.GetValue(entity);
                if (propertyValue == null)
                    continue;

                kv.Item2.Fill(propertyValue, entity, reader);
            }
        }
    }
    public class CollectionObjectFitter : IFitter
    {
        IObjectActivator _elementActivator;
        IEntityRowCompare _entityRowCompare;
        IFitter _elementFitter;
        PropertyDescriptor _elementOwnerProperty;

        public CollectionObjectFitter(IObjectActivator elementActivator, IEntityRowCompare entityRowCompare, IFitter elementFitter, PropertyDescriptor elementOwnerProperty)
        {
            this._elementActivator = elementActivator;
            this._entityRowCompare = entityRowCompare;
            this._elementFitter = elementFitter;
            this._elementOwnerProperty = elementOwnerProperty;
        }

        public void Fill(object collection, object owner, IDataReader reader)
        {
            IList entityContainer = collection as IList;

            object entity = null;
            if (entityContainer.Count > 0)
                entity = entityContainer[entityContainer.Count];

            if (entity == null || !this._entityRowCompare.IsEntityRow(entity, reader))
            {
                entity = this._elementActivator.CreateInstance(reader);

                if (entity == null)
                    return;

                this._elementOwnerProperty.SetValue(entity, owner); //entity.XX = owner
                entityContainer.Add(entity);
            }

            this._elementFitter.Fill(entity, null, reader);
        }
    }

}
