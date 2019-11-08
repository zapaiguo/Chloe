using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
