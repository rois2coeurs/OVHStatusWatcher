using System.ComponentModel.DataAnnotations;

namespace OVHStatusWatcher.Models;

public class ServiceType
{
    public long Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }
}