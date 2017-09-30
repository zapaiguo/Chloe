using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    public class Sql
    {


        public Sql From(string fromPart)
        {
            return null;
        }

        public Sql Where(string @where)
        {
            return null;
        }

        public Sql OrderBy(string ordering)
        {
            return null;
        }

        public Sql GroupBy(string group)
        {
            return null;
        }

        public Sql Having(string having)
        {
            return null;
        }

        public Sql Select(string columns)
        {
            return null;
        }

        public Sql Skip(int count)
        {
            return null;
        }
        public Sql Take(int count)
        {
            return null;
        }
        public Sql TakePage(int pageNumber, int pageSize)
        {
            int skipCount = (pageNumber - 1) * pageSize;
            int takeCount = pageSize;
            return this.Skip(skipCount).Take(takeCount);
        }

        public string ToSql()
        {
            return null;
        }
        public string Count()
        {
            return null;
        }
        public string LongCount()
        {
            return null;
        }
        public string AsTable(string alias)
        {
            return null;
        }
    }
}
