using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public interface Q<T>
    {
        Q<T, T1> InnerJoin<T1>(Expression<Func<T, T1, bool>> on);
        Q<T, T1> LeftJoin<T1>(Expression<Func<T, T1, bool>> on);
        Q<T, T1> RightJoin<T1>(Expression<Func<T, T1, bool>> on);
        TResult ToQuery<TResult>(Expression<Func<T, TResult>> func);
    }
    public interface Q<T, T1>
    {
        Q<T, T1, T2> InnerJoin<T2>(Expression<Func<T, T1, T2, bool>> on);
        Q<T, T1, T2> LeftJoin<T2>(Expression<Func<T, T1, T2, bool>> on);
        Q<T, T1, T2> RightJoin<T2>(Expression<Func<T, T1, T2, bool>> on);
        TResult ToQuery<TResult>(Expression<Func<T, T1, TResult>> func);
    }
    public interface Q<T, T1, T2>
    {
        Q<T, T1, T2, T3> InnerJoin<T3>(Expression<Func<T, T1, T2, T3, bool>> on);
        Q<T, T1, T2, T3> LeftJoin<T3>(Expression<Func<T, T1, T2, T3, bool>> on);
        Q<T, T1, T2, T3> RightJoin<T3>(Expression<Func<T, T1, T2, T3, bool>> on);
        TResult ToQuery<TResult>(Expression<Func<T, T1, T2, TResult>> func);
    }
    public interface Q<T, T1, T2, T3>
    {
        TResult ToQuery<TResult>(Expression<Func<T, T1, T2, TResult>> func);
    }


    public class aa
    {
        public int TT;
        public void A()
        {
            Q<A> q = null;
            Q<A, A1> q1 = q.InnerJoin<A1>((a, a1) => a.Id == a1.Id);
            Q<A, A1, A2> q2 = q1.LeftJoin<A2>((a, a1, a2) => a1.Id == a2.Id);
            var o = q2.ToQuery((a, a1, a2) => new { Id = a.Id, Id1 = a1.Id });
        }
    }

    public class A
    {
        public int Id { get; set; }
    }
    public class A1
    {
        public int Id { get; set; }
    }
    public class A2
    {
        public int Id { get; set; }
    }
    public class A3
    {
        public int Id { get; set; }
    }
    public class A4
    {
        public int Id { get; set; }
    }
}
