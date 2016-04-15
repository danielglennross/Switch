using CoreDNX.Autofac;
using Microsoft.AspNet.Mvc;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITestFeature _testFeature;

        public HomeController(ITestFeature testFeature)
        {
            _testFeature = testFeature;
        }

        public IActionResult Index()
        {
            var value = _testFeature.Run();
            ViewBag.Value = value;
            return View();
        }
    }
}
