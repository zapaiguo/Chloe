using Chloe.Core;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using Chloe.Infrastructure.Interception;
using Chloe.InternalExtensions;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Data
{
    class ExternalAdoSession : AdoSession, IDisposable
    {
        IDbTransaction _dbTransaction;

        public ExternalAdoSession(IDbTransaction dbTransaction)
        {
            this._dbTransaction = dbTransaction;
        }

        public override IDbConnection DbConnection { get { return this._dbTransaction.Connection; } }
        public override IDbTransaction DbTransaction
        {
            get
            {
                return this._dbTransaction;
            }
            protected set
            {
                throw new NotSupportedException();
            }
        }

        public override bool IsInTransaction
        {
            get
            {
                return true;
            }
            protected set
            {
                throw new NotSupportedException();
            }
        }

        public override void Activate()
        {
        }
        public override void Complete()
        {
        }

        public override void BeginTransaction(IsolationLevel? il)
        {
            throw NotSupported();
        }
        public override void CommitTransaction()
        {
            throw NotSupported();
        }
        public override void RollbackTransaction()
        {
            throw NotSupported();
        }

        protected override void Dispose(bool disposing)
        {

        }

        NotSupportedException NotSupported()
        {
            return new NotSupportedException("使用外部事务情况下无法调用此方法。");
        }
    }
}
