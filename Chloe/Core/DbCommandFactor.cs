using Chloe.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Core
{
    class DbCommandFactor
    {
        public DbCommandFactor(IObjectActivator objectActivator, string commandText, IDictionary<string, object> parameters)
        {
            this.ObjectActivator = objectActivator;
            this.CommandText = commandText;
            this.Parameters = parameters;
        }
        public IObjectActivator ObjectActivator { get; set; }
        public string CommandText { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
    }
}
