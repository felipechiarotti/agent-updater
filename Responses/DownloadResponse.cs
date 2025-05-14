namespace Updater.Responses;
public class DownloadResponse
{
    public List<DownloadItem> Values { get; set; } = new();
}

public class DownloadItem
{
    public string Name { get; set; } = string.Empty;

    public DownloadLinks Links { get; set; } = new();
}

public class DownloadLinks
{
    public Link Self { get; set; } = new();
}

public class Link
{
    public string Href { get; set; } = string.Empty;
}