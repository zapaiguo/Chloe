using Chloe.Data;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.MySql
{
    public class ChloeMySqlDataReader : DataReaderDecorator, IDataReader, IDataRecord, IDisposable
    {
        public ChloeMySqlDataReader(IDataReader reader) : base(reader)
        {
        }

        public override byte GetByte(int i)
        {
            object obj = this.PersistedReader.GetValue(i);
            if (obj is byte)
                return (byte)obj;

            return Convert.ToByte(obj);
        }

        public override char GetChar(int i)
        {
            object obj = this.PersistedReader.GetValue(i);
            if (obj is char)
                return (char)obj;

            return Convert.ToChar(obj);
        }

        public override DateTime GetDateTime(int i)
        {
            object obj = this.PersistedReader.GetValue(i);
            if (obj is DateTime)
                return (DateTime)obj;

            return Convert.ToDateTime(obj);
        }
        public override decimal GetDecimal(int i)
        {
            object obj = this.PersistedReader.GetValue(i);
            if (obj is decimal)
                return (decimal)obj;

            return Convert.ToDecimal(obj);
        }
        public override double GetDouble(int i)
        {
            object obj = this.PersistedReader.GetValue(i);
            if (obj is double)
                return (double)obj;

            return Convert.ToDouble(obj);
        }

        public override float GetFloat(int i)
        {
            object obj = this.PersistedReader.GetValue(i);
            if (obj is float)
                return (float)obj;

            return Convert.ToSingle(obj);
        }

        public override short GetInt16(int i)
        {
            object obj = this.PersistedReader.GetValue(i);
            if (obj is short)
                return (short)obj;

            return Convert.ToInt16(obj);
        }
        public override int GetInt32(int i)
        {
            object obj = this.PersistedReader.GetValue(i);
            if (obj is Int32)
                return (Int32)obj;

            return Convert.ToInt32(obj);
        }
        public override long GetInt64(int i)
        {
            object obj = this.GetValue(i);
            if (obj is Int64)
                return (Int64)obj;

            return Convert.ToInt64(obj);
        }
    }
}
