using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Mapper.Binders
{

    public class CollectionMemberBinder : MemberBinder, IValueSetter
    {
        public CollectionMemberBinder(Action<object, object> setter, IObjectActivator activtor) : base(setter, activtor)
        {

        }
    }
}
