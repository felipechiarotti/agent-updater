namespace Updater.Services;

public interface IWindowsServiceHandler
{
    void Uninstall(string serviceName);
    void Install(string serviceName, string assemblyName);
}

public class WindowsServiceHandler : IWindowsServiceHandler
{
    private readonly ILogger<WindowsServiceHandler> _logger;

    public WindowsServiceHandler(ILogger<WindowsServiceHandler> logger)
    {
        _logger = logger;
    }

    public void Uninstall(string serviceName)
    {
        _logger.LogInformation("Uninstalling windows service: {serviceName}", serviceName);
        Process.Start(GetProcessStartInfo("uninstall.bat", serviceName));
    }

    public void Install(string serviceName, string assemblyName)
    {
        _logger.LogInformation("Installing windows service: {serviceName}", serviceName);
        Process.Start(GetProcessStartInfo("install.bat", serviceName, assemblyName));
    }

    private ProcessStartInfo GetProcessStartInfo(string fileName, params string[] arguments)
    {
        return new ProcessStartInfo
        {
            FileName = Path.Combine(AppContext.BaseDirectory, fileName),
            Arguments = string.Join(" ", arguments),
            Verb = "runas",
            UseShellExecute = true,
        };
    }
}
