using Chloe.Data;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Oracle
{
    public class ChloeOracleDataReader : DataReaderDecorator, IDataReader, IDataRecord, IDisposable
    {
        public ChloeOracleDataReader(IDataReader reader) : base(reader)
        {
        }

        public override bool GetBoolean(int i)
        {
            Type fieldType = this.PersistedReader.GetFieldType(i);

            if (fieldType == UtilConstants.TypeOfBoolean)
                return this.PersistedReader.GetBoolean(i);

            return Convert.ToBoolean(this.PersistedReader.GetValue(i));
        }
    }
}
