using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Chloe.Data;

namespace Chloe.Mapper.Activators
{
    public class RootEntityActivator : IObjectActivator
    {
        IObjectActivator _entityActivator;
        IFitter _fitter;
        IEntityRowCompare _entityRowCompare;

        public RootEntityActivator(IObjectActivator entityActivator, IFitter fitter, IEntityRowCompare entityRowCompare)
        {
            this._entityActivator = entityActivator;
            this._fitter = fitter;
            this._entityRowCompare = entityRowCompare;
        }

        public object CreateInstance(IDataReader reader)
        {
            var entity = this._entityActivator.CreateInstance(reader);

            //导航属性
            this._fitter.Fill(entity, null, reader);

            IQueryDataReader queryDataReader = (IQueryDataReader)reader;
            queryDataReader.AllowReadNextRecord = true;

            while (queryDataReader.Read())
            {
                if (!_entityRowCompare.IsEntityRow(entity, reader))
                {
                    queryDataReader.AllowReadNextRecord = false;
                    break;
                }

                this._fitter.Fill(entity, null, reader);
            }

            return entity;
        }
    }
}
