using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Mapper
{
    public interface IObjectActivator
    {
        void Prepare(IDataReader reader);
        object CreateInstance(IDataReader reader);
    }
}
