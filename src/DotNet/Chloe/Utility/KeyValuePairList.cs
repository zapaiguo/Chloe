using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe
{
    class KeyValuePairList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
    {
        public KeyValuePairList()
        {
        }
        public void Add(TKey key, TValue value)
        {
            this.Add(new KeyValuePair<TKey, TValue>(key, value));
        }
    }
}
