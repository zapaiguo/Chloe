using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
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
        TableAccess,
        Column,
        ColumnAccess,
        Parameter,
        SubQuery,
        SqlQuery,
    }
}
