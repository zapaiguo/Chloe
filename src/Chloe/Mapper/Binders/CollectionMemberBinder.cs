using Chloe.Reflection;

namespace Chloe.Mapper.Binders
{
    public class CollectionMemberBinder : MemberBinder, IMemberBinder
    {
        public CollectionMemberBinder(MemberValueSetter setter, IObjectActivator activtor) : base(setter, activtor)
        {

        }
    }
}
