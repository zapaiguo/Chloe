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
    //internal class GeneralSelectExpressionVisitor : SelectExpressionVisitor
    //{
    //    ResultElement _rawEntity = null;

    //    public GeneralSelectExpressionVisitor(BaseExpressionVisitor visitor, ResultElement rawEntity)
    //        : base(visitor)
    //    {
    //        this._rawEntity = rawEntity;
    //    }

    //    protected override MappingMembers VisitNavigationMember(MemberExpression exp)
    //    {
    //        return null;

    //        //Stack<MemberExpression> memberExpressions = exp.Reverse();
    //        //MappingMembers mappingMembers = this._rawEntity.MappingMembers;

    //        //foreach (MemberExpression memberExpression in memberExpressions)
    //        //{
    //        //    MappingMembers t;
    //        //    if (!mappingMembers.SubResultEntities.TryGetValue(memberExpression.Member, out t))
    //        //    {
    //        //        throw new Exception(exp.ToString());
    //        //    }

    //        //    mappingMembers = t;
    //        //}

    //        //return mappingMembers;
    //    }
    //}
}
