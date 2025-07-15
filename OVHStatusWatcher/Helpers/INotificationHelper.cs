using System.ServiceModel.Syndication;
using OVHStatusWatcher.Models;

namespace OVHStatusWatcher.Helpers;

public interface INotificationHelper
{
    public Task SendDataCenterNotificationAsync(Datacenter datacenter, SyndicationItem post,
        CancellationToken cancellationToken = default);

    public Task SendRegionNotificationAsync(Region region, SyndicationItem post, string? discordMessage = null,
        CancellationToken cancellationToken = default);
}