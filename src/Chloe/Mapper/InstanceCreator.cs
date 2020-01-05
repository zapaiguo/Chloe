using System.Collections.Generic;
using System.Data;

namespace Chloe.Mapper
{
    public delegate object InstanceCreator(IDataReader reader, List<IObjectActivator> argumentActivators);
}
