using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query;
using Chloe.Extensions;
using Chloe.Utility;

namespace Chloe.Core
{
    internal static class DelegateCreateManage
    {
        #region
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

        //public static object InvokeGenerator(IDataReader reader, ObjectCreateContext context)
        //{
        //    //等于null 表示是导航实体的映射 主实体的selectInfo.AssociationMember=null
        //    if (reader.IsDBNull(context.AssociationMemberReaderOrdinal))
        //    {
        //        return null;
        //    }
        //    return context.CreateObject(reader);
        //}

        //public static Func<IDataReader, ObjectCreateContext, object> CreateObjectGenerator(EntityDescriptor entityDescriptor)
        //{
        //    Type entityType = entityDescriptor.EntityType;
        //    Type createContextType = typeof(ObjectCreateContext);
        //    Type intType = Utils.TypeOfInt32;
        //    Type createContextDicType = typeof(Dictionary<int, ObjectCreateContext>);
        //    Type listIntType = typeof(List<int>);

        //    Func<IDataReader, ObjectCreateContext, object> del;
        //    DynamicMethod dm = new DynamicMethod("CreateObject_" + Guid.NewGuid().ToString(), entityType, new Type[] { typeof(IDataReader), createContextType }, true);
        //    ILGenerator il = dm.GetILGenerator();

        //    //声明变量
        //    var obj = il.DeclareLocal(entityType);   //生成对象变量
        //    var selectedFieldIndexList = il.DeclareLocal(listIntType);   //List<int> selectedFieldIndexList = selectInfo.SelectedFieldIndexList;  select 字段在Generator对应索引
        //    var nextSelectedFieldIndex = il.DeclareLocal(intType);   //int nextSelectedFieldIndex = 0;  //selectedFieldIndexList中下个映射字段索引位置   与readIndex一样 每次GetValue后自增1
        //    var selectedFieldCount = il.DeclareLocal(intType);   //int selectedFieldCount = selectedFieldIndexList.Count; select字段数量
        //    var readIndex = il.DeclareLocal(intType);   //int readIndex = selectInfo.StartReadIndex; 该对象reader的索引 每次GetValue后自增1
        //    var subSelectInfoDic = il.DeclareLocal(createContextDicType);   //导航属性的集合
        //    var subSelectInfo = il.DeclareLocal(createContextType);   // subSelectInfo  镶嵌类的一个临时变量

        //    #region 初始化声明的变量
        //    //初始化声明的变量
        //    il.Emit(OpCodes.Nop);
        //    ConstructorInfo ctor = entityType.GetConstructor(Type.EmptyTypes);
        //    il.Emit(OpCodes.Newobj, ctor);
        //    il.Emit(OpCodes.Stloc_S, obj);
        //    il.Emit(OpCodes.Ldloc_S, obj);   //加载 后面用Dup方式  这样就没必要定义一堆的变量记录GetValue得到的值了

        //    //加载导航属性的集合
        //    il.Emit(OpCodes.Ldarg_S, 1);
        //    var dic_Getter = createContextType.GetProperty("NavMemberObjectCreateContext").GetGetMethod();
        //    il.EmitCall(OpCodes.Callvirt, dic_Getter, null);
        //    il.Emit(OpCodes.Stloc_S, subSelectInfoDic);

        //    //给readIndex赋值
        //    il.Emit(OpCodes.Ldarg_S, 1);
        //    var startReadIndex_Getter = createContextType.GetProperty("StartReadOrdinal").GetGetMethod();
        //    il.EmitCall(OpCodes.Callvirt, startReadIndex_Getter, null);
        //    il.Emit(OpCodes.Stloc_S, readIndex);

        //    //给selectedFieldIndexList赋值
        //    il.Emit(OpCodes.Ldarg_S, 1);
        //    var selectedFieldIndexList_Getter = createContextType.GetProperty("SelectedFieldIndexList").GetGetMethod();
        //    il.EmitCall(OpCodes.Callvirt, selectedFieldIndexList_Getter, null);
        //    il.Emit(OpCodes.Stloc_S, selectedFieldIndexList);

        //    //给selectedFieldCount字段数量赋值
        //    il.Emit(OpCodes.Ldloc_S, selectedFieldIndexList);
        //    var listCount_Getter = listIntType.GetProperty("Count").GetGetMethod();
        //    il.EmitCall(OpCodes.Callvirt, listCount_Getter, null);
        //    il.Emit(OpCodes.Stloc_S, selectedFieldCount);
        //    #endregion

        //    Label lbNav = il.DefineLabel();

        //    #region  映射公共 属性 字段

        //    #region
        //    //int num;
        //    //Cart cart = new Cart();
        //    //Dictionary<int, ObjectCreateContext> navMemberObjectCreateContext = reader1.NavMemberObjectCreateContext;
        //    //ObjectCreateContext context = null;
        //    //int startReadIndex = reader1.StartReadIndex;
        //    //List<int> selectedFieldIndexList = reader1.SelectedFieldIndexList;
        //    //int count = selectedFieldIndexList.Count;
        //    //if (selectedFieldIndexList[num] == 0)
        //    //{
        //    //    cart.RR = ((IDataReader)this).Reader_GetInt32_Nullable(startReadIndex);
        //    //    startReadIndex++;
        //    //    num++;
        //    //    if (num == count)
        //    //    {
        //    //        goto Label_0170;
        //    //    }
        //    //}
        //    //if (selectedFieldIndexList[num] == 1)
        //    //{
        //    //    cart.BoolField = ((IDataReader)this).Reader_GetBoolean_Nullable(startReadIndex);
        //    //    startReadIndex++;
        //    //    num++;
        //    //    if (num == count)
        //    //    {
        //    //        goto Label_0170;
        //    //    }
        //    //}

        //    #endregion

        //    int cepIndex = 0;
        //    //循环MapFieldMemberList
        //    foreach (var item in entityDescriptor.MapMembers)
        //    {
        //        var member = item.Value.MemberInfo;
        //        Label lbNext = il.DefineLabel();

        //        il.Emit(OpCodes.Ldloc_S, selectedFieldIndexList);
        //        il.Emit(OpCodes.Ldloc_S, nextSelectedFieldIndex);
        //        var method_ListItem = listIntType.GetMethod("get_Item", new Type[] { intType });
        //        il.EmitCall(OpCodes.Callvirt, method_ListItem, null);  //stack  [value]
        //        il.Emit(OpCodes.Ldc_I4, cepIndex);
        //        il.Emit(OpCodes.Ceq);   //if (selectedFieldIndexList[nextSelectedFieldIndex] == i)      
        //        il.Emit(OpCodes.Brfalse, lbNext);   //stack 空  不等直接跳转到lbNext

        //        var readerMethod = GetReaderMethod(member.GetPropertyOrFieldType());

        //        il.Emit(OpCodes.Dup);

        //        il.Emit(OpCodes.Ldarg_S, 0);    //加载参数DataReader
        //        il.Emit(OpCodes.Ldloc_S, readIndex);
        //        il.EmitCall(OpCodes.Call, readerMethod, null);     //调用对应的 readerMethod 得到value

        //        SetValueIL(il, member);

        //        //只要能进入到if (selectedFieldIndexList[nextSelectedFieldIndex] == i)  就给++
        //        //readerIndex++
        //        il.Emit(OpCodes.Ldloc_S, readIndex);
        //        il.Emit(OpCodes.Ldc_I4_1);
        //        il.Emit(OpCodes.Add);
        //        il.Emit(OpCodes.Stloc_S, readIndex);

        //        //nextSelectedFieldIndex++
        //        il.Emit(OpCodes.Ldloc_S, nextSelectedFieldIndex);
        //        il.Emit(OpCodes.Ldc_I4_1);
        //        il.Emit(OpCodes.Add);
        //        il.Emit(OpCodes.Stloc_S, nextSelectedFieldIndex);

        //        il.Emit(OpCodes.Ldloc_S, nextSelectedFieldIndex);
        //        il.Emit(OpCodes.Ldloc_S, selectedFieldCount);
        //        il.Emit(OpCodes.Ceq);       //if (nextSelectedFieldIndex == selectedFieldCount)  
        //        il.Emit(OpCodes.Brtrue, lbNav);   //等于则直接跳转至导航属性映射位置 lbNav  

        //        il.MarkLabel(lbNext);

        //        cepIndex++;
        //    }
        //    #endregion  映射结束

        //    Label lbFinish = il.DefineLabel();//映射结束ret

        //    #region 导航属性映射

        //    //导航属性映射位置
        //    il.MarkLabel(lbNav);
        //    //----------------导航映射
        //    #region
        //    //if ((navMemberObjectCreateContext != null) && (navMemberObjectCreateContext.Count != 0))
        //    //{
        //    //    if (navMemberObjectCreateContext.TryGetValue(0x19e89e, out context))
        //    //    {
        //    //        cart.User = (User)Generator.InvokeGenerator((IDataReader)this, context);
        //    //    }
        //    //    if (navMemberObjectCreateContext.TryGetValue(0x333256a, out context))
        //    //    {
        //    //        cart.User1 = (User)Generator.InvokeGenerator((IDataReader)this, context);
        //    //    }
        //    //}
        //    #endregion

        //    if (entityDescriptor.NavMembers != null && entityDescriptor.NavMembers.Count > 0)
        //    {
        //        il.Emit(OpCodes.Ldloc_S, subSelectInfoDic);

        //        il.Emit(OpCodes.Brfalse, lbFinish);     //如果为null 直接return

        //        //判断Dictionary.Count是否大于0
        //        il.Emit(OpCodes.Ldloc_S, subSelectInfoDic);
        //        var dicCount_Getter = createContextDicType.GetProperty("Count").GetGetMethod();
        //        il.EmitCall(OpCodes.Callvirt, dicCount_Getter, null);
        //        il.Emit(OpCodes.Ldc_I4_0);
        //        il.Emit(OpCodes.Ceq);
        //        il.Emit(OpCodes.Brtrue, lbFinish);

        //        foreach (var item in entityDescriptor.NavMembers)
        //        {
        //            var member = item.Value.MemberInfo;
        //            var lbNextNav = il.DefineLabel();
        //            il.Emit(OpCodes.Ldloc_S, subSelectInfoDic);
        //            il.Emit(OpCodes.Ldc_I4, member.GetHashCode());
        //            il.Emit(OpCodes.Ldloca_S, subSelectInfo);
        //            var method_TryGetValue = createContextDicType.GetMethods().Where(a => a.Name == "TryGetValue").First();
        //            il.EmitCall(OpCodes.Callvirt, method_TryGetValue, null);
        //            il.Emit(OpCodes.Brfalse, lbNextNav);

        //            il.Emit(OpCodes.Dup);   //复制obj

        //            il.Emit(OpCodes.Ldarg_S, 0);    //加载参数DataReader
        //            il.Emit(OpCodes.Ldloc_S, subSelectInfo);    //加载subMappingInfo
        //            var method_InvokeGenerator = typeof(DelegateCreateManage).GetMethods().Where(a => a.Name == "InvokeGenerator").First();
        //            il.EmitCall(OpCodes.Call, method_InvokeGenerator, null);

        //            Type navType = null;

        //            MemberTypes navMemberType = member.MemberType;
        //            if (navMemberType == MemberTypes.Property)
        //            {
        //                navType = ((PropertyInfo)member).PropertyType;
        //            }
        //            else if (navMemberType == MemberTypes.Field)
        //            {
        //                navType = ((FieldInfo)member).FieldType;
        //            }
        //            else
        //            {
        //                throw new Exception("只支持公共属性和字段映射");
        //            }

        //            il.Emit(OpCodes.Castclass, navType);

        //            SetValueIL(il, member);

        //            //il.EmitWriteLine("Yes");
        //            il.MarkLabel(lbNextNav);
        //        }
        //    }

        //    #endregion

        //    //================
        //    //il.Emit(OpCodes.Br, lbFinish);//跳过异常段代码

        //    //il.EmitWriteLine("----------------");

        //    il.MarkLabel(lbFinish);

        //    //il.Emit(OpCodes.Nop);
        //    //il.EmitWriteLine("映射结束，obj创建成功");
        //    il.Emit(OpCodes.Ret);

        //    del = (Func<IDataReader, ObjectCreateContext, object>)dm.CreateDelegate(typeof(Func<IDataReader, ObjectCreateContext, object>));
        //    return del;
        //}

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


        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object, List<KeyValuePair<string, object>>>> getValuesDelegateCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object, List<KeyValuePair<string, object>>>>();

        private static Func<object, List<KeyValuePair<string, object>>> CreateGetValuesDelegate(Type t)
        {
            Type retType = typeof(List<KeyValuePair<string, object>>);
            Type kvType = typeof(KeyValuePair<string, object>);
            ConstructorInfo kvConstructorInfo = kvType.GetConstructor(new Type[] { UtilConstants.TypeOfString, UtilConstants.TypeOfObject });

            IEnumerable<MemberInfo> members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(a => a is PropertyInfo || a is FieldInfo);

            Func<object, List<KeyValuePair<string, object>>> del;
            DynamicMethod dm = new DynamicMethod("GetValues_" + Guid.NewGuid().ToString(), retType, new Type[] { UtilConstants.TypeOfObject }, true);
            ILGenerator il = dm.GetILGenerator();

            var obj = il.DeclareLocal(retType);   //生成对象变量
            var arg_0 = il.DeclareLocal(t);

            OpCode opLdc;
            OpCode opCall;

            ConstructorInfo ctor = retType.GetConstructor(new Type[] { UtilConstants.TypeOfInt32 });
            il.Emit(OpCodes.Ldc_I4, members.Count());
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc, obj);
            il.Emit(OpCodes.Ldloc, obj);   //加载 后面用Dup方式  这样就没必要定义一堆的变量记录GetValue得到的值了

            il.Emit(OpCodes.Ldarg_0);
            if (t.IsValueType)
            {
                opLdc = OpCodes.Ldloca_S;
                opCall = OpCodes.Call;
                il.Emit(OpCodes.Unbox_Any, t);
            }
            else
            {
                opLdc = OpCodes.Ldloc;
                opCall = OpCodes.Callvirt;
                il.Emit(OpCodes.Castclass, t);
            }
            il.Emit(OpCodes.Stloc, arg_0);

            var listAddMethod = retType.GetMethod("Add", new Type[] { kvType });

            foreach (var member in members)
            {
                PropertyInfo prop = null;
                FieldInfo field = null;
                if ((prop = member as PropertyInfo) != null)
                {
                    var getter = prop.GetGetMethod();
                    if (getter == null)
                        continue;       //对非公共的 getter 无视

                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldstr, prop.Name);

                    //访问属性，得到属性值
                    il.Emit(opLdc, arg_0);
                    il.EmitCall(opCall, getter, null);

                    if (prop.PropertyType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, prop.PropertyType);
                    }

                    // new KeyValuePair<string, object>(key,value);
                    il.Emit(OpCodes.Newobj, kvConstructorInfo);


                    il.EmitCall(OpCodes.Callvirt, listAddMethod, null);
                }
                else if ((field = member as FieldInfo) != null)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldstr, field.Name);

                    //访问字段，得到字段值
                    il.Emit(OpCodes.Ldloc, arg_0);
                    il.Emit(OpCodes.Ldfld, field);

                    if (field.FieldType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, field.FieldType);
                    }

                    // new KeyValuePair<string, object>(key,value);
                    il.Emit(OpCodes.Newobj, kvConstructorInfo);

                    il.EmitCall(OpCodes.Callvirt, listAddMethod, null);
                }
                else
                    continue;// 只获取公共属性和字段
            }

            il.Emit(OpCodes.Ret);

            del = (Func<object, List<KeyValuePair<string, object>>>)dm.CreateDelegate(typeof(Func<object, List<KeyValuePair<string, object>>>));
            return del;
        }

        public static Func<object, List<KeyValuePair<string, object>>> GetGetValuesDelegate(Type t)
        {
            Func<object, List<KeyValuePair<string, object>>> del;
            if (!getValuesDelegateCache.TryGetValue(t, out del))
            {
                del = CreateGetValuesDelegate(t);
                del = getValuesDelegateCache.GetOrAdd(t, del);
            }

            return del;
        }

        #endregion


        #region
        internal static MethodInfo GetReaderMethod(Type type)
        {

            MethodInfo result;
            bool isNullable = false;
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
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
                    isNullable = true;
                    result = Reader_GetString;
                    break;
                case TypeCode.Object:
                    isNullable = true;
                    result = Reader_GetValue;
                    break;
                default:
                    if (type == typeof(Guid))
                    {
                        result = isNullable ? Reader_GetGuid_Nullable : Reader_GetGuid;
                    }
                    else if (type == typeof(TimeSpan))
                    {
                        result = isNullable ? Reader_GetTimeSpan_Nullable : Reader_GetTimeSpan;
                    }
                    else if (type == typeof(DateTimeOffset))
                    {
                        result = isNullable ? Reader_GetDateTimeOffset_Nullable : Reader_GetDateTimeOffset;
                    }
                    else if (type == UtilConstants.TypeOfObject)
                    {
                        isNullable = true;
                        result = Reader_GetValue;
                    }
                    else if (type == typeof(Byte[]))
                    {
                        isNullable = true;
                        result = Reader_GetBytes;
                    }
                    else if (type == typeof(Char[]))
                    {
                        isNullable = true;
                        result = Reader_GetChars;
                    }
                    else
                    {
                        isNullable = true;
                        result = Reader_GetValue;
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
        internal static readonly MethodInfo Reader_GetTimeSpan = typeof(DataReaderExtensions).GetMethod("Reader_GetTimeSpan");
        internal static readonly MethodInfo Reader_GetTimeSpan_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetTimeSpan_Nullable");
        internal static readonly MethodInfo Reader_GetDateTimeOffset = typeof(DataReaderExtensions).GetMethod("Reader_GetDateTimeOffset");
        internal static readonly MethodInfo Reader_GetDateTimeOffset_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetDateTimeOffset_Nullable");
        internal static readonly MethodInfo Reader_GetString = typeof(DataReaderExtensions).GetMethod("Reader_GetString");
        internal static readonly MethodInfo Reader_GetValue = typeof(DataReaderExtensions).GetMethod("Reader_GetValue");
        internal static readonly MethodInfo Reader_GetBytes = typeof(DataReaderExtensions).GetMethod("Reader_GetBytes");
        internal static readonly MethodInfo Reader_GetChars = typeof(DataReaderExtensions).GetMethod("Reader_GetChars");

        internal static readonly MethodInfo Reader_GetEnum = typeof(DataReaderExtensions).GetMethod("Reader_GetEnum");
        internal static readonly MethodInfo Reader_GetEnum_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetEnum_Nullable");
        #endregion

        #endregion

    }
}
