
namespace Chloe.DbExpressions
{
    public enum DbExpressionType
    {
        And = 1,
        Or,
        Equal,
        NotEqual,
        Not,
        Convert,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Add,
        Subtract,
        Multiply,
        Divide,
        Constant,
        CaseWhen,
        MemberAccess,
        Call,
        Table,
        DerivedTable,
        Column,
        ColumnAccess,
        Parameter,
        SubQuery,
        SqlQuery,
        FromTable,
        JoinTable,
        OrderSegment,
    }
}
