using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace OVHStatusWatcher;

public class Worker(IServiceProvider serviceProvider) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                CheckOvhStatus(db);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Task.Delay(TimeSpan.FromSeconds(15), stoppingToken).Wait(stoppingToken);
        }

        return Task.CompletedTask;
    }

    private static void CheckOvhStatus(MyDbContext db)
    {
        Console.WriteLine("Checking OVH status...");

        const string url = "https://bare-metal-servers.status-ovhcloud.com/history.rss";
        using var reader = XmlReader.Create(url);
        var feed = SyndicationFeed.Load(reader);

        var datacenters = db.Trackers.Select(t => t.Datacenter).Distinct().ToList();
        var regions = datacenters.Select(GetOnlyAlphabetical).Distinct().ToList();

        var firstPost = feed
            .Items
            .Where(item => datacenters.Any(dc => item.Title.Text.Contains(dc, StringComparison.OrdinalIgnoreCase))
                           || regions.Any(region =>
                               item.Title.Text.Contains(region, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Console.WriteLine(firstPost);
    }

    private static string GetOnlyAlphabetical(string input)
    {
        return new string(input.Where(char.IsLetter).ToArray());
    }
}