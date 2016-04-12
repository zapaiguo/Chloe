using Chloe.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Internals
{
    internal class QueryFactor
    {
        public QueryFactor(IObjectActivtor objectActivtor, string cmdText, IDictionary<string, object> parameters)
        {
            this.ObjectActivtor = objectActivtor;
            this.CmdText = cmdText;
            this.Parameters = parameters;
        }
        public IObjectActivtor ObjectActivtor { get; set; }
        public string CmdText { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
    }

}
