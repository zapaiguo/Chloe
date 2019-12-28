using Chloe.Data;
using Chloe.Exceptions;
using Chloe.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Mapper.Activators
{
    public class PrimitiveObjectActivator : IObjectActivator
    {
        Type _primitiveType;
        IDbValueReader _dbValueReader = null;
        int _readerOrdinal;
        public PrimitiveObjectActivator(Type primitiveType, int readerOrdinal)
        {
            this._primitiveType = primitiveType;
            this._readerOrdinal = readerOrdinal;
            this._dbValueReader = DataReaderConstant.GetDbValueReader(primitiveType);
        }
        public void Prepare(IDataReader reader)
        {

        }
        public object CreateInstance(IDataReader reader)
        {
            try
            {
                return this._dbValueReader.GetValue(reader, _readerOrdinal);
            }
            catch (Exception ex)
            {
                throw new ChloeException(ComplexObjectActivator.AppendErrorMsg(reader, this._readerOrdinal, ex), ex);
            }
        }
    }
}
