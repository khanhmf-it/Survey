using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using SURVEY.Models;
using SURVEY.Service.Services.Implementations;

namespace SURVEY.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ILogger<ReviewController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly EmployeeEvaluationService _employeeEvaluationService;

        public ReviewController(ILogger<ReviewController> logger, EmployeeEvaluationService employeeEvaluationService, IWebHostEnvironment env)
        {
            _logger = logger;
            _employeeEvaluationService = employeeEvaluationService;
            _env = env;
        }
        public IActionResult ViewAllReviews()
        {
            return View();
        }
        // Tim kiem đánh giá công nhân viên
        [HttpPost]
        public async Task<IActionResult> SearchReviews([FromBody] ReviewSearchModel searchModel)
        {
            try
            {
                var reviews = await _employeeEvaluationService.GetEvaluationsByEvaluatorIdAsync(
                    searchModel.EmployeeId,
                    searchModel.Group, searchModel.DateFrom,
                    searchModel.DateTo, searchModel.PageIndex,
                    searchModel.PageSize);
                if (!reviews.Success)
                {
                    return BadRequest(reviews.Message);
                }
                return Ok(reviews.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reviews");
                return BadRequest(ex.Message);
            }
        }
        // Xuất dữ liệu đánh giá công nhân viên ra file Excel
        [HttpPost]
        public async Task<IActionResult> ExportReviewsToExcel([FromBody] ReviewSearchModel searchModel)
        {
            try
            {
                var reviews = await _employeeEvaluationService.GetEvaluationsByEvaluatorIdAsync(
                    searchModel.EmployeeId,
                    searchModel.Group, searchModel.DateFrom,
                    searchModel.DateTo, null, null);
                if (!reviews.Success)
                {
                    return BadRequest(reviews.Message);
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = System.IO.Path.Combine(root, "template", "TemplateQuotationResults.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemplateQuotationResults.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }
                int rowStart = 4;
                foreach (var item in reviews.Data)
                {
                    int col = 1;
                    ws.Cell(rowStart, col++).SetValue(rowStart - 3);
                    ws.Cell(rowStart, col++).SetValue(item.employee_code ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.employee_name ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.department ?? string.Empty);

                    ws.Cell(rowStart, col++).SetValue(item.g1_good_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g1_good_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g1_improve_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g1_improve_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g1_example ?? string.Empty);


                    ws.Cell(rowStart, col++).SetValue(item.g2_good_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g2_good_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g2_improve_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g2_improve_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g2_example ?? string.Empty);

                    ws.Cell(rowStart, col++).SetValue(item.g3_good_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g3_good_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g3_improve_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g3_improve_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g3_example ?? string.Empty);

                    ws.Cell(rowStart, col++).SetValue(item.g4_good_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g4_good_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g4_improve_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g4_improve_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g4_example ?? string.Empty);

                    ws.Cell(rowStart, col++).SetValue(item.g5_good_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g5_good_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g5_improve_point ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.g5_improve_score ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.g5_example ?? string.Empty);

                    ws.Cell(rowStart, col++).SetValue(item.improvement_proposal ?? null);
                    ws.Cell(rowStart, col++).SetValue(item.created_at ?? null);

                    rowStart++;
                }
                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"DataEmployeeEvaluation_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting reviews to Excel");
                return BadRequest(ex.Message);
            }
        }
    }
}
