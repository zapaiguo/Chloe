using Chloe.Reflection;

namespace Chloe.Mapper.Binders
{
    public class ComplexMemberBinder : MemberBinder, IMemberBinder
    {
        public ComplexMemberBinder(MemberValueSetter setter, IObjectActivator activtor) : base(setter, activtor)
        {
        }
    }
}
