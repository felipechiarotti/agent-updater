namespace Updater;
public class App(
    IUpdaterClient updaterClient,
    IFileHandler fileHandler,
    IWindowsServiceHandler windowsServiceHandler,
    ILogger<App> logger,
    IOptions<WindowsServiceSettings> windowsServiceOptions,
    IOptions<TargetSettings> targetOptions) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var serviceName = windowsServiceOptions.Value.ServiceName;
        var targetServiceName = windowsServiceOptions.Value.TargetServiceName;
        var updateInterval = windowsServiceOptions.Value.CheckUpdateInterval;

        var isWindowsService = !Environment.UserInteractive;
        var useAsWindowsService = windowsServiceOptions.Value.UseAsWindowsService;
        if (!isWindowsService && useAsWindowsService)
        {
            await InstallUpdater();
            Environment.Exit(0);
        }

        logger.LogInformation("Windows Service: {windowsServiceName} started. Check Interval: {Interval}", serviceName, updateInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var latestDownload = await updaterClient.GetLatestDownload(stoppingToken);
                if (latestDownload == null)
                {
                    logger.LogInformation("No version was found");
                    continue;
                }

                var isUpToDate = fileHandler.IsAssemblyUpToDate(latestDownload.Name.Replace(".zip", ""));
                if (isUpToDate)
                {
                    logger.LogInformation("The {targetServiceName} is up to date", serviceName);
                    continue;
                }

                fileHandler.BackupFiles();

                using var stream = await updaterClient.DownloadFile(latestDownload.Link, stoppingToken);

                windowsServiceHandler.Uninstall(targetServiceName);

                await fileHandler.UpdateTarget(stream);

                fileHandler.RestoreBackup();

                windowsServiceHandler.Install(targetServiceName, Path.Combine(targetOptions.Value.Path, targetOptions.Value.Assembly));

                logger.LogInformation("{targetServiceName} updated to the latest version successfully", targetServiceName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking for updates");
            }
            finally
            {
                logger.LogInformation("Checking updates again in: {interval}", updateInterval);
                await Task.Delay(updateInterval, stoppingToken);
            }
        }
    }

    private async Task InstallUpdater()
    {
        var serviceName = windowsServiceOptions.Value.ServiceName;
        var updaterServiceExists = ServiceController.GetServices().Any(s => s.ServiceName == serviceName);
        if (updaterServiceExists)
        {
            logger.LogInformation("Windows Service: {windowsServiceName} already installed", serviceName);

            windowsServiceHandler.Uninstall(serviceName);
            while (updaterServiceExists = ServiceController.GetServices().Any(s => s.ServiceName == serviceName))
            {
                await Task.Delay(5);
            }
        }

        windowsServiceHandler.Install(serviceName, Assembly.GetExecutingAssembly().GetName().Name!);
        logger.LogInformation("Windows Service: {windowsServiceName} was successfully installed", serviceName);
        return;
    }
}