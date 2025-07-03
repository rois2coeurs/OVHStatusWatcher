using System.ComponentModel.DataAnnotations;

namespace OVHStatusWatcher.Models;

public class Tracker
{
    public long Id { get; set; }
    
    [MaxLength(5)]
    public required string Datacenter { get; set; }
    
    public required ServiceType ServiceType { get; set; }
    
    [MaxLength(20)]
    public string? Rack { get; set; }
    
    public required string WebHookUrl { get; set; }
    
    public DateTime? LastCheck { get; set; } 
}

public enum ServiceType
{
    DedicatedServers = 0,
    DomainNameSystem = 1,
    Licensing = 2,
    StorageAndBackups = 3,
    IdentitySecurityOperations = 4,
    ManagedServices = 5,
    VirtualPrivateServers = 6
}