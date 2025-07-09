using System.ServiceModel.Syndication;
using System.Xml;
using OVHStatusWatcher.Helpers;
using OVHStatusWatcher.Models;

namespace OVHStatusWatcher;

public class Worker(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOvhStatus(db);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).WaitAsync(stoppingToken);
        }
    }

    private static async Task CheckOvhStatus(MyDbContext db)
    {
        Console.WriteLine("Checking OVH status...");

        const string url = "https://bare-metal-servers.status-ovhcloud.com/history.rss";
        using var reader = XmlReader.Create(url);
        var feed = SyndicationFeed.Load(reader);

        var posts = feed.Items.ToList();

        foreach (var post in posts)
        {
            try
            {
                await ProcessPost(post, db);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing post '{post.Title.Text}': {ex.Message}");
            }
        }
    }

    private static async Task ProcessPost(SyndicationItem post, MyDbContext db)
    {
        var discordWebhook = new DiscordWebhookHelper(db);
        if (OvhDataHelper.IsRack(post.Title.Text))
        {
            try
            {
                var rackNum = OvhDataHelper.ExtractRackNumber(post.Title.Text);
                var rack = db.Racks.FirstOrDefault(r => r.Name == rackNum);
                if (rack is not null)
                {
                    await discordWebhook.SendRackNotificationAsync(rack, post);
                    return;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong while trying to extract rack number");
            }
        }

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
            await discordWebhook.SendRegionNotificationAsync(region, post);
        }

        foreach (var datacenter in datacenters)
        {
            await discordWebhook.SendDataCenterNotificationAsync(datacenter, post);
        }
    }

    private static Datacenter GetOrCreateDatacenter(MyDbContext db, string datacenter)
    {
        var obj = db.Datacenters.FirstOrDefault(d => d.Name == datacenter);
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