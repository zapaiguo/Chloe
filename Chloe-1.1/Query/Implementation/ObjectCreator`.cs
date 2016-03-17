using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Implementation
{
    internal class ObjectCreator<T> : ICreate<T>
    {
        private ObjectCreateContext objectCreateContext;
        public ObjectCreator(ObjectCreateContext context)
        {
            objectCreateContext = context;
        }
        public T Create(IDataReader reader)
        {
            return (T)objectCreateContext.CreateObject(reader);
        }

    }
}
