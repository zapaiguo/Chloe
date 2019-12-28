using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.Mapper
{
    public delegate object InstanceCreator(IDataReader reader, ObjectActivatorEnumerator argumentActivatorEnumerator);
}
