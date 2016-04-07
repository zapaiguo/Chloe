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
        ResultElement _resultElement;
        Type _elementType;
        BaseExpressionVisitor _visitor = null;
        public RootQueryState(Type elementType)
        {
            this._elementType = elementType;
            this.Init();
        }

        void Init()
        {
            //TODO init _resultElement
            this._resultElement = null;
        }

        BaseExpressionVisitor Visitor
        {
            get
            {
                if (this._visitor == null)
                    _visitor = new GeneralExpressionVisitor(this._resultElement.MappingObjectExpression);

                return this._visitor;
            }
        }

        public override IQueryState AppendWhereExpression(WhereExpression whereExp)
        {
            BaseExpressionVisitor visitor = this.Visitor;
            var dbExp = visitor.Visit(whereExp.Expression);
            this._resultElement.UpdateWhereExpression(dbExp);
            return this;
        }
        public override IQueryState AppendOrderExpression(OrderExpression orderExp)
        {
            if (orderExp.NodeType == QueryExpressionType.OrderBy || orderExp.NodeType == QueryExpressionType.OrderByDesc)
                this._resultElement.OrderParts.Clear();

            BaseExpressionVisitor visitor = this.Visitor;
            var r = VisistOrderExpression(visitor, orderExp);
            this._resultElement.OrderParts.Add(r);

            return this;
        }

        /// <summary>
        /// 当前 querystate 作为一个源表，传递到下个 querystate
        /// </summary>
        public override ResultElement Result { get { return this.GetResultElement(); } }
        ResultElement GetResultElement()
        {
            return this._resultElement;

            //BaseExpressionVisitor visitor = new GeneralExpressionVisitor(this._rawEntity);
            //TablePart tablePart = this._rawEntity.RootTablePart;
            ////设置 table 的一些必要信息

            //ResultElement result = new ResultElement(tablePart);
            //MappingMembers mm = new MappingMembers(this._elementType.GetConstructor(new Type[0]));
            //result.MappingMembers = mm;

            ////在这解析所有表达式树，如 where、order、select、IncludeNavigationMember 等
            ////解析 where 表达式，得出的 DbExpression 

            //result.UpdateWhereExpression(VisistWhereExpressions(visitor, this.WhereExpressions));

            //this.VisistOrderExpressions(visitor, result.OrderParts);

            //this.SetMembers(tablePart, this._rawEntity.IncludedNavigationMembers, this._rawEntity.TypeDescriptor.MappingMemberDescriptors, result.MappingMembers);

            //return result;
        }

        //void SetMembers(TablePart tablePart, Dictionary<MemberInfo, IncludeMemberInfo> includedNavigationMembers, List<MappingMemberDescriptor> mappingMemberDescriptors, MappingMembers mappingResult)
        //{
        //    foreach (MappingMemberDescriptor mappingMemberDescriptor in mappingMemberDescriptors)
        //    {
        //        DbTableExpression tableExpression = tablePart.Table;
        //        DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(mappingMemberDescriptor.MemberType, tableExpression, mappingMemberDescriptor.ColumnName);
        //        mappingResult.SelectedMembers.Add(mappingMemberDescriptor.MemberInfo, columnAccessExpression);
        //    }

        //    foreach (var kv in includedNavigationMembers)
        //    {
        //        //MemberInfo key = kv.Key;
        //        IncludeMemberInfo includeMemberInfo = kv.Value;
        //        //key.MemberType.

        //        MappingTypeDescriptor navigationMemberTypeDescriptor = includeMemberInfo.MemberTypeDescriptor;

        //        MappingMembers subMappingResult = new MappingMembers(navigationMemberTypeDescriptor.EntityType.GetConstructor(new Type[0]));

        //        if (includeMemberInfo.IsIncludeMember)
        //        {
        //            //TODO 获取关联的键
        //            subMappingResult.AssociatingMemberInfo = includeMemberInfo.GetAssociatingMemberInfo();
        //        }

        //        this.SetMembers(includeMemberInfo.TablePart, includeMemberInfo.IncludeMembers, navigationMemberTypeDescriptor.MappingMemberDescriptors, subMappingResult);
        //        mappingResult.SubResultEntities.Add(kv.Key, subMappingResult);
        //    }
        //}

        public override IQueryState UpdateSelectResult(SelectExpression selectExpression)
        {
            ResultElement result = new ResultElement();
            result.TablePart = this._resultElement.TablePart;

            SelectExpressionVisitor1 visistor = new SelectExpressionVisitor1(this.Visitor, this._resultElement.MappingObjectExpression);

            IMappingObjectExpression r = visistor.Visit(selectExpression.Expression);
            result.MappingObjectExpression = r;
            result.OrderParts.AddRange(this._resultElement.OrderParts);
            result.UpdateWhereExpression(this._resultElement.WhereExpression);

            return new GeneralQueryState(result);


            //ResultElement result = new ResultElement(this._rawEntity.RootTablePart);

            ////解析 where order 表达式树
            ////解析 selectExpression
            ////构建一个新的 ResultElement
            //BaseExpressionVisitor visitor = new GeneralExpressionVisitor(this._rawEntity);
            //SelectExpressionVisitor selectExpressionVisitor = new RootSelectExpressionVisitor(visitor, this._rawEntity);
            //MappingMembers mappingMembers = selectExpressionVisitor.Visit(selectExpression.Expression);

            //result.MappingMembers = mappingMembers;

            //result.UpdateWhereExpression(VisistWhereExpressions(visitor, this.WhereExpressions));

            //this.VisistOrderExpressions(visitor, result.OrderParts);

            //return new GeneralQueryState(result);
        }

        public override MappingData GenerateMappingData()
        {
            MappingData data = new MappingData();

            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            TablePart tablePart = this._resultElement.TablePart;

            sqlQuery.Table = tablePart;
            sqlQuery.Orders.AddRange(this._resultElement.OrderParts);
            sqlQuery.UpdateWhereExpression(this._resultElement.WhereExpression);
            //tablePart.SetTableNameByNumber(0);

            var oac = this._resultElement.MappingObjectExpression.GenarateObjectActivtorCreator(sqlQuery);

            data.SqlQuery = sqlQuery;
            data.MappingEntity = oac;

            return data;


            //MappingData data = new MappingData();
            //ConstructorInfo constructorInfo = this._rawEntity.TypeDescriptor.EntityType.GetConstructor(new Type[0]);//获取默认构造函数
            //MappingEntity mappingMember = new MappingEntity(constructorInfo);

            ////------------
            //DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();

            //BaseExpressionVisitor visitor = new GeneralExpressionVisitor(this._rawEntity);
            //TablePart tablePart = this._rawEntity.RootTablePart;

            //sqlQuery.UpdateWhereExpression(VisistWhereExpressions(visitor, this.WhereExpressions));

            //this.VisistOrderExpressions(visitor, sqlQuery.Orders);

            //tablePart.SetTableNameByNumber(0);
            //this.FillColumnList(sqlQuery, tablePart, this._rawEntity.TypeDescriptor.MappingMemberDescriptors, this._rawEntity.IncludedNavigationMembers, mappingMember);
            //sqlQuery.Table = tablePart;
            ////============

            //data.SqlQuery = sqlQuery;
            //data.MappingEntity = mappingMember;

            //return data;
        }

        //void FillColumnList(DbSqlQueryExpression sqlQuery, TablePart tablePart, List<MappingMemberDescriptor> mappingMemberDescriptors, Dictionary<MemberInfo, IncludeMemberInfo> includedNavigationMembers, MappingEntity mappingMember)
        //{
        //    List<DbColumnExpression> columnList = sqlQuery.Columns;
        //    foreach (MappingMemberDescriptor mappingMemberDescriptor in mappingMemberDescriptors)
        //    {
        //        DbTableExpression tableExpression = tablePart.Table;
        //        DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(mappingMemberDescriptor.MemberType, tableExpression, mappingMemberDescriptor.ColumnName);
        //        string alias = sqlQuery.GenerateUniqueColumnAlias(mappingMemberDescriptor.ColumnName);
        //        DbColumnExpression columnExp = new DbColumnExpression(mappingMemberDescriptor.MemberType, columnAccessExpression, alias);

        //        columnList.Add(columnExp);

        //        if (mappingMember != null)
        //        {
        //            int ordinal = columnList.Count - 1;
        //            mappingMember.Members.Add(mappingMemberDescriptor.MemberInfo, ordinal);
        //        }
        //    }

        //    foreach (KeyValuePair<MemberInfo, IncludeMemberInfo> kv in includedNavigationMembers)
        //    {
        //        IncludeMemberInfo includeMemberInfo = kv.Value;

        //        MappingTypeDescriptor navigationMemberTypeDescriptor = includeMemberInfo.MemberTypeDescriptor;

        //        //MemberInfo associatingColumnMemberInfo = null;
        //        MappingEntity navMappingMember = null;
        //        if (mappingMember != null)
        //        {
        //            navMappingMember = new MappingEntity(includeMemberInfo.MemberDescriptor.MemberType.GetConstructor(new Type[0]));
        //            mappingMember.EntityMembers.Add(kv.Key, navMappingMember);

        //            //TODO 设置 AssociatingColumnOrdinal
        //            //if (includeMemberInfo.IsIncludeMember)
        //            //{
        //            //    //TODO 获取关联的键
        //            //    //navMappingMember.AssociatingColumnOrdinal = null; //在下面调用的 FillColumnList1 中设置
        //            //    //获取关联的 MemberInfo ，传递到下面调用的 FillColumnList1 中，以便设置 navMappingMember.AssociatingColumnOrdinal
        //            //    //associatingColumnMemberInfo = includeMemberInfo.GetAssociatingMemberInfo();
        //            //    navMappingMember.AssociatingMemberInfo = includeMemberInfo.GetAssociatingMemberInfo();
        //            //}
        //        }

        //        this.FillColumnList(sqlQuery, includeMemberInfo.TablePart, navigationMemberTypeDescriptor.MappingMemberDescriptors, includeMemberInfo.IncludeMembers, navMappingMember);
        //    }
        //}

    }
}
