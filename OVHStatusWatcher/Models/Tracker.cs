using System.ComponentModel.DataAnnotations;

namespace OVHStatusWatcher.Models;

public class Tracker
{
    public long Id { get; set; }

    [MaxLength(512)] public required string WebHookUrl { get; set; }

    public required ServiceType ServiceType { get; set; }

    public Region? Region { get; set; }
    public Datacenter? Datacenter { get; set; }
    public Rack? Rack { get; set; }
}