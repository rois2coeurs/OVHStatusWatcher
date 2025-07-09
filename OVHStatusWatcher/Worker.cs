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
            await ProcessPost(post, db);
        }
    }

    private static async Task ProcessPost(SyndicationItem post, MyDbContext db)
    {
        var discordWebhook =
            new DiscordWebhookHelper(
                "https://discord.com/api/webhooks/1389631246786891888/zYBi8gr7mhuc8Yg4_PI0hmN01dQ2EnH47cQ9u1KFPoUMwTVKyu1vlY6oF80v42zqk426");
        if (OvhDataHelper.IsRack(post.Title.Text))
        {
            try
            {
                var rackNum = OvhDataHelper.ExtractRackNumber(post.Title.Text);
                var rack = db.Racks.FirstOrDefault(r => r.Name == rackNum);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while trying to extract rack number");
            }
        }

        var envs = OvhDataHelper.ExtractEnvs(post.Title.Text);

        foreach (var env in envs)
        {
            if (OvhDataHelper.IsRegion(env))
            {
                var region = GetOrCreateRegion(db, env);
                await discordWebhook.SendRegionNotificationAsync(region, post);
            }
            else
            {
                var datacenter = GetOrCreateDatacenter(db, env);
                await discordWebhook.SendDataCenterNotificationAsync(datacenter, post);
            }
        }

        Console.WriteLine(envs);
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