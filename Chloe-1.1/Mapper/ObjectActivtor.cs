using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Mapper
{
    public class ObjectActivtor : IObjectActivtor
    {
        Func<object> _instanceCreator;
        List<IValueSetter> _memberSetters;
        public ObjectActivtor(Func<object> instanceCreator, List<IValueSetter> memberSetters)
        {
            this._instanceCreator = instanceCreator;
            this._memberSetters = memberSetters;
        }

        public object CreateInstance(IDataReader reader)
        {
            object obj = this._instanceCreator();//根据类型对象创建实例

            foreach (IValueSetter memberSetter in this._memberSetters)
            {
                memberSetter.SetValue(obj, reader);
            }

            return obj;
        }
    }
}
