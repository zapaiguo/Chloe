using Chloe.Core;
using Chloe.DbExpressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.SqlServer
{
    partial class SqlGenerator : DbExpressionVisitor<DbExpression>
    {
        static Dictionary<string, Action<DbFunctionExpression, SqlGenerator>> InitFuncHandlers()
        {
            var funcHandlers = new Dictionary<string, Action<DbFunctionExpression, SqlGenerator>>();
            funcHandlers.Add("Count", Func_Count);
            funcHandlers.Add("LongCount", Func_LongCount);
            funcHandlers.Add("Sum", Func_Sum);
            funcHandlers.Add("Max", Func_Max);
            funcHandlers.Add("Min", Func_Min);
            funcHandlers.Add("Average", Func_Average);

            var ret = new Dictionary<string, Action<DbFunctionExpression, SqlGenerator>>(funcHandlers.Count, StringComparer.Ordinal);
            foreach (var item in funcHandlers)
            {
                ret.Add(item.Key, item.Value);
            }

            return ret;
        }

        static void Func_Count(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_Count(generator);
        }
        static void Func_LongCount(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_LongCount(generator);
        }
        static void Func_Sum(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_Sum(generator, exp.Parameters.First(), exp.Method.ReturnType);
        }
        static void Func_Max(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_Max(generator, exp.Parameters.First(), exp.Method.ReturnType);
        }
        static void Func_Min(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_Min(generator, exp.Parameters.First(), exp.Method.ReturnType);
        }
        static void Func_Average(DbFunctionExpression exp, SqlGenerator generator)
        {
            Func_Average(generator, exp.Parameters.First(), exp.Method.ReturnType);
        }
    }
}
