namespace SURVEY.Model.Common.Enum
{
    public enum OrderDirection
    {
        Asc,
        Desc
    }
    public enum LogicType
    {
        None,
        Not,
        And,
        Or
    }
    public enum OperatorType
    {
        Equal = 1,
        NotEqual,
        Greater,
        GreaterThanEqual,
        Lower,
        LowerThanEqual,
        Contain,
        StartWith,
        EndWith,
        In,
        NotIn,
        IsNull,
        IsNotNull
    }
}
