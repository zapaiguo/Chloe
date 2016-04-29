using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Oracle
{
    internal static class MemberInfoExtensions
    {
        public static Type GetMemberInfoType(this MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).PropertyType;
            if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).FieldType;
            if (member is MethodInfo)
                return ((MethodInfo)member).ReturnType;
            if (member is ConstructorInfo)
                return ((ConstructorInfo)member).ReflectedType;
            //throw new ArgumentException();
            return null;
        }
    }
}
