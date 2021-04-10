using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notifications.services
{
    public interface IPublishService
    {
        Task Publish(Dictionary<string, object> data);
        Task PublishPatchNotes(Dictionary<string, object> data);

    }
}