using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Entity
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        public TableAttribute() { }
        public TableAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
