using System.ComponentModel.DataAnnotations;

namespace OVHStatusWatcher.Models;

public class Datacenter
{
    public long Id { get; set; }

    [MaxLength(10)] public required string Name { get; set; }

    public required Region Region { get; set; }
}