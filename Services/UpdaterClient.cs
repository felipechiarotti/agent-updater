using Updater.Models;
using Updater.Responses;
using Updater.Settings;

namespace Updater.Services;

public interface IUpdaterClient
{
    Task<Download?> GetLatestDownload(CancellationToken cancellationToken);
    Task<Stream> DownloadFile(string fileUrl, CancellationToken cancellationToken);
}

public class UpdaterClient : IUpdaterClient
{
    private readonly UpdaterSettings _updaterSettings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UpdaterClient> _logger;

    public UpdaterClient(IOptions<UpdaterSettings> options, HttpClient httpClient, ILogger<UpdaterClient> logger)
    {
        _updaterSettings = options.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Download?> GetLatestDownload(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking the latest version available");

        var response = await _httpClient.GetAsync($"2.0/repositories/{_updaterSettings.Workspace}/{_updaterSettings.Repository}/downloads", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var downloadResponse = JsonSerializer.Deserialize<DownloadResponse>(content, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        var latest = downloadResponse?.Values.FirstOrDefault();
        if (latest == null)
            return default;

        _logger.LogInformation("Latest version found: {latestVersion}", latest.Name);
        return new()
        {
            Name = latest.Name,
            Link = latest.Links.Self.Href
        };
    }

    public async Task<Stream> DownloadFile(string fileUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading latest version");

        var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }
}
