
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
        TableSegment,

        ColumnAccess,
        ColumnSegment,

        Parameter,
        FromTable,
        JoinTable,
        OrderSegment,
        Function,

        SqlQuery,
        SubQuery,
        Insert,
        Update,
        Delete,

    }
}
