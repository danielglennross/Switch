using System.Collections.Generic;
using System.Threading.Tasks;
using CoreDNX;
using CoreDNX.Services;
using Microsoft.AspNet.Mvc;
using CoreDNX.Extensions;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly TestFeatures.ITestFeature _testFeature;
        private readonly IFeatureActionService _featureService;
        private readonly IFeatureInfoService _featureInfoService;

        public HomeController(
            TestFeatures.ITestFeature testFeature, 
            IFeatureActionService featureService,
            IFeatureInfoService featureInfoService)
        {
            _featureInfoService = featureInfoService;
            _featureService = featureService;
            _testFeature = testFeature;
        }

        public IActionResult Index()
        {
            //var value = _testFeature.Run();
            //ViewBag.Value = value;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var features = await _featureInfoService.GetFeaturesItems().ConfigureAwait(false);
            return Ok(features);
        }

        [HttpPost]
        public async Task<IActionResult> Put([FromBody]FeatureChangeRequest featureChangeRequest)
        {
            await featureChangeRequest
                .FeaturesToEnable
                .ForEachAsync(x => _featureService.EnableFeature(x))
                .ConfigureAwait(false);

            await featureChangeRequest
                .FeaturesToDisable
                .ForEachAsync(x => _featureService.DisableFeature(x))
                .ConfigureAwait(false);

            return Ok();
        }
    }

    public struct FeatureChangeRequest
    {
        public IEnumerable<string> FeaturesToEnable { get; set; }
        public IEnumerable<string> FeaturesToDisable { get; set; }
    }
}
