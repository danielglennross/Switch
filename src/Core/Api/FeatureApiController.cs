using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Core.Extensions;
using Core.Services;

namespace Core.Api
{
    [RoutePrefix("api/switch")]
    public class FeatureApiController : ApiController
    {
        private readonly IFeatureActionService _featureActionService;
        private readonly IFeatureInfoService _featureInfoService;

        public FeatureApiController(
            IFeatureInfoService featureInfoService,
            IFeatureActionService featureActionService)
        {
            _featureInfoService = featureInfoService;
            _featureActionService = featureActionService;
        }

        [Route]
        public async Task<IHttpActionResult> Get()
        {
            var features = 
                await _featureInfoService.GetFeaturesItems().ConfigureAwait(false);

            return Ok(features);
        }

        [Route]
        public async Task<IHttpActionResult> Put([FromBody]FeatureChangeRequest featureChangeRequest)
        {
            await featureChangeRequest
                .FeaturesToEnable
                .ForEachAsync(x => _featureActionService.EnableFeature(x))
                .ConfigureAwait(false);

            await featureChangeRequest
                .FeaturesToDisable
                .ForEachAsync(x => _featureActionService.DisableFeature(x))
                .ConfigureAwait(false);

            return Ok();
        }
    }
}
