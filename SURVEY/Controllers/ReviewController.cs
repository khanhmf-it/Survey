using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using SURVEY.Models;
using SURVEY.Service.Services.Implementations;
using SURVEY.Service.Services.Interfaces;
using System.Security.Claims;

namespace SURVEY.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ILogger<ReviewController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly EmployeeEvaluationService _employeeEvaluationService;
        private readonly IAuthenticationService _authenticationService;

        public ReviewController(ILogger<ReviewController> logger, EmployeeEvaluationService employeeEvaluationService, IAuthenticationService authenticationService, IWebHostEnvironment env)
        {
            _logger = logger;
            _employeeEvaluationService = employeeEvaluationService;
            _authenticationService = authenticationService;
            _env = env;
        }
        public async Task<IActionResult> ViewAllReviews()
        {
            var check = await EnsureUserAllowedAsync();
            if (check != null) return check;
            return View();
        }
        // Tim kiem đánh giá công nhân viên
        [HttpPost]
        public async Task<IActionResult> SearchReviews([FromBody] ReviewSearchModel searchModel)
        {
            try
            {
                var check = await EnsureUserAllowedAsync();
                if (check != null) return check;
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
                var check = await EnsureUserAllowedAsync();
                if (check != null) return check;
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
                    return BadRequest("Không tìm thấy file template: TemplateSurvey 360.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }
                int rowStart = 60;
                foreach (var item in reviews.Data)
                {
                    int col = 1;
                    //ws.Cell(rowStart, col++).SetValue(rowStart - 3);
                    ws.Cell(rowStart, col++).SetValue(item.department ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.employee_code ?? string.Empty);
                    //ws.Cell(rowStart, col++).SetValue(item.employee_name ?? string.Empty);

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
                    //ws.Cell(rowStart, col++).SetValue(item.created_at ?? null);

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

        private async Task<IActionResult?> EnsureUserAllowedAsync()
        {
            try
            {
                var userAdid = User.Identity?.Name?.Split('\\').LastOrDefault() ?? "";
                if (string.IsNullOrEmpty(userAdid))
                {
                    return Unauthorized();
                }

                var existRes = await _authenticationService.CheckUserExistAsync(userAdid);
                if (existRes == null)
                {
                    _logger.LogWarning("Authentication service returned null when checking user existence for {User}", userAdid);
                    return Forbid();
                }
                if (!existRes.Success)
                {
                    _logger.LogWarning("Error checking user existence for {User}: {Message}", userAdid, existRes.Message);
                    return BadRequest(existRes.Message);
                }
                if (!existRes.Data)
                {
                    _logger.LogInformation("User {User} is not allowed to perform this action.", userAdid);
                    return Forbid();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while verifying user permission");
                return BadRequest("Error verifying user permission");
            }
        }

    }
}
