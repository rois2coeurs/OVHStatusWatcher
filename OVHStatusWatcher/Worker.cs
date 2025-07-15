using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using OVHStatusWatcher.Helpers;
using OVHStatusWatcher.Models;

namespace OVHStatusWatcher;

public class Worker(IServiceProvider serviceProvider) : BackgroundService
{
    private static readonly HashSet<string> ProcessedPosts = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOvhStatus(db, "https://bare-metal-servers.status-ovhcloud.com/history.rss", stoppingToken);
                await CheckOvhStatus(db, "https://network.status-ovhcloud.com/history.rss", stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).WaitAsync(stoppingToken);
        }
    }

    private static async Task CheckOvhStatus(MyDbContext db, string url, CancellationToken stoppingToken = default)
    {
        Console.WriteLine("Checking OVH status...");

        using var reader = XmlReader.Create(url);
        var feed = SyndicationFeed.Load(reader);

        var posts = feed.Items.ToList();

        foreach (var post in posts)
        {
            try
            {
                if (post.Id is null || ProcessedPosts.Contains(post.Id)) continue;

                await ProcessPost(post, db, stoppingToken);
                ProcessedPosts.Add(post.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing post '{post.Title.Text}': {ex.Message}");
            }
        }

        Console.WriteLine("OVH status check completed.");
    }

    private static async Task ProcessPost(SyndicationItem post, MyDbContext db, CancellationToken stoppingToken = default)
    {
        var discordWebhook = new DiscordWebhookHelper(db);

        var envs = OvhDataHelper.ExtractEnvs(post.Title.Text);

        List<Region> regions = [];
        List<Datacenter> datacenters = [];

        foreach (var env in envs)
        {
            if (OvhDataHelper.IsRegion(env))
            {
                regions.Add(GetOrCreateRegion(db, env));
            }
            else
            {
                datacenters.Add(GetOrCreateDatacenter(db, env));
            }
        }

        foreach (var region in regions)
        {
            await discordWebhook.SendRegionNotificationAsync(region, post, cancellationToken: stoppingToken);
        }

        foreach (var datacenter in datacenters)
        {
            await discordWebhook.SendDataCenterNotificationAsync(datacenter, post, stoppingToken);
        }
    }

    private static Datacenter GetOrCreateDatacenter(MyDbContext db, string datacenter)
    {
        var obj = db.Datacenters.Include(d => d.Region).FirstOrDefault(d => d.Name == datacenter);
        if (obj is not null) return obj;
        var region = GetOrCreateRegion(db, datacenter);
        obj = new Datacenter
        {
            Name = datacenter,
            Region = region
        };
        db.Add(obj);
        db.SaveChanges();
        return obj;
    }

    private static Region GetOrCreateRegion(MyDbContext db, string datacenter)
    {
        var region = OvhDataHelper.ExtractRegionFromDatacenter(datacenter);
        var regionObj = db.Regions.FirstOrDefault(r => r.Name == region);
        if (regionObj is not null) return regionObj;
        regionObj = new Region
        {
            Name = region
        };
        db.Add(regionObj);
        db.SaveChanges();
        return regionObj;
    }
}