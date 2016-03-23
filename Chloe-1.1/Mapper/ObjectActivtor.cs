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
        Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivtorEnumerator, object> _instanceCreator;
        List<int> _readerOrdinals;
        List<IObjectActivtor> _objectActivtors;
        List<IValueSetter> _memberSetters;
        public ObjectActivtor(Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivtorEnumerator, object> instanceCreator, List<int> readerOrdinals, List<IObjectActivtor> objectActivtors, List<IValueSetter> memberSetters)
        {
            this._instanceCreator = instanceCreator;
            this._readerOrdinals = readerOrdinals;
            this._objectActivtors = objectActivtors;
            this._memberSetters = memberSetters;
        }

        public object CreateInstance(IDataReader reader)
        {
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
