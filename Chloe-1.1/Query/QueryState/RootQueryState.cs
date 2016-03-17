using Chloe.Query.DbExpressions;
using Chloe.Query.Implementation;
using Chloe.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chloe.Extensions;
using Chloe.Query.Mapping;

namespace Chloe.Query.QueryState
{
    internal sealed class RootQueryState : BaseQueryState
    {
        Type _elementType;
        RootEntity _rawEntity;
        public RootQueryState(Type elementType)
        {
            this._elementType = elementType;
            this.Init();
        }

        void Init()
        {
            RootEntity rawEntity = new RootEntity(this._elementType);
            this._rawEntity = rawEntity;
        }


        /// <summary>
        /// 当前 querystate 作为一个源表，传递到下个 querystate
        /// </summary>
        public override ResultElement Result { get { return this.GetResultElement(); } }
        ResultElement GetResultElement()
        {
            BaseExpressionVisitor visitor = new GeneralExpressionVisitor(this._rawEntity);
            TablePart tablePart = this._rawEntity.RootTablePart;
            //设置 table 的一些必要信息

            ResultElement result = new ResultElement(this._elementType, tablePart);

            //在这解析所有表达式树，如 where、order、select、IncludeNavigationMember 等
            //解析 where 表达式，得出的 DbExpression 

            result.UpdateWhereExpression(VisistWhereExpressions(visitor, this.WhereExpressions));

            this.VisistOrderExpressions(visitor, result.OrderParts);

            this.SetMembers(tablePart, this._rawEntity.IncludedNavigationMembers, this._rawEntity.TypeDescriptor.MappingMemberDescriptors, result.MappingMembers);

            return result;
        }

        void SetMembers(TablePart tablePart, Dictionary<MemberInfo, IncludeMemberInfo> includedNavigationMembers, List<MappingMemberDescriptor> mappingMemberDescriptors, MappingMembers mappingResult)
        {
            foreach (MappingMemberDescriptor mappingMemberDescriptor in mappingMemberDescriptors)
            {
                DbTableExpression tableExpression = tablePart.Table;
                DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(mappingMemberDescriptor.MemberType, tableExpression, mappingMemberDescriptor.ColumnName);
                mappingResult.SelectedMembers.Add(mappingMemberDescriptor.MemberInfo, columnAccessExpression);
            }

            foreach (var kv in includedNavigationMembers)
            {
                //MemberInfo key = kv.Key;
                IncludeMemberInfo includeMemberInfo = kv.Value;
                //key.MemberType.

                MappingTypeDescriptor navigationMemberTypeDescriptor = includeMemberInfo.MemberTypeDescriptor;

                MappingMembers subMappingResult = new MappingMembers(navigationMemberTypeDescriptor.EntityType);

                if (includeMemberInfo.IsIncludeMember)
                {
                    //TODO 获取关联的键
                    subMappingResult.AssociatingMemberInfo = includeMemberInfo.GetAssociatingMemberInfo();
                }

                this.SetMembers(includeMemberInfo.TablePart, includeMemberInfo.IncludeMembers, navigationMemberTypeDescriptor.MappingMemberDescriptors, subMappingResult);
                mappingResult.SubResultEntities.Add(kv.Key, subMappingResult);
            }
        }

        public override IQueryState UpdateSelectResult(SelectExpression selectExpression)
        {
            ResultElement result = new ResultElement(selectExpression.ElementType, this._rawEntity.RootTablePart);

            //解析 where order 表达式树
            //解析 selectExpression
            //构建一个新的 ResultElement
            BaseExpressionVisitor visitor = new GeneralExpressionVisitor(this._rawEntity);
            SelectExpressionVisitor selectExpressionVisitor = new RootSelectExpressionVisitor(visitor, this._rawEntity);
            MappingMembers mappingMembers = selectExpressionVisitor.Visit(selectExpression.Expression);

            result.MappingMembers = mappingMembers;

            result.UpdateWhereExpression(VisistWhereExpressions(visitor, this.WhereExpressions));

            this.VisistOrderExpressions(visitor, result.OrderParts);

            return new GeneralQueryState(result);
        }

        public override void IncludeNavigationMember(Expression exp)
        {
            LambdaExpression lambdaExpression = (LambdaExpression)exp;
            Expression body = lambdaExpression.Body;

            if (body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException(string.Format("Include path", exp.ToString()));

            this._rawEntity.IncludeNavigationMember((MemberExpression)body);
        }

        public override MappingData GenerateMappingData()
        {
            MappingData data = new MappingData(this._rawEntity.TypeDescriptor.EntityType);
            MappingMember mappingMember = new MappingMember(data.EntityType);

            //------------
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();

            BaseExpressionVisitor visitor = new GeneralExpressionVisitor(this._rawEntity);
            TablePart tablePart = this._rawEntity.RootTablePart;

            sqlQuery.UpdateWhereExpression(VisistWhereExpressions(visitor, this.WhereExpressions));

            this.VisistOrderExpressions(visitor, sqlQuery.Orders);

            tablePart.SetTableNameByNumber(0);
            this.FillColumnList(sqlQuery.Columns, tablePart, this._rawEntity.TypeDescriptor.MappingMemberDescriptors, this._rawEntity.IncludedNavigationMembers, mappingMember);
            sqlQuery.Table = tablePart;
            //============

            data.SqlQuery = sqlQuery;
            data.MappingInfo = mappingMember;

            return data;
        }

        void FillColumnList(List<DbColumnExpression> columnList, TablePart tablePart, List<MappingMemberDescriptor> mappingMemberDescriptors, Dictionary<MemberInfo, IncludeMemberInfo> includedNavigationMembers, MappingMember mappingMember)
        {
            foreach (MappingMemberDescriptor mappingMemberDescriptor in mappingMemberDescriptors)
            {
                DbTableExpression tableExpression = tablePart.Table;
                DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(mappingMemberDescriptor.MemberType, tableExpression, mappingMemberDescriptor.ColumnName);
                DbColumnExpression columnExp = new DbColumnExpression(mappingMemberDescriptor.MemberType, columnAccessExpression, mappingMemberDescriptor.ColumnName + "_Alias");

                columnList.Add(columnExp);

                if (mappingMember != null)
                {
                    int ordinal = columnList.Count - 1;
                    mappingMember.MappingMembers.Add(mappingMemberDescriptor.MemberInfo, ordinal);
                }
            }

            foreach (KeyValuePair<MemberInfo, IncludeMemberInfo> kv in includedNavigationMembers)
            {
                IncludeMemberInfo includeMemberInfo = kv.Value;

                MappingTypeDescriptor navigationMemberTypeDescriptor = includeMemberInfo.MemberTypeDescriptor;

                MappingMembers subMappingResult = new MappingMembers(navigationMemberTypeDescriptor.EntityType);

                //MemberInfo associatingColumnMemberInfo = null;
                MappingNavMember navMappingMember = null;
                if (mappingMember != null)
                {
                    navMappingMember = new MappingNavMember(includeMemberInfo.MemberDescriptor.MemberType);
                    mappingMember.MappingNavMembers.Add(kv.Key, navMappingMember);

                    //TODO 设置 AssociatingColumnOrdinal
                    if (includeMemberInfo.IsIncludeMember)
                    {
                        //TODO 获取关联的键
                        //navMappingMember.AssociatingColumnOrdinal = null; //在下面调用的 FillColumnList1 中设置
                        //获取关联的 MemberInfo ，传递到下面调用的 FillColumnList1 中，以便设置 navMappingMember.AssociatingColumnOrdinal
                        //associatingColumnMemberInfo = includeMemberInfo.GetAssociatingMemberInfo();
                        navMappingMember.AssociatingMemberInfo = includeMemberInfo.GetAssociatingMemberInfo();
                    }
                }

                this.FillColumnList(columnList, includeMemberInfo.TablePart, navigationMemberTypeDescriptor.MappingMemberDescriptors, includeMemberInfo.IncludeMembers, navMappingMember);
            }
        }

    }
}
