using System.Collections.Generic;

namespace Core.Api
{
    public struct FeatureChangeRequest
    {
        public IEnumerable<string> FeaturesToEnable { get; set; }
        public IEnumerable<string> FeaturesToDisable { get; set; }
    }
}
