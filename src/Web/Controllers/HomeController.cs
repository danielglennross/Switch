using CoreDNX.Autofac;
using CoreDNX.Services;
using Microsoft.AspNet.Mvc;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITestFeature _testFeature;
        private readonly IFeatureActionService _featureService;

        public HomeController(ITestFeature testFeature, IFeatureActionService featureService)
        {
            _featureService = featureService;
            _testFeature = testFeature;
        }

        public IActionResult Index()
        {
            var value = _testFeature.Run();
            ViewBag.Value = value;
            return View();
        }

        [HttpPost]
        public IActionResult Enable(string name)
        {
            _featureService.EnableFeature(name);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Disable(string name)
        {
            _featureService.DisableFeature(name);
            return RedirectToAction("Index");
        }
    }
}
