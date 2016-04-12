using Chloe.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Core.Database
{
    class DbCommandFactor
    {
        public DbCommandFactor(IObjectActivtor objectActivtor, string commandText, IDictionary<string, object> parameters)
        {
            this.ObjectActivtor = objectActivtor;
            this.CommandText = commandText;
            this.Parameters = parameters;
        }
        public IObjectActivtor ObjectActivtor { get; set; }
        public string CommandText { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
    }
}
