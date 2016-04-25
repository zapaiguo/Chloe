
namespace Chloe.DbExpressions
{
    public enum DbExpressionType
    {
        SqlQuery = 1,
        Insert,
        Update,
        Delete,
        SubQuery,

        And,
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

        Column,
        ColumnAccess,
        ColumnSegment,

        Parameter,
        FromTable,
        JoinTable,
        OrderSegment,
        Function,
    }
}
