using Chloe.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Mapper.Activators
{
    public class PrimitiveObjectActivator : IObjectActivator
    {
        Func<IDataReader, int, object> _fn = null;
        int _readerOrdinal;
        public PrimitiveObjectActivator(Func<IDataReader, int, object> fn, int readerOrdinal)
        {
            this._fn = fn;
            this._readerOrdinal = readerOrdinal;
        }
        public object CreateInstance(IDataReader reader)
        {
            try
            {
                return _fn(reader, _readerOrdinal);
            }
            catch (Exception ex)
            {
                throw new ChloeException(ComplexObjectActivator.AppendErrorMsg(reader, this._readerOrdinal, ex), ex);
            }
        }
    }
}
