using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreDNX.Services
{
    public interface ISwitchBus
    {
        Task<object> Notify(string messageName, IDictionary<string, object> eventData);
    }
}
