using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Chloe.InternalExtensions;

namespace Chloe.Mapper.Binders
{
    public class PrimitiveMemberBinder : IMemberBinder
    {
        MemberInfo _member;
        MRMTuple _mrmTuple;
        int _ordinal;
        IMRM _mMapper;

        public PrimitiveMemberBinder(MemberInfo member, MRMTuple mrmTuple, int ordinal)
        {
            this._member = member;
            this._mrmTuple = mrmTuple;
            this._ordinal = ordinal;
        }

        public int Ordinal { get { return this._ordinal; } }

        public void Prepare(IDataReader reader)
        {
            Type fieldType = reader.GetFieldType(this._ordinal);
            if (fieldType == this._member.GetMemberType().GetUnderlyingType())
            {
                this._mMapper = _mrmTuple.StrongMRM.Value;
                return;
            }

            this._mMapper = _mrmTuple.SafeMRM.Value;
        }
        public void Bind(object obj, IDataReader reader)
        {
            this._mMapper.Map(obj, reader, this._ordinal);
        }
    }
}
