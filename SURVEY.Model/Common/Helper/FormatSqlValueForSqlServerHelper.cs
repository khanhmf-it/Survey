using SURVEY.Model.Common.Enum;

namespace SURVEY.Model.Common.Helper
{
    public static class FormatSqlValueForSqlServerHelper
    {
        public static string FormatSqlValue(Filter filter)
        {
            string value = filter.Value?.Replace("'", "''") ?? string.Empty;

            return filter.Operator switch
            {
                OperatorType.Contain => $"'%{value}%'",
                OperatorType.StartWith => $"'{value}%'",
                OperatorType.EndWith => $"'%{value}'",
                OperatorType.In or OperatorType.NotIn => $"({value})", // đảm bảo value là danh sách đúng định dạng
                OperatorType.IsNull or OperatorType.IsNotNull => string.Empty,
                _ => $"'{value}'"
            };
        }

    }
}
