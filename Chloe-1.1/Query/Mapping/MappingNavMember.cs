using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Mapping
{
    public class MappingNavMember : MappingMember
    {
        public MappingNavMember(Type entityType)
            : base(entityType)
        {

        }
        public MemberInfo AssociatingMemberInfo { get; set; }
    }
}
