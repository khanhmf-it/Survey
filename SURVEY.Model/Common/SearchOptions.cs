using SURVEY.Model.Common.Enum;

namespace SURVEY.Model.Common
{
    public class SearchOptions
    {
        public string SearchTerm { get; set; }
        public List<string> SearchFields { get; set; } = new(); // "Name,Email,Category"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public List<FilterOption> Filters { get; set; } = new();
        public List<SortOption> SortOptions { get; set; } = new();
    }

    public class SortOption
    {
        public string Field { get; set; }
        public string SortDirection { get; set; } = OrderDirection.Asc.ToString(); // ASC, DESC
    }

    public class FilterOption
    {
        public string Field { get; set; }
        public object Value { get; set; } // Có thể là 1 giá trị, 1 danh sách, hoặc 1 tuple (cho BETWEEN)

        // Trường hợp sử dụng BETWEEN thì Value bắt buộc phải có 2 giá trị có hậu tố _from và _to
        public string Operator { get; set; } = "="; // =, <>, >, <, LIKE, IN, BETWEEN, IS NULL, IS NOT NULL
        public string LogicType { get; set; } = "AND"; // AND, OR
    }
}
