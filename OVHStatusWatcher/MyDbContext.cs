using Microsoft.EntityFrameworkCore;
using OVHStatusWatcher.Models;

namespace OVHStatusWatcher;

public class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    public DbSet<Tracker> Trackers { get; set; }
    public DbSet<Datacenter> Datacenters { get; set; }
    public DbSet<Rack> Racks { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }
}