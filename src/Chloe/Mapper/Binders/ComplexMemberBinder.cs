using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Mapper.Binders
{
    public class ComplexMemberBinder : MemberBinder, IValueSetter
    {
        public ComplexMemberBinder(Action<object, object> setter, IObjectActivator activtor) : base(setter, activtor)
        {
        }
    }
}
