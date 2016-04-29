using Chloe.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Mapping
{
    public interface IObjectActivtorCreator
    {
        IObjectActivtor CreateObjectActivtor();
    }
 }
