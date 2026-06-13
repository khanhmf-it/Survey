using System.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using SURVEY.Model.DTOs;
using SURVEY.Models;
using SURVEY.Service.Services.Implementations;

namespace SURVEY.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EmployeeEvaluationService _employeeEvaluationService;

        public HomeController(ILogger<HomeController> logger, EmployeeEvaluationService employeeEvaluationService)
        {
            _logger = logger;
            _employeeEvaluationService = employeeEvaluationService;
        }

        public IActionResult Index()
        {
            return View();
        }
        // chuyển ngôn ngữ
        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl = "/survey")
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = "vi";
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                });

            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home") ?? "/survey";
            }

            return LocalRedirect(returnUrl);
        }

        // Thêm đánh giá công nhân viên
        [HttpPost]
        public async Task<IActionResult> AddEvaluation([FromBody] employee_evaluationDTO evaluation)
        {
            try
            {
                var result = await _employeeEvaluationService.AddEmployeeEvaluationAsync(evaluation);
                if (!result.Success)
                {
                    return BadRequest("Error import employee evaluation: " + result.Message);
                }
                return Ok();
            }
            catch (Exception ex) { 
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        // Thêm đánh giá công nhân viên
        [HttpPost]
        public async Task<IActionResult> SendMailSectionManager([FromBody] employee_evaluationDTO evaluation)
        {
            try
            {
                var result = await _employeeEvaluationService.SendEmailToSectionManagerAsync(evaluation);
                if (!result.Success)
                {
                    return BadRequest("Error sending email to section manager: " + result.Message);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
