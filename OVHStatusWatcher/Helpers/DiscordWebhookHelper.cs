using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using OVHStatusWatcher.Models;

namespace OVHStatusWatcher.Helpers;

public partial class DiscordWebhookHelper(MyDbContext db) : INotificationHelper
{
    private static async Task SendDiscordNotificationAsync(string message, string webhookUrl,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();

        var content = new StringContent(
            message,
            System.Text.Encoding.UTF8,
            "application/json"
        );
        var res = await httpClient.PostAsync(webhookUrl, content, cancellationToken);
        if (!res.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Failed to send notification: {res.StatusCode} - {await res.Content.ReadAsStringAsync(cancellationToken)}");
        }
    }

    private async Task SendRackNotificationAsync(Rack rack, string discordMessage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rack);

        var trackers = await db.Trackers
            .Where(t => t.Rack != null && t.Rack.Id == rack.Id)
            .Select(t => t.WebHookUrl)
            .ToListAsync(cancellationToken);

        foreach (var tracker in trackers)
        {
            await SendDiscordNotificationAsync(discordMessage, tracker, cancellationToken);
        }
    }

    public async Task SendDataCenterNotificationAsync(Datacenter datacenter, SyndicationItem post,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datacenter);

        var trackers = await db.Trackers
            .Where(t => t.Datacenter != null && t.Datacenter.Id == datacenter.Id)
            .Select(t => t.WebHookUrl)
            .ToListAsync(cancellationToken: cancellationToken);

        var discordMessage = GetDiscordMessage(post);

        if (post.Title.Text.Contains("Rack", StringComparison.OrdinalIgnoreCase))
        {
            var racks = await db.Racks.Where(r => r.Datacenter == datacenter && post.Title.Text.Contains(r.Name))
                .ToListAsync(cancellationToken: cancellationToken);
            foreach (var rack in racks)
            {
                await SendRackNotificationAsync(rack, discordMessage, cancellationToken);
            }
        }

        foreach (var tracker in trackers)
        {
            await SendDiscordNotificationAsync(discordMessage, tracker, cancellationToken);
        }

        await SendRegionNotificationAsync(datacenter.Region, post, discordMessage, cancellationToken);
    }

    public async Task SendRegionNotificationAsync(Region region, SyndicationItem post, string? discordMessage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(region);

        var trackers = await db.Trackers
            .Where(t => t.Region != null && t.Region.Id == region.Id)
            .Select(t => t.WebHookUrl)
            .ToListAsync(cancellationToken: cancellationToken);

        if (string.IsNullOrEmpty(discordMessage)) discordMessage = GetDiscordMessage(post);

        foreach (var tracker in trackers)
        {
            await SendDiscordNotificationAsync(discordMessage, tracker, cancellationToken);
        }
    }

    private static string GetDiscordMessage(SyndicationItem post)
    {
        ArgumentNullException.ThrowIfNull(post);

        var description = ConvertHtmlToMarkdown(post.Summary.Text);
        var imageUrl = string.Empty;
        if (RegexImgTag().IsMatch(description))
        {
            var match = RegexImgTag().Match(description);
            if (match.Success)
            {
                imageUrl = match.Groups[1].Value;
            }
        }

        var message = new DiscordMessage
        {
            Content = "Embed",
            Embeds =
            [
                new Embed
                {
                    Title = post.Title.Text,
                    Description = ConvertHtmlToMarkdown(post.Summary.Text),
                    Color = 5793266,
                    Footer = new EmbedFooter
                    {
                        Text = "OVH Status Watcher",
                        IconUrl = "https://www.ovh.com/manager/images/logo-ovh.svg"
                    },
                    Image = new EmbedImage
                    {
                        Url = imageUrl
                    },
                }
            ]
        };

        return JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    private static string ConvertHtmlToMarkdown(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;

        html = html.Replace("<br>", "\n")
            .Replace("<b>", "**")
            .Replace("</b>", "**")
            .Replace("<i>", "*")
            .Replace("</i>", "*")
            .Replace("<p>", "")
            .Replace("</p>", "\n")
            .Replace("<br />", "\n")
            .Replace("<strong>", "**")
            .Replace("</strong>", "**")
            .Replace("<small>", "_")
            .Replace("</small>", "_")
            .Replace("<h1>", "# ")
            .Replace("</h1>", "\n")
            .Replace("<h2>", "## ")
            .Replace("</h2>", "\n")
            .Replace("<h3>", "### ")
            .Replace("</h3>", "\n")
            .Replace("<h4>", "#### ")
            .Replace("</h4>", "\n")
            .Replace("<h5>", "##### ")
            .Replace("</h5>", "\n")
            .Replace("<html>", "")
            .Replace("</html>", "")
            .Replace("<body>", "")
            .Replace("</body>", "");

        html = RegexVarTag().Replace(html, m => $"`{m.Groups[2].Value}`");
        html = RegexHrefTag().Replace(html, m => $"[{m.Groups[2].Value}]({m.Groups[1].Value})");
        return html.Trim();
    }

    [GeneratedRegex("<var(.*?)>(.*?)</var>", RegexOptions.IgnoreCase, "")]
    private static partial Regex RegexVarTag();

    [GeneratedRegex("<a href=\"(.*?)\">(.*?)</a>", RegexOptions.IgnoreCase, "")]
    private static partial Regex RegexHrefTag();

    [GeneratedRegex("<img src=\"(.*?)\"/>", RegexOptions.IgnoreCase, "")]
    private static partial Regex RegexImgTag();
}

public class Embed
{
    [JsonPropertyName("title")] public string Title { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("color")] public int Color { get; set; }
    [JsonPropertyName("footer")] public EmbedFooter Footer { get; set; }
    [JsonPropertyName("image")] public EmbedImage Image { get; set; }
    [JsonPropertyName("thumbnail")] public string ThumbnailUrl { get; set; }
    [JsonPropertyName("fields")] public List<EmbedField> Fields { get; set; }
}

public class EmbedField
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("value")] public string Value { get; set; }
    [JsonPropertyName("inline")] public bool Inline { get; set; } = false;
}

public class DiscordMessage
{
    [JsonPropertyName("content")] public string Content { get; set; }
    public bool tts { get; set; } = false;
    [JsonPropertyName("embeds")] public List<Embed> Embeds { get; set; } = [];
}

public class EmbedFooter
{
    [JsonPropertyName("text")] public string Text { get; set; }

    [JsonPropertyName("icon_url")] public string IconUrl { get; set; }
}

public class EmbedImage
{
    [JsonPropertyName("url")] public string Url { get; set; }
}