using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Mapper
{
    public class MappingFieldActivator : IObjectActivator
    {
        Func<IDataReader, int, object> _fn = null;
        int _readerOrdinal;
        public MappingFieldActivator(Func<IDataReader, int, object> fn, int readerOrdinal)
        {
            this._fn = fn;
            this._readerOrdinal = readerOrdinal;
        }
        public object CreateInstance(IDataReader reader)
        {
            return _fn(reader, _readerOrdinal);
        }
    }
}
