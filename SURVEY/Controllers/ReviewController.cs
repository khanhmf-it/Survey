using Microsoft.AspNetCore.Mvc;

namespace SURVEY.Controllers
{
    public class ReviewController : Controller
    {
        public IActionResult ViewAllReviews()
        {
            return View();
        }
    }
}
