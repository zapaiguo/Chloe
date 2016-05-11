using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Mapper
{
    public class MappingMemberBinder : IValueSetter
    {
        Action<object, IDataReader, int> _setter;
        int _ordinal;
        public MappingMemberBinder(Action<object, IDataReader, int> setter, int ordinal)
        {
            this._setter = setter;
            this._ordinal = ordinal;
        }
        public void SetValue(object obj, IDataReader reader)
        {
            this._setter(obj, reader, this._ordinal);
        }
    }
}
