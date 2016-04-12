using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    internal interface ICreate<T>
    {
        T Create(IDataReader reader);
    }
}
