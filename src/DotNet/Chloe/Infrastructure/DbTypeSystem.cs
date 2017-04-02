using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Infrastructure
{
    public static class DbTypeSystem
    {
        static Dictionary<Type, Type> FixedMapTypes = new Dictionary<Type, Type>();

        static DbTypeSystem()
        {
            var mapTypes = new List<Type>();

            mapTypes.Add(typeof(string));
            mapTypes.Add(typeof(int));
            mapTypes.Add(typeof(long));
            mapTypes.Add(typeof(decimal));
            mapTypes.Add(typeof(double));
            mapTypes.Add(typeof(float));
            mapTypes.Add(typeof(bool));
            mapTypes.Add(typeof(DateTime));
            mapTypes.Add(typeof(short));
            mapTypes.Add(typeof(Guid));
            mapTypes.Add(typeof(byte));
            mapTypes.Add(typeof(char));

            mapTypes.Add(typeof(ulong));
            mapTypes.Add(typeof(uint));
            mapTypes.Add(typeof(ushort));
            mapTypes.Add(typeof(sbyte));

            mapTypes.Add(typeof(Object));

            mapTypes.Add(typeof(byte[]));

            Dictionary<Type, Type> fixedMapTypes = new Dictionary<Type, Type>();
        }

    }
}
