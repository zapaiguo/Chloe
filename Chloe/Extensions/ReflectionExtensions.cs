using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Extensions
{
    internal static class ReflectionExtensions
    {
        public static Type GetMemberInfoType(this MemberInfo member)
        {
            if (member == null)
                throw new ArgumentNullException("member");

            if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).PropertyType;
            if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).FieldType;
            if (member is MethodInfo)
                return ((MethodInfo)member).ReturnType;
            if (member is ConstructorInfo)
                return ((ConstructorInfo)member).ReflectedType;

            return null;
        }

        /// <summary>
        /// 获取属性或者字段的类型，如果 MemberInfo 非 PropertyInfo 或 FieldInfo 则引发 NotSupportedException
        /// </summary>
        /// <param name="propertyOrField"></param>
        /// <returns></returns>
        public static Type GetPropertyOrFieldType(this MemberInfo propertyOrField)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
                return ((PropertyInfo)propertyOrField).PropertyType;
            if (propertyOrField.MemberType == MemberTypes.Field)
                return ((FieldInfo)propertyOrField).FieldType;

            throw new NotSupportedException("not property or field");
        }

        public static void SetPropertyOrFieldValue(this MemberInfo propertyOrField, object obj, object value)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
                ((PropertyInfo)propertyOrField).SetValue(obj, value);
            else if (propertyOrField.MemberType == MemberTypes.Field)
                ((FieldInfo)propertyOrField).SetValue(obj, value);
            else
                throw new ArgumentException("只支持 FieldInfo 和 PropertyInfo");
        }

        public static object GetPropertyOrFieldValue(this MemberInfo propertyOrField, object obj)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
                return ((PropertyInfo)propertyOrField).GetValue(obj);
            else if (propertyOrField.MemberType == MemberTypes.Field)
                return ((FieldInfo)propertyOrField).GetValue(obj);
            else
                throw new ArgumentException("只支持 FieldInfo 和 PropertyInfo");
        }
    }
}
