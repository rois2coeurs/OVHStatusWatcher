using System.ServiceModel.Syndication;
using OVHStatusWatcher.Models;

namespace OVHStatusWatcher.Helpers;

public interface INotificationHelper
{
    public Task SendRackNotificationAsync(Rack rack, SyndicationItem post);

    public Task SendDataCenterNotificationAsync(Datacenter datacenter, SyndicationItem post);

    public Task SendRegionNotificationAsync(Region region, SyndicationItem post);
}