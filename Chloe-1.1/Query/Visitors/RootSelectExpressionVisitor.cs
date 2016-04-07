using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chloe.Core;
using Chloe.Extensions;
using Chloe.Utility;
using Chloe.Query.DbExpressions;
using Chloe.Query.Implementation;

namespace Chloe.Query
{
    //internal class RootSelectExpressionVisitor : SelectExpressionVisitor
    //{
    //    RootEntity _rawEntity = null;

    //    public RootSelectExpressionVisitor(BaseExpressionVisitor visitor, RootEntity rawEntity)
    //        : base(visitor)
    //    {
    //        this._rawEntity = rawEntity;
    //    }

    //    void FillSelectedMemberList(Dictionary<MemberInfo, DbExpression> selectedMembers, TablePart tablePart, List<MappingMemberDescriptor> mappingMemberDescriptors, Dictionary<MemberInfo, IncludeMemberInfo> includedNavigationMembers)
    //    {
    //        foreach (MappingMemberDescriptor mappingMemberDescriptor in mappingMemberDescriptors)
    //        {
    //            DbTableExpression tableExpression = tablePart.Table;
    //            DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(mappingMemberDescriptor.MemberType, tableExpression, mappingMemberDescriptor.ColumnName);

    //            selectedMembers.Add(mappingMemberDescriptor.MemberInfo, columnAccessExpression);
    //        }

    //        foreach (KeyValuePair<MemberInfo, IncludeMemberInfo> kv in includedNavigationMembers)
    //        {
    //            IncludeMemberInfo includeMemberInfo = kv.Value;
    //            MappingTypeDescriptor navigationMemberTypeDescriptor = includeMemberInfo.MemberTypeDescriptor;
    //            MappingMembers subMappingResult = new MappingMembers(navigationMemberTypeDescriptor.EntityType.GetConstructor(new Type[0]));
    //            if (includeMemberInfo.IsIncludeMember)
    //            {
    //                //subMappingResult.IsIncludeMember = includeMemberInfo.IsIncludeMember;
    //                subMappingResult.AssociatingMemberInfo = includeMemberInfo.GetAssociatingMemberInfo();
    //            }

    //            this.FillSelectedMemberList(subMappingResult.SelectedMembers, includeMemberInfo.TablePart, navigationMemberTypeDescriptor.MappingMemberDescriptors, includeMemberInfo.IncludeMembers);
    //        }
    //    }

    //    protected override MappingMembers VisitNavigationMember(MemberExpression exp)
    //    {
    //        IncludeMemberInfo includeMemberInfo = this._rawEntity.IncludeNavigationMember(exp);

    //        MappingMembers subMappingResult = new MappingMembers(includeMemberInfo.MemberTypeDescriptor.EntityType.GetConstructor(new Type[0]));
    //        if (includeMemberInfo.IsIncludeMember)
    //        {
    //            //subMappingResult.IsIncludeMember = includeMemberInfo.IsIncludeMember;
    //            subMappingResult.AssociatingMemberInfo = includeMemberInfo.GetAssociatingMemberInfo();
    //        }

    //        this.FillSelectedMemberList(subMappingResult.SelectedMembers, includeMemberInfo.TablePart, includeMemberInfo.MemberTypeDescriptor.MappingMemberDescriptors, includeMemberInfo.IncludeMembers);

    //        return subMappingResult;
    //    }
    //}
}
