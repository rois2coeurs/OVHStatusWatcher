using Microsoft.EntityFrameworkCore;
using OVHStatusWatcher.Models;

namespace OVHStatusWatcher;

public class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    public DbSet<Tracker> Trackers { get; set; }

}