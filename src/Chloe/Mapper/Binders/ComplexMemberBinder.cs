using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Mapper.Binders
{
    public class ComplexMemberBinder : MemberBinder, IMemberBinder
    {
        public ComplexMemberBinder(MemberValueSetter setter, IObjectActivator activtor) : base(setter, activtor)
        {
        }
    }
}
