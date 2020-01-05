using System.Data;

namespace Chloe.Mapper
{
    public interface IObjectActivator
    {
        void Prepare(IDataReader reader);
        object CreateInstance(IDataReader reader);
    }
}
