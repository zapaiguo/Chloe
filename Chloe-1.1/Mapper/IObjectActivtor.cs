using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Mapper
{
    public interface IObjectActivtor
    {
        object CreateInstance(IDataReader reader);
    }
}
