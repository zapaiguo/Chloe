using Chloe.Descriptors;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Core
{
    public interface IEntityState
    {
        object Entity { get; }
        TypeDescriptor TypeDescriptor { get; }
        bool IsChanged(MemberInfo member, object val);
        void Refresh();
    }

    class EntityState : IEntityState
    {
        Dictionary<MemberInfo, object> _fakes;
        object _entity;
        TypeDescriptor _typeDescriptor;

        public EntityState(TypeDescriptor typeDescriptor, object entity)
        {
            this._typeDescriptor = typeDescriptor;
            this._entity = entity;
            this.Refresh();
        }

        public object Entity { get { return this._entity; } }
        public TypeDescriptor TypeDescriptor { get { return this._typeDescriptor; } }

        public bool IsChanged(MemberInfo member, object val)
        {
            object oldVal;
            if (!this._fakes.TryGetValue(member, out oldVal))
            {
                return true;
            }

            return !Utils.IsEqual(oldVal, val);
        }
        public void Refresh()
        {
            Dictionary<MemberInfo, MappingMemberDescriptor> mappingMemberDescriptors = this.TypeDescriptor.MappingMemberDescriptors;

            if (this._fakes == null)
            {
                this._fakes = new Dictionary<MemberInfo, object>(mappingMemberDescriptors.Count);
            }
            else
            {
                this._fakes.Clear();
            }

            object entity = this._entity;
            foreach (var kv in mappingMemberDescriptors)
            {
                var key = kv.Key;
                var memberDescriptor = kv.Value;

                var val = memberDescriptor.GetValue(entity);

                this._fakes[key] = val;
            }
        }
    }
}
