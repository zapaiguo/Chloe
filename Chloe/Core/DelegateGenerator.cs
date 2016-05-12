using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Chloe.Query;
using Chloe.Extensions;
using Chloe.Utility;
using Chloe.Mapper;
using System.Linq.Expressions;

namespace Chloe.Core
{
    public static class DelegateGenerator
    {
        public static Action<object, IDataReader, int> CreateSetValueFromReaderDelegate(MemberInfo member)
        {
            Action<object, IDataReader, int> del = null;

            DynamicMethod dm = new DynamicMethod("SetValueFromReader_" + Guid.NewGuid().ToString(), null, new Type[] { typeof(object), typeof(IDataReader), typeof(int) }, true);
            ILGenerator il = dm.GetILGenerator();

            il.Emit(OpCodes.Ldarg_S, 0);//将第一个参数 object 对象加载到栈顶
            il.Emit(OpCodes.Castclass, member.DeclaringType);//将 object 对象转换为强类型对象 此时栈顶为强类型的对象

            var readerMethod = GetReaderMethod(member.GetPropertyOrFieldType());

            //ordinal
            il.Emit(OpCodes.Ldarg_S, 1);    //加载参数DataReader
            il.Emit(OpCodes.Ldarg_S, 2);    //加载 read ordinal
            il.EmitCall(OpCodes.Call, readerMethod, null);     //调用对应的 readerMethod 得到 value  reader.Getxx(ordinal);  此时栈顶为 value

            SetValueIL(il, member); // object.XX = value; 此时栈顶为空

            il.Emit(OpCodes.Ret);   // 即可 return

            del = (Action<object, IDataReader, int>)dm.CreateDelegate(typeof(Action<object, IDataReader, int>));
            return del;
        }

        static void SetValueIL(ILGenerator il, MemberInfo member)
        {
            MemberTypes memberType = member.MemberType;
            if (memberType == MemberTypes.Property)
            {
                MethodInfo setter = ((PropertyInfo)member).GetSetMethod();
                il.EmitCall(OpCodes.Callvirt, setter, null);//给属性赋值
            }
            else if (memberType == MemberTypes.Field)
            {
                il.Emit(OpCodes.Stfld, ((FieldInfo)member));//给字段赋值
            }
            else
                throw new Exception("- -");
        }
        static void SetValueIL(ILGenerator il, PropertyInfo property)
        {
            MethodInfo setter = property.GetSetMethod();
            il.EmitCall(OpCodes.Callvirt, setter, null);//给属性赋值
        }
        static void SetValueIL(ILGenerator il, FieldInfo field)
        {
            il.Emit(OpCodes.Stfld, field);//给字段赋值
        }

        public static Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> CreateObjectGenerator(ConstructorInfo constructor)
        {
            Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object> ret = null;

            var pExp_reader = Expression.Parameter(typeof(IDataReader), "reader");
            var pExp_readerOrdinalEnumerator = Expression.Parameter(typeof(ReaderOrdinalEnumerator), "readerOrdinalEnumerator");
            var pExp_objectActivatorEnumerator = Expression.Parameter(typeof(ObjectActivatorEnumerator), "objectActivatorEnumerator");

            ParameterInfo[] parameters = constructor.GetParameters();
            List<Expression> arguments = new List<Expression>(parameters.Length);

            foreach (ParameterInfo parameter in parameters)
            {
                if (Utils.IsMapType(parameter.ParameterType))
                {
                    var readerMethod = GetReaderMethod(parameter.ParameterType);
                    //int ordinal = readerOrdinalEnumerator.Next();
                    var readerOrdinal = Expression.Call(pExp_readerOrdinalEnumerator, ReaderOrdinalEnumerator.NextMethodInfo);
                    //DataReaderExtensions.GetValue(reader,readerOrdinal)
                    var getValue = Expression.Call(readerMethod, pExp_reader, readerOrdinal);
                    arguments.Add(getValue);
                }
                else
                {
                    //IObjectActivator oa = objectActivatorEnumerator.Next();
                    var oa = Expression.Call(pExp_objectActivatorEnumerator, ObjectActivatorEnumerator.NextMethodInfo);
                    //object obj = oa.CreateInstance(IDataReader reader);
                    var entity = Expression.Call(oa, typeof(IObjectActivator).GetMethod("CreateInstance"), pExp_reader);
                    //(T)entity;
                    var val = Expression.Convert(entity, parameter.ParameterType);
                    arguments.Add(val);
                }
            }

            var body = Expression.New(constructor, arguments);

            ret = Expression.Lambda<Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivatorEnumerator, object>>(body, pExp_reader, pExp_readerOrdinalEnumerator, pExp_objectActivatorEnumerator).Compile();

            return ret;
        }
        public static Func<IDataReader, int, object> CreateMappingTypeGenerator(Type type)
        {
            var pExp_reader = Expression.Parameter(typeof(IDataReader), "reader");
            var pExp_readerOrdinal = Expression.Parameter(typeof(int), "readerOrdinal");

            var readerMethod = GetReaderMethod(type);
            var getValue = Expression.Call(readerMethod, pExp_reader, pExp_readerOrdinal);
            var body = Expression.Convert(getValue, typeof(object));

            Func<IDataReader, int, object> ret = Expression.Lambda<Func<IDataReader, int, object>>(body, pExp_reader, pExp_readerOrdinal).Compile();

            return ret;
        }

        public static Action<object, object> CreateValueSetter(MemberInfo propertyOrField)
        {
            PropertyInfo propertyInfo = propertyOrField as PropertyInfo;
            if (propertyInfo != null)
                return CreateValueSetter(propertyInfo);

            FieldInfo fieldInfo = propertyOrField as FieldInfo;
            if (fieldInfo != null)
                return CreateValueSetter(fieldInfo);

            throw new ArgumentException();
        }
        public static Action<object, object> CreateValueSetter(PropertyInfo propertyInfo)
        {
            var p = Expression.Parameter(typeof(object), "instance");
            var pValue = Expression.Parameter(typeof(object), "value");
            var instance = Expression.Convert(p, propertyInfo.DeclaringType);
            var value = Expression.Convert(pValue, propertyInfo.PropertyType);

            var pro = Expression.Property(instance, propertyInfo);
            var setValue = Expression.Assign(pro, value);

            Expression body = setValue;

            var lambda = Expression.Lambda<Action<object, object>>(body, p, pValue);
            Action<object, object> ret = lambda.Compile();

            return ret;
        }
        public static Action<object, object> CreateValueSetter(FieldInfo fieldInfo)
        {
            var p = Expression.Parameter(typeof(object), "instance");
            var pValue = Expression.Parameter(typeof(object), "value");
            var instance = Expression.Convert(p, fieldInfo.DeclaringType);
            var value = Expression.Convert(pValue, fieldInfo.FieldType);

            var field = Expression.Field(instance, fieldInfo);
            var setValue = Expression.Assign(field, value);

            Expression body = setValue;

            var lambda = Expression.Lambda<Action<object, object>>(body, p, pValue);
            Action<object, object> ret = lambda.Compile();

            return ret;
        }
        public static Func<object, object> CreateValueGetter(MemberInfo member)
        {
            var p = Expression.Parameter(typeof(object), "a");
            var instance = Expression.Convert(p, member.DeclaringType);
            var memberAccess = Expression.MakeMemberAccess(instance, member);

            Type type = ReflectionExtensions.GetMemberInfoType(member);

            Expression body = memberAccess;
            if (type.IsValueType)
            {
                body = Expression.Convert(memberAccess, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object, object>>(body, p);
            Func<object, object> ret = lambda.Compile();

            return ret;
        }


        #region
        internal static MethodInfo GetReaderMethod(Type type)
        {

            MethodInfo result;
            bool isNullable = false;
            Type underlyingType;
            if (Utils.IsNullable(type, out underlyingType))
            {
                isNullable = true;
                type = underlyingType;
            }

            if (type.IsEnum)
            {
                result = (isNullable ? Reader_GetEnum_Nullable : Reader_GetEnum).MakeGenericMethod(type);
                return result;
            }

            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Int16:
                    result = isNullable ? Reader_GetInt16_Nullable : Reader_GetInt16;
                    break;
                case TypeCode.Int32:
                    result = isNullable ? Reader_GetInt32_Nullable : Reader_GetInt32;
                    break;
                case TypeCode.Int64:
                    result = isNullable ? Reader_GetInt64_Nullable : Reader_GetInt64;
                    break;
                case TypeCode.Decimal:
                    result = isNullable ? Reader_GetDecimal_Nullable : Reader_GetDecimal;
                    break;
                case TypeCode.Double:
                    result = isNullable ? Reader_GetDouble_Nullable : Reader_GetDouble;
                    break;
                case TypeCode.Single:
                    result = isNullable ? Reader_GetFloat_Nullable : Reader_GetFloat;
                    break;
                case TypeCode.Boolean:
                    result = isNullable ? Reader_GetBoolean_Nullable : Reader_GetBoolean;
                    break;
                case TypeCode.DateTime:
                    result = isNullable ? Reader_GetDateTime_Nullable : Reader_GetDateTime;
                    break;
                case TypeCode.Byte:
                    result = isNullable ? Reader_GetByte_Nullable : Reader_GetByte;
                    break;
                case TypeCode.Char:
                    result = isNullable ? Reader_GetChar_Nullable : Reader_GetChar;
                    break;
                case TypeCode.String:
                    result = Reader_GetString;
                    break;
                default:
                    if (type == UtilConstants.TypeOfGuid)
                    {
                        result = isNullable ? Reader_GetGuid_Nullable : Reader_GetGuid;
                    }
                    else if (type == UtilConstants.TypeOfObject)
                    {
                        result = Reader_GetValue;
                    }
                    else
                    {
                        result = (isNullable ? Reader_GetValue_NullableT : Reader_GetValue_T).MakeGenericMethod(type);
                    }
                    break;
            }
            return result;
        }

        #region
        internal static readonly MethodInfo Reader_GetInt16 = typeof(DataReaderExtensions).GetMethod("Reader_GetInt16");
        internal static readonly MethodInfo Reader_GetInt16_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetInt16_Nullable");
        internal static readonly MethodInfo Reader_GetInt32 = typeof(DataReaderExtensions).GetMethod("Reader_GetInt32");
        internal static readonly MethodInfo Reader_GetInt32_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetInt32_Nullable");
        internal static readonly MethodInfo Reader_GetInt64 = typeof(DataReaderExtensions).GetMethod("Reader_GetInt64");
        internal static readonly MethodInfo Reader_GetInt64_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetInt64_Nullable");
        internal static readonly MethodInfo Reader_GetDecimal = typeof(DataReaderExtensions).GetMethod("Reader_GetDecimal");
        internal static readonly MethodInfo Reader_GetDecimal_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetDecimal_Nullable");
        internal static readonly MethodInfo Reader_GetDouble = typeof(DataReaderExtensions).GetMethod("Reader_GetDouble");
        internal static readonly MethodInfo Reader_GetDouble_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetDouble_Nullable");
        internal static readonly MethodInfo Reader_GetFloat = typeof(DataReaderExtensions).GetMethod("Reader_GetFloat");
        internal static readonly MethodInfo Reader_GetFloat_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetFloat_Nullable");
        internal static readonly MethodInfo Reader_GetBoolean = typeof(DataReaderExtensions).GetMethod("Reader_GetBoolean");
        internal static readonly MethodInfo Reader_GetBoolean_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetBoolean_Nullable");
        internal static readonly MethodInfo Reader_GetDateTime = typeof(DataReaderExtensions).GetMethod("Reader_GetDateTime");
        internal static readonly MethodInfo Reader_GetDateTime_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetDateTime_Nullable");
        internal static readonly MethodInfo Reader_GetGuid = typeof(DataReaderExtensions).GetMethod("Reader_GetGuid");
        internal static readonly MethodInfo Reader_GetGuid_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetGuid_Nullable");
        internal static readonly MethodInfo Reader_GetByte = typeof(DataReaderExtensions).GetMethod("Reader_GetByte");
        internal static readonly MethodInfo Reader_GetByte_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetByte_Nullable");
        internal static readonly MethodInfo Reader_GetChar = typeof(DataReaderExtensions).GetMethod("Reader_GetChar");
        internal static readonly MethodInfo Reader_GetChar_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetChar_Nullable");
        internal static readonly MethodInfo Reader_GetString = typeof(DataReaderExtensions).GetMethod("Reader_GetString");
        internal static readonly MethodInfo Reader_GetValue = typeof(DataReaderExtensions).GetMethod("Reader_GetValue");

        internal static readonly MethodInfo Reader_GetEnum = typeof(DataReaderExtensions).GetMethod("Reader_GetEnum");
        internal static readonly MethodInfo Reader_GetEnum_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetEnum_Nullable");

        internal static readonly MethodInfo Reader_GetValue_T = typeof(DataReaderExtensions).GetMethod("Reader_GetValue_T");
        internal static readonly MethodInfo Reader_GetValue_NullableT = typeof(DataReaderExtensions).GetMethod("Reader_GetValue_NullableT");
        #endregion

        #endregion

    }
}
