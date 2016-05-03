using Chloe.Core;
using Chloe.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Chloe.Query.Mapping
{
    public class MappingField : IObjectActivatorCreator
    {
        Type _type;
        public MappingField(Type type, int readerOrdinal)
        {
            this._type = type;
            this.ReaderOrdinal = readerOrdinal;
        }
        public int ReaderOrdinal { get; private set; }

        public IObjectActivator CreateObjectActivator()
        {
            Func<IDataReader, int, object> fn = MappingTypeConstructor.GetInstance(this._type).InstanceCreator;
            MappingFieldActivator act = new MappingFieldActivator(fn, this.ReaderOrdinal);
            return act;
        }
    }
}
