using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface ISwitchBus
    {
        Task<T> Notify<T>(string messageName, IDictionary<string, object> eventData);
    }
}
