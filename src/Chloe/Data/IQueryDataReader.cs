using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.Data
{
    public interface IQueryDataReader : IDataReader
    {
        bool AllowReadNextRecord { get; set; }
    }

    public class QueryDataReader : DataReaderDecorator, IQueryDataReader
    {
        public QueryDataReader(IDataReader reader) : base(reader)
        {
        }

        public bool AllowReadNextRecord { get; set; } = true;

        public override bool Read()
        {
            if (!this.AllowReadNextRecord)
                return true;

            var ret = base.Read();
            this.AllowReadNextRecord = true;
            return ret;
        }
    }

}
