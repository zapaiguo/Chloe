using Chloe.Core;
using Chloe.Mapper;
using Chloe.Mapper.Activators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Query.Mapping
{
    public class PrimitiveObjectActivatorCreator : IObjectActivatorCreator
    {
        public PrimitiveObjectActivatorCreator(Type type, int readerOrdinal)
        {
            this.ObjectType = type;
            this.ReaderOrdinal = readerOrdinal;
        }

        public Type ObjectType { get; private set; }
        public bool IsRoot { get; set; }
        public int ReaderOrdinal { get; private set; }
        public int? CheckNullOrdinal { get; set; }

        public IObjectActivator CreateObjectActivator()
        {
            return this.CreateObjectActivator(null);
        }
        public IObjectActivator CreateObjectActivator(IDbContext dbContext)
        {
            PrimitiveObjectActivator activator = new PrimitiveObjectActivator(this.ObjectType, this.ReaderOrdinal);
            return activator;
        }

        public IFitter CreateFitter(IDbContext dbContext)
        {
            throw new NotSupportedException();
        }
    }
}
