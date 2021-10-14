using Microsoft.AspNetCore.Mvc;

namespace TaskBoard.WebAPI.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Gets API info (Swagger UI).
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     GET /
        /// </remarks>
        /// <response code="200">Returns "OK" with API info.</response>
        [HttpGet]
        [Route("/")]
        [Route("/api")]
        [Route("/swagger")]
        public IActionResult Index()
        {
            return LocalRedirect(@"/api/docs/index.html");
        }
    }
}
