namespace SURVEY.Models
{
    public class ReviewSearchModel
    {
        public string? EmployeeId { get; set; }
        public string? Group { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
