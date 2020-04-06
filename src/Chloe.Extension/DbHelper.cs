using System.Data;

namespace Chloe.Extension
{
    static class DbHelper
    {
        public static DataTable FillDataTable(IDataReader reader)
        {
            DataTable dt = new DataTable();
            dt.Load(reader);
            return dt;
        }
        public static DataSet FillDataSet(IDataReader reader)
        {
            DataSet ds = new DataSet();
            var dt = FillDataTable(reader);
            ds.Tables.Add(dt);

            while (reader.NextResult())
            {
                dt = FillDataTable(reader);
                ds.Tables.Add(dt);
            }

            return ds;
        }
    }
}
