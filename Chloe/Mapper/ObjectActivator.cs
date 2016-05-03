using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Mapper
{
    public class ObjectActivator : IObjectActivator
    {
        int? _checkNullOrdinal;
        Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> _instanceCreator;
        List<int> _readerOrdinals;
        List<IObjectActivator> _objectActivators;
        List<IValueSetter> _memberSetters;
        public ObjectActivator(Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> instanceCreator, List<int> readerOrdinals, List<IObjectActivator> objectActivators, List<IValueSetter> memberSetters, int? checkNullOrdinal)
        {
            this._instanceCreator = instanceCreator;
            this._readerOrdinals = readerOrdinals;
            this._objectActivators = objectActivators;
            this._memberSetters = memberSetters;
            this._checkNullOrdinal = checkNullOrdinal;
        }

        public object CreateInstance(IDataReader reader)
        {
            if (this._checkNullOrdinal != null)
            {
                if (reader.IsDBNull(this._checkNullOrdinal.Value))
                    return null;
            }

            ReaderOrdinalEnumerator readerOrdinalEnumerator = new ReaderOrdinalEnumerator(this._readerOrdinals);
            ObjectActivatorEnumerator objectActivatorEnumerator = new ObjectActivatorEnumerator(this._objectActivators);

            object obj = this._instanceCreator(reader, readerOrdinalEnumerator, objectActivatorEnumerator);

            foreach (IValueSetter memberSetter in this._memberSetters)
            {
                memberSetter.SetValue(obj, reader);
            }

            return obj;
        }
    }
}
