using System.ServiceModel.Syndication;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using OVHStatusWatcher.Models;

namespace OVHStatusWatcher.Helpers;

public partial class DiscordWebhookHelper(string webhookUrl) : INotificationHelper
{
    private async Task SendDiscordNotificationAsync(string message)
    {
        using var httpClient = new HttpClient();

        var content = new StringContent(
            message,
            System.Text.Encoding.UTF8,
            "application/json"
        );
        var res = await httpClient.PostAsync(webhookUrl, content);
        if (!res.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Failed to send notification: {res.StatusCode} - {await res.Content.ReadAsStringAsync()}");
        }
    }

    public async Task SendRackNotificationAsync(Rack rack, SyndicationItem post)
    {
        if (rack == null)
        {
            throw new ArgumentNullException(nameof(rack), "Rack cannot be null");
        }

        var message = $"Rack Notification: {rack.Name} - {post.Title.Text}\n{post.Summary.Text}";
        await SendDiscordNotificationAsync(message);
    }

    public async Task SendDataCenterNotificationAsync(Datacenter datacenter, SyndicationItem post)
    {
        if (datacenter == null)
        {
            throw new ArgumentNullException(nameof(datacenter), "Datacenter cannot be null");
        }

        await SendDiscordNotificationAsync(GetDiscordMessage(post));
    }

    public async Task SendRegionNotificationAsync(Region region, SyndicationItem post)
    {
        if (region == null)
        {
            throw new ArgumentNullException(nameof(region), "Region cannot be null");
        }

        await SendDiscordNotificationAsync(GetDiscordMessage(post));
    }

    private static string GetDiscordMessage(SyndicationItem post)
    {
        if (post == null)
        {
            throw new ArgumentNullException(nameof(post), "Post cannot be null");
        }

        var description = ConvertHtmlToMarkdown(post.Summary.Text);
        var imageUrl = string.Empty;
        if (RegexImgTag().IsMatch(description))
        {
            // Extract the first image URL from the description
            var match = RegexImgTag().Match(description);
            if (match.Success)
            {
                imageUrl = match.Groups[1].Value;
                RegexImgTag().Replace(description, "");
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

        return System.Text.Json.JsonSerializer.Serialize(message, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }

    private static string ConvertHtmlToMarkdown(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        // Simple conversion logic, can be improved with a proper HTML parser
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

        // Replace <var *> <.var> with `code`
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