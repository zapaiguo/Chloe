using Chloe.Descriptors;
using Chloe.Exceptions;
using Chloe.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe
{
    public static class DbContextExtension
    {
        public static IQuery<T> Query<T>(this IDbContext dbContext, Expression<Func<T, bool>> predicate) where T : new()
        {
            return dbContext.Query<T>().Where(predicate);
        }

        public static void BeginTransaction(this IDbContext dbContext, IsolationLevel il = IsolationLevel.ReadCommitted)
        {
            dbContext.Session.BeginTransaction(il);
        }
        public static void CommitTransaction(this IDbContext dbContext)
        {
            dbContext.Session.CommitTransaction();
        }
        public static void RollbackTransaction(this IDbContext dbContext)
        {
            dbContext.Session.RollbackTransaction();
        }
        public static void DoWithTransaction(this IDbContext dbContext, Action action, IsolationLevel il = IsolationLevel.ReadCommitted)
        {
            dbContext.Session.BeginTransaction(il);
            try
            {
                action();
                dbContext.Session.CommitTransaction();
            }
            catch
            {
                if (dbContext.Session.IsInTransaction)
                    dbContext.Session.RollbackTransaction();
                throw;
            }
        }
        public static T DoWithTransaction<T>(this IDbContext dbContext, Func<T> action, IsolationLevel il = IsolationLevel.ReadCommitted)
        {
            dbContext.Session.BeginTransaction(il);
            try
            {
                T ret = action();
                dbContext.Session.CommitTransaction();
                return ret;
            }
            catch
            {
                if (dbContext.Session.IsInTransaction)
                    dbContext.Session.RollbackTransaction();
                throw;
            }
        }

        public static DbActionBag CreateActionBag(this IDbContext dbContext)
        {
            DbActionBag bag = new DbActionBag(dbContext);
            return bag;
        }
    }
}
