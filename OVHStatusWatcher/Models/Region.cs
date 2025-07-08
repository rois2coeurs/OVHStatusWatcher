using System.ComponentModel.DataAnnotations;

namespace OVHStatusWatcher.Models;

public class Region
{
    public long Id { get; set; }

    [MaxLength(10)] public required string Name { get; set; }
}