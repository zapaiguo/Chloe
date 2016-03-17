using Chloe.Query.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chloe.Extensions;

namespace Chloe.Query
{
    public class RootEntity : IRawEntity
    {
        Type _elementType;
        MappingTypeDescriptor _typeDescriptor;

        TablePart _tablePart;
        Dictionary<MemberInfo, IncludeMemberInfo> _relatedNavigationMembers = new Dictionary<MemberInfo, IncludeMemberInfo>();
        Dictionary<MemberInfo, IncludeMemberInfo> _includedNavigationMembers = new Dictionary<MemberInfo, IncludeMemberInfo>();

        public Dictionary<MemberInfo, IncludeMemberInfo> RelatedNavigationMembers { get { return this._relatedNavigationMembers; } }
        public Dictionary<MemberInfo, IncludeMemberInfo> IncludedNavigationMembers { get { return this._includedNavigationMembers; } }
        public RootEntity(Type elementType)
        {
            this._elementType = elementType;
            this._typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(this._elementType);
            this._tablePart = this.CreateRootTable(this._typeDescriptor.TableName);
        }

        /// <summary>
        /// 将一个派生至 parameter 的 MemberExpression 转换成  DbExpression。 case ：1. a.Name --> T.Name; 2. a.User.Name --> User.Name; 3.  a.Name.Length --> DbExpression.MemberAccess(LengthmMmber, T.Name);
        /// </summary>
        /// <param name="memberExpressionDeriveParameter"></param>
        /// <returns></returns>
        public DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter)
        {
            Stack<MemberExpression> memberExpressions = memberExpressionDeriveParameter.Reverse();
            MappingTypeDescriptor typeDescriptor = this._typeDescriptor;
            TablePart tablePart = this._tablePart;
            Dictionary<MemberInfo, IncludeMemberInfo> relatedNavigationMembers = this._relatedNavigationMembers;

            DbExpression ret = null;

            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;
                MappingMemberDescriptor mappingMemberDescriptor = null;
                mappingMemberDescriptor = typeDescriptor.GetMappingMemberDescriptor(member);

                if (ret != null)
                {
                    ret = DbExpression.MemberAccess(member, ret);
                    continue;
                }

                if (mappingMemberDescriptor != null)
                {
                    DbTableExpression tableExpression = tablePart.Table;
                    DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(mappingMemberDescriptor.MemberType, tableExpression, mappingMemberDescriptor.ColumnName);

                    ret = columnAccessExpression;

                    continue;
                }
                else
                {
                    NavigationMemberDescriptor navigationMemberDescriptor = typeDescriptor.GetNavigationMemberDescriptor(member);
                    if (navigationMemberDescriptor == null)
                    {
                        throw new Exception(member.Name);
                    }

                    IncludeMemberInfo relatedMemberInfo;

                    if (!relatedNavigationMembers.TryGetValue(member, out relatedMemberInfo))
                    {
                        MappingTypeDescriptor navigationMemberTypeDescriptor = navigationMemberDescriptor.MemberType.GetEntityDescriptor();
                        JoinTablePart joinTablePart = this.CreateJoinTable(typeDescriptor, navigationMemberTypeDescriptor, member, tablePart.Table);
                        relatedMemberInfo = new IncludeMemberInfo(joinTablePart, navigationMemberDescriptor, navigationMemberTypeDescriptor);
                        relatedNavigationMembers.Add(member, relatedMemberInfo);

                        tablePart.JoinTables.Add(joinTablePart);
                    }

                    tablePart = relatedMemberInfo.TablePart;
                    typeDescriptor = relatedMemberInfo.MemberTypeDescriptor;
                    relatedNavigationMembers = relatedMemberInfo.IncludeMembers;
                }
            }

            if (ret == null)
                throw new Exception(memberExpressionDeriveParameter.ToString());

            return ret;
        }
        public IncludeMemberInfo IncludeNavigationMember(MemberExpression memberExpressionDeriveParameter)
        {
            DeriveType deriveType = memberExpressionDeriveParameter.GetMemberExpressionDeriveType();
            if (deriveType != DeriveType.Parameter)
                throw new NotSupportedException(string.Format("path", memberExpressionDeriveParameter.ToString()));

            IncludeMemberInfo deepestIncludeMemberInfo = null;

            Stack<MemberExpression> memberExpressions = memberExpressionDeriveParameter.Reverse();
            MappingTypeDescriptor typeDescriptor = this._typeDescriptor;
            TablePart tablePart = this._tablePart;
            Dictionary<MemberInfo, IncludeMemberInfo> relatedNavigationMembers = this._relatedNavigationMembers;
            Dictionary<MemberInfo, IncludeMemberInfo> includedNavigationMembers = this._includedNavigationMembers;
            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;
                NavigationMemberDescriptor navigationMemberDescriptor = typeDescriptor.GetNavigationMemberDescriptor(member);

                if (navigationMemberDescriptor == null)
                {
                    throw new Exception(string.Format("member:{0}", member.Name));
                }

                IncludeMemberInfo relatedMemberInfo;

                if (!relatedNavigationMembers.TryGetValue(member, out relatedMemberInfo))
                {
                    NavigationMemberDescriptor navMemberDescriptor = typeDescriptor.GetNavigationMemberDescriptor(member);
                    MappingTypeDescriptor navigationMemberTypeDescriptor = navigationMemberDescriptor.MemberType.GetEntityDescriptor();

                    JoinTablePart joinTablePart = this.CreateJoinTable(typeDescriptor, navigationMemberTypeDescriptor, member, tablePart.Table);

                    relatedMemberInfo = new IncludeMemberInfo(joinTablePart, navigationMemberDescriptor, navigationMemberTypeDescriptor);
                    relatedNavigationMembers.Add(member, relatedMemberInfo);

                    tablePart.JoinTables.Add(joinTablePart);
                }

                IncludeMemberInfo includeMemberInfo;
                if (!includedNavigationMembers.TryGetValue(member, out includeMemberInfo))
                {
                    MappingTypeDescriptor memberTypeDescriptor = relatedMemberInfo.MemberTypeDescriptor;
                    TablePart joinTablePart = relatedMemberInfo.TablePart;
                    includeMemberInfo = new IncludeMemberInfo(joinTablePart, navigationMemberDescriptor, memberTypeDescriptor);
                    includeMemberInfo.IsIncludeMember = true;
                    includedNavigationMembers.Add(member, includeMemberInfo);
                }

                tablePart = relatedMemberInfo.TablePart;
                typeDescriptor = relatedMemberInfo.MemberTypeDescriptor;
                relatedNavigationMembers = relatedMemberInfo.IncludeMembers;
                includedNavigationMembers = includeMemberInfo.IncludeMembers;
                deepestIncludeMemberInfo = includeMemberInfo;
            }

            if (deepestIncludeMemberInfo == null)
                throw new Exception(string.Format("path", memberExpressionDeriveParameter.ToString()));

            return deepestIncludeMemberInfo;
        }
        public IncludeMemberInfo VisistNavigationMember(MemberExpression memberExpressionDeriveParameter)
        {
            DeriveType deriveType = memberExpressionDeriveParameter.GetMemberExpressionDeriveType();
            if (deriveType != DeriveType.Parameter)
                throw new NotSupportedException(string.Format("path", memberExpressionDeriveParameter.ToString()));

            IncludeMemberInfo deepestIncludeMemberInfo = null;

            Stack<MemberExpression> memberExpressions = memberExpressionDeriveParameter.Reverse();
            MappingTypeDescriptor typeDescriptor = this._typeDescriptor;
            TablePart tablePart = this._tablePart;
            Dictionary<MemberInfo, IncludeMemberInfo> relatedNavigationMembers = this._relatedNavigationMembers;
            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;
                NavigationMemberDescriptor navigationMemberDescriptor = typeDescriptor.GetNavigationMemberDescriptor(member);

                if (navigationMemberDescriptor == null)
                {
                    throw new Exception(string.Format("member:{0}", member.Name));
                }

                IncludeMemberInfo relatedMemberInfo;

                if (!relatedNavigationMembers.TryGetValue(member, out relatedMemberInfo))
                {
                    NavigationMemberDescriptor navMemberDescriptor = typeDescriptor.GetNavigationMemberDescriptor(member);
                    MappingTypeDescriptor navigationMemberTypeDescriptor = navigationMemberDescriptor.MemberType.GetEntityDescriptor();

                    JoinTablePart joinTablePart = this.CreateJoinTable(typeDescriptor, navigationMemberTypeDescriptor, member, tablePart.Table);

                    relatedMemberInfo = new IncludeMemberInfo(joinTablePart, navigationMemberDescriptor, navigationMemberTypeDescriptor);
                    relatedNavigationMembers.Add(member, relatedMemberInfo);

                    tablePart.JoinTables.Add(joinTablePart);
                }

                tablePart = relatedMemberInfo.TablePart;
                typeDescriptor = relatedMemberInfo.MemberTypeDescriptor;
                relatedNavigationMembers = relatedMemberInfo.IncludeMembers;
                deepestIncludeMemberInfo = relatedMemberInfo;
            }

            if (deepestIncludeMemberInfo == null)
                throw new Exception(string.Format("path", memberExpressionDeriveParameter.ToString()));

            return deepestIncludeMemberInfo;
        }


        JoinTablePart CreateJoinTable(MappingTypeDescriptor typeDescriptor, MappingTypeDescriptor navigationMemberTypeDescriptor, MemberInfo navMember, DbTableExpression table)
        {
            NavigationMemberDescriptor navMemberDescriptor = typeDescriptor.GetNavigationMemberDescriptor(navMember);

            MappingMemberDescriptor thisKeyDescriptor = typeDescriptor.GetMappingMemberDescriptor(navMemberDescriptor.ThisKey);
            MappingMemberDescriptor associatingKeyDescriptor = navigationMemberTypeDescriptor.GetMappingMemberDescriptor(navMemberDescriptor.AssociatingKey);

            if (thisKeyDescriptor == null)
                throw new Exception(string.Format("ThisKey {0} 对应的成员不存在", navMemberDescriptor.ThisKey));
            if (associatingKeyDescriptor == null)
                throw new Exception(string.Format("AssociatingKey {0} 对应的成员不存在", navMemberDescriptor.AssociatingKey));

            string thisColumn; string associatingColumn; Type thisColumnType; Type associatingColumnType;

            if (thisKeyDescriptor.MemberType != associatingKeyDescriptor.MemberType)
            {
                Type thisKeyUnderlyingType = Nullable.GetUnderlyingType(thisKeyDescriptor.MemberType);
                if (thisKeyUnderlyingType != associatingKeyDescriptor.MemberType)
                {
                    Type associatingKeyUnderlyingType = Nullable.GetUnderlyingType(associatingKeyDescriptor.MemberType);
                    if (associatingKeyUnderlyingType != thisKeyDescriptor.MemberType)
                    {
                        throw new Exception(string.Format("导航属性 {0} 关联错误: ThisKey 类型 {1} 与 AssociatingKey 类型 {2} 关联失败", navMember.Name, thisKeyDescriptor.MemberType.FullName, associatingKeyDescriptor.MemberType.FullName));
                    }
                }
            }

            thisColumn = thisKeyDescriptor.ColumnName;
            thisColumnType = thisKeyDescriptor.MemberType;
            associatingColumn = associatingKeyDescriptor.ColumnName;
            associatingColumnType = associatingKeyDescriptor.MemberType;

            DbTableAccessExpression associatingTableAccessExp = new DbTableAccessExpression(navigationMemberTypeDescriptor.TableName);
            DbTableExpression associatingTable = new DbTableExpression(associatingTableAccessExp);
            JoinTablePart joinTablePart = this.CreateJoinTable(table, associatingTable, thisColumn, associatingColumn, thisColumnType, associatingColumnType);

            return joinTablePart;
        }
        JoinTablePart CreateJoinTable(DbTableExpression table, DbTableExpression associatingTable, string thisColumn, string associatingColumn, Type thisColumnType, Type associatingColumnType)
        {
            //TODO Nullable DbConvertExpression
            DbColumnAccessExpression left = new DbColumnAccessExpression(thisColumnType, table, thisColumn);
            DbColumnAccessExpression right = new DbColumnAccessExpression(associatingColumnType, associatingTable, associatingColumn);
            DbExpression on = DbExpression.Equal(left, right);  //构建一个 on 连接条件表达式树

            JoinTablePart joinTable = new JoinTablePart(JoinType.LeftJoin, associatingTable, table, on);
            return joinTable;
        }
        TablePart CreateRootTable(string rawTableName)
        {
            DbTableAccessExpression rootTable = new DbTableAccessExpression(rawTableName);
            DbTableExpression tableExp = new DbTableExpression(rootTable);
            TablePart table = new TablePart(tableExp);
            return table;
        }

        public TablePart RootTablePart { get { return this._tablePart; } }
        public MappingTypeDescriptor TypeDescriptor { get { return this._typeDescriptor; } }

    }

    public class IncludeMemberInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablePart"></param>
        /// <param name="memberDescriptor">导航属性的 NavigationMemberDescriptor</param>
        /// <param name="memberTypeDescriptor">导航属性类型 MappingTypeDescriptor</param>
        public IncludeMemberInfo(TablePart tablePart, NavigationMemberDescriptor memberDescriptor, MappingTypeDescriptor memberTypeDescriptor)
        {
            this.TablePart = tablePart;
            this.MemberDescriptor = memberDescriptor;
            this.MemberTypeDescriptor = memberTypeDescriptor;
            this.IncludeMembers = new Dictionary<MemberInfo, IncludeMemberInfo>();
        }
        public TablePart TablePart { get; set; }
        /// <summary>
        /// 导航属性的 NavigationMemberDescriptor
        /// </summary>
        public NavigationMemberDescriptor MemberDescriptor { get; set; }
        public bool IsIncludeMember { get; set; }
        /// <summary>
        /// 导航属性类型 MappingTypeDescriptor
        /// </summary>
        public MappingTypeDescriptor MemberTypeDescriptor { get; set; }
        public Dictionary<MemberInfo, IncludeMemberInfo> IncludeMembers { get; set; }

        /// <summary>
        /// 获取 this.MemberDescriptor.AssociatingKey 在 MemberTypeDescriptor 中相关联的 MemberInfo
        /// </summary>
        /// <returns></returns>
        public MemberInfo GetAssociatingMemberInfo()
        {
            return this.MemberTypeDescriptor.GetMappingMemberDescriptor(this.MemberDescriptor.AssociatingKey).MemberInfo;
        }
    }
}
