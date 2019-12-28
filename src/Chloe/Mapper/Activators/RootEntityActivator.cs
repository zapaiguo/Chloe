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
        IEntityRowComparer _entityRowComparer;

        public RootEntityActivator(IObjectActivator entityActivator, IFitter fitter, IEntityRowComparer entityRowComparer)
        {
            this._entityActivator = entityActivator;
            this._fitter = fitter;
            this._entityRowComparer = entityRowComparer;
        }

        public void Prepare(IDataReader reader)
        {
            this._entityActivator.Prepare(reader);
            this._fitter.Prepare(reader);
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
                if (!_entityRowComparer.IsEntityRow(entity, reader))
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
