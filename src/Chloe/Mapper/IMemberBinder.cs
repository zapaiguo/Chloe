using System.Data;

namespace Chloe.Mapper
{
    public interface IMemberBinder
    {
        void Prepare(IDataReader reader);
        void Bind(object obj, IDataReader reader);
    }
}
