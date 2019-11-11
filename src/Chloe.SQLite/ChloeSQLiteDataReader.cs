using Chloe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.SQLite
{
    public class ChloeSQLiteDataReader : DataReaderDecorator, IDataReader, IDataRecord, IDisposable
    {
        ChloeSQLiteCommand _cmd;
        bool _hasReleaseLock = false;
        public ChloeSQLiteDataReader(IDataReader reader, ChloeSQLiteCommand cmd) : base(reader)
        {
            this._cmd = cmd;
        }

        ~ChloeSQLiteDataReader()
        {
            this.Dispose();
        }

        void ReleaseLock()
        {
            if (this._hasReleaseLock == false)
            {
                this._cmd.Conn.RWLock.EndRead();
                this._hasReleaseLock = true;
            }
        }

        public override void Close()
        {
            this.PersistedReader.Close();
            this.ReleaseLock();
        }

        public override void Dispose()
        {
            try
            {
                if (this.PersistedReader != null)
                    this.PersistedReader.Dispose();
            }
            finally
            {
                this.ReleaseLock();
            }

            GC.SuppressFinalize(this);
        }
    }
}
