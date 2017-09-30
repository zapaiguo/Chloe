using Chloe;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe
{
    public static class ChloeExtensions1
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">支持传 object 和 dynamic。</typeparam>
        /// <param name="dbContext"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static List<T> SqlQueryDynamic<T>(this IDbContext dbContext, string sql, params DbParam[] parameters)
        {
            if (typeof(T) != typeof(object))
            {
                return dbContext.SqlQuery<T>(sql, parameters).ToList();
            }

            DapperTable table = null;
            IDataReader reader = dbContext.Session.ExecuteReader(sql, parameters);
            int effectiveFieldCount = reader.FieldCount;
            int startBound = 0;
            List<object> rows = new List<object>();

            /* Copy from Dapper */
            using (reader)
            {
                while (reader.Read())
                {
                    if (table == null)
                    {
                        string[] names = new string[effectiveFieldCount];
                        for (int i = 0; i < effectiveFieldCount; i++)
                        {
                            names[i] = reader.GetName(i + startBound);
                        }
                        table = new DapperTable(names);
                    }

                    var values = new object[effectiveFieldCount];

                    //if (returnNullIfFirstMissing)
                    //{
                    //    values[0] = r.GetValue(startBound);
                    //    if (values[0] is DBNull)
                    //    {
                    //        return null;
                    //    }
                    //}

                    if (startBound == 0)
                    {
                        reader.GetValues(values);
                        for (int i = 0; i < values.Length; i++)
                            if (values[i] is DBNull) values[i] = null;
                    }
                    else
                    {
                        throw new NotSupportedException();
                        //var begin = returnNullIfFirstMissing ? 1 : 0;
                        //for (var iter = begin; iter < effectiveFieldCount; ++iter)
                        //{
                        //    object obj = r.GetValue(iter + startBound);
                        //    values[iter] = obj is DBNull ? null : obj;
                        //}
                    }
                    var row = new DapperRow(table, values);
                    rows.Add(row);
                }
            }

            return rows.Cast<T>().ToList();
        }
    }

    /* Copy from Dapper */
    sealed partial class DapperTable
    {
        string[] fieldNames;
        readonly Dictionary<string, int> fieldNameLookup;

        internal string[] FieldNames { get { return fieldNames; } }

        public DapperTable(string[] fieldNames)
        {
            if (fieldNames == null) throw new ArgumentNullException("fieldNames");
            this.fieldNames = fieldNames;

            fieldNameLookup = new Dictionary<string, int>(fieldNames.Length, StringComparer.Ordinal);
            // if there are dups, we want the **first** key to be the "winner" - so iterate backwards
            for (int i = fieldNames.Length - 1; i >= 0; i--)
            {
                string key = fieldNames[i];
                if (key != null) fieldNameLookup[key] = i;
            }
        }

        internal int IndexOfName(string name)
        {
            int result;
            return (name != null && fieldNameLookup.TryGetValue(name, out result)) ? result : -1;
        }
        internal int AddField(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (fieldNameLookup.ContainsKey(name)) throw new InvalidOperationException("Field already exists: " + name);
            int oldLen = fieldNames.Length;
            Array.Resize(ref fieldNames, oldLen + 1); // yes, this is sub-optimal, but this is not the expected common case
            fieldNames[oldLen] = name;
            fieldNameLookup[name] = oldLen;
            return oldLen;
        }


        internal bool FieldExists(string key)
        {
            return key != null && fieldNameLookup.ContainsKey(key);
        }

        public int FieldCount { get { return fieldNames.Length; } }
    }

    sealed partial class DapperRowMetaObject : System.Dynamic.DynamicMetaObject
    {
        static readonly MethodInfo getValueMethod = typeof(IDictionary<string, object>).GetProperty("Item").GetGetMethod();
        static readonly MethodInfo setValueMethod = typeof(DapperRow).GetMethod("SetValue", new Type[] { typeof(string), typeof(object) });

        public DapperRowMetaObject(
            System.Linq.Expressions.Expression expression,
            System.Dynamic.BindingRestrictions restrictions
            )
            : base(expression, restrictions)
        {
        }

        public DapperRowMetaObject(
            System.Linq.Expressions.Expression expression,
            System.Dynamic.BindingRestrictions restrictions,
            object value
            )
            : base(expression, restrictions, value)
        {
        }

        System.Dynamic.DynamicMetaObject CallMethod(
            MethodInfo method,
            System.Linq.Expressions.Expression[] parameters
            )
        {
            var callMethod = new System.Dynamic.DynamicMetaObject(
                System.Linq.Expressions.Expression.Call(
                    System.Linq.Expressions.Expression.Convert(Expression, LimitType),
                    method,
                    parameters),
                System.Dynamic.BindingRestrictions.GetTypeRestriction(Expression, LimitType)
                );
            return callMethod;
        }

        public override System.Dynamic.DynamicMetaObject BindGetMember(System.Dynamic.GetMemberBinder binder)
        {
            var parameters = new System.Linq.Expressions.Expression[]
                                 {
                                         System.Linq.Expressions.Expression.Constant(binder.Name)
                                 };

            var callMethod = CallMethod(getValueMethod, parameters);

            return callMethod;
        }

        // Needed for Visual basic dynamic support
        public override System.Dynamic.DynamicMetaObject BindInvokeMember(System.Dynamic.InvokeMemberBinder binder, System.Dynamic.DynamicMetaObject[] args)
        {
            var parameters = new System.Linq.Expressions.Expression[]
                                 {
                                         System.Linq.Expressions.Expression.Constant(binder.Name)
                                 };

            var callMethod = CallMethod(getValueMethod, parameters);

            return callMethod;
        }

        public override System.Dynamic.DynamicMetaObject BindSetMember(System.Dynamic.SetMemberBinder binder, System.Dynamic.DynamicMetaObject value)
        {
            var parameters = new System.Linq.Expressions.Expression[]
                                 {
                                         System.Linq.Expressions.Expression.Constant(binder.Name),
                                         value.Expression,
                                 };

            var callMethod = CallMethod(setValueMethod, parameters);

            return callMethod;
        }
    }

    sealed partial class DapperRow
      : System.Dynamic.IDynamicMetaObjectProvider
      , IDictionary<string, object>
    {
        readonly DapperTable table;
        object[] values;

        public DapperRow(DapperTable table, object[] values)
        {
            if (table == null) throw new ArgumentNullException("table");
            if (values == null) throw new ArgumentNullException("values");
            this.table = table;
            this.values = values;
        }
        private sealed class DeadValue
        {
            public static readonly DeadValue Default = new DeadValue();
            private DeadValue() { }
        }
        int ICollection<KeyValuePair<string, object>>.Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    if (!(values[i] is DeadValue)) count++;
                }
                return count;
            }
        }

        public bool TryGetValue(string name, out object value)
        {
            var index = table.IndexOfName(name);
            if (index < 0)
            { // doesn't exist
                value = null;
                return false;
            }
            // exists, **even if** we don't have a value; consider table rows heterogeneous
            value = index < values.Length ? values[index] : null;
            if (value is DeadValue)
            { // pretend it isn't here
                value = null;
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("{DapperRow");
            foreach (var kv in this)
            {
                var value = kv.Value;
                sb.Append(", ").Append(kv.Key);
                if (value != null)
                {
                    sb.Append(" = '").Append(kv.Value).Append('\'');
                }
                else
                {
                    sb.Append(" = NULL");
                }
            }

            return sb.Append('}').ToString();
        }

        System.Dynamic.DynamicMetaObject System.Dynamic.IDynamicMetaObjectProvider.GetMetaObject(
            System.Linq.Expressions.Expression parameter)
        {
            return new DapperRowMetaObject(parameter, System.Dynamic.BindingRestrictions.Empty, this);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            var names = table.FieldNames;
            for (var i = 0; i < names.Length; i++)
            {
                object value = i < values.Length ? values[i] : null;
                if (!(value is DeadValue))
                {
                    yield return new KeyValuePair<string, object>(names[i], value);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Implementation of ICollection<KeyValuePair<string,object>>

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            IDictionary<string, object> dic = this;
            dic.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        { // removes values for **this row**, but doesn't change the fundamental table
            for (int i = 0; i < values.Length; i++)
                values[i] = DeadValue.Default;
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            object value;
            return TryGetValue(item.Key, out value) && Equals(value, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var kv in this)
            {
                array[arrayIndex++] = kv; // if they didn't leave enough space; not our fault
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            IDictionary<string, object> dic = this;
            return dic.Remove(item.Key);
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Implementation of IDictionary<string,object>

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            int index = table.IndexOfName(key);
            if (index < 0 || index >= values.Length || values[index] is DeadValue) return false;
            return true;
        }

        void IDictionary<string, object>.Add(string key, object value)
        {
            SetValue(key, value, true);
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            int index = table.IndexOfName(key);
            if (index < 0 || index >= values.Length || values[index] is DeadValue) return false;
            values[index] = DeadValue.Default;
            return true;
        }

        object IDictionary<string, object>.this[string key]
        {
            get { object val; TryGetValue(key, out val); return val; }
            set { SetValue(key, value, false); }
        }

        public object SetValue(string key, object value)
        {
            return SetValue(key, value, false);
        }
        private object SetValue(string key, object value, bool isAdd)
        {
            if (key == null) throw new ArgumentNullException("key");
            int index = table.IndexOfName(key);
            if (index < 0)
            {
                index = table.AddField(key);
            }
            else if (isAdd && index < values.Length && !(values[index] is DeadValue))
            {
                // then semantically, this value already exists
                throw new ArgumentException("An item with the same key has already been added", "key");
            }
            int oldLength = values.Length;
            if (oldLength <= index)
            {
                // we'll assume they're doing lots of things, and
                // grow it to the full width of the table
                Array.Resize(ref values, table.FieldCount);
                for (int i = oldLength; i < values.Length; i++)
                {
                    values[i] = DeadValue.Default;
                }
            }
            return values[index] = value;
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return this.Select(kv => kv.Key).ToArray(); }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return this.Select(kv => kv.Value).ToArray(); }
        }

        #endregion
    }

}
