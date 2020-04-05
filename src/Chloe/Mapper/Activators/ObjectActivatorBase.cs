using System.Data;
using System.Threading.Tasks;

namespace Chloe.Mapper.Activators
{
    public abstract class ObjectActivatorBase : IObjectActivator
    {
        public virtual void Prepare(IDataReader reader)
        {

        }
        public abstract object CreateInstance(IDataReader reader);
        public virtual async Task<object> CreateInstanceAsync(IDataReader reader)
        {
            return this.CreateInstance(reader);
        }
    }
}
