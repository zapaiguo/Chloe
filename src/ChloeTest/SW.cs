using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    public class SW
    {
        public static void Do(Action act)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();

            act();

            sw.Stop();

            Console.WriteLine("用时：{0}", sw.ElapsedMilliseconds);
        }

        public static T Do<T>(Func<T> fn)
        {
            T t;
            Stopwatch sw = new Stopwatch();

            sw.Start();

            t = fn();

            sw.Stop();

            Console.WriteLine("用时：{0}", sw.ElapsedMilliseconds);

            return t;
        }
    }
}
