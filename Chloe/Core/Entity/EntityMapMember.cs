using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Core
{
    internal class EntityMapMember
    {
        /// <summary>
        /// 成员的 MemberInfo 对象
        /// </summary>
        public MemberInfo MemberInfo { get; set; }
        /// <summary>
        /// 真实的列名
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// reader's ordinal
        /// </summary>
        public int OrderedIndex { get; set; }
    }
}
