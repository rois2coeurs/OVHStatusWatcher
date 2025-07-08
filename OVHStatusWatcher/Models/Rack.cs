using System.ComponentModel.DataAnnotations;

namespace OVHStatusWatcher.Models;

public class Rack
{
    public long Id { get; set; }

    [MaxLength(20)]
    public required string Name { get; set; }

    public required Datacenter Datacenter { get; set; }
}