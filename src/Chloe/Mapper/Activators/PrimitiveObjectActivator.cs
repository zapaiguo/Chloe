using Chloe.Data;
using Chloe.Exceptions;
using System;
using System.Data;

namespace Chloe.Mapper.Activators
{
    public class PrimitiveObjectActivator : IObjectActivator
    {
        Type _primitiveType;
        int _readerOrdinal;
        IDbValueReader _dbValueReader;

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
