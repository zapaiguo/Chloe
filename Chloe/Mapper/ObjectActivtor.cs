using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Mapper
{
    public class ObjectActivtor : IObjectActivtor
    {
        int? _checkNullOrdinal;
        Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivtorEnumerator, object> _instanceCreator;
        List<int> _readerOrdinals;
        List<IObjectActivtor> _objectActivtors;
        List<IValueSetter> _memberSetters;
        public ObjectActivtor(Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivtorEnumerator, object> instanceCreator, List<int> readerOrdinals, List<IObjectActivtor> objectActivtors, List<IValueSetter> memberSetters, int? checkNullOrdinal)
        {
            this._instanceCreator = instanceCreator;
            this._readerOrdinals = readerOrdinals;
            this._objectActivtors = objectActivtors;
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
            ObjectActivtorEnumerator objectActivtorEnumerator = new ObjectActivtorEnumerator(this._objectActivtors);

            object obj = this._instanceCreator(reader, readerOrdinalEnumerator, objectActivtorEnumerator);

            foreach (IValueSetter memberSetter in this._memberSetters)
            {
                memberSetter.SetValue(obj, reader);
            }

            return obj;
        }
    }
}
