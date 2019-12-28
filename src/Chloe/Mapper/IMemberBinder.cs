using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Mapper
{
    public interface IMemberBinder
    {
        void Prepare(IDataReader reader);
        void Bind(object obj, IDataReader reader);
    }
}
