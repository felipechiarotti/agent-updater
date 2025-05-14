namespace Updater.Services;
public interface IFileHandler
{
    bool IsAssemblyUpToDate(string version);
    Task UpdateTarget(Stream stream);
    void BackupFiles();
    void RestoreBackup();
}

internal class FileHandler : IFileHandler
{
    private readonly TargetSettings _targetSettings;
    private readonly BackupSettings _backupSettings;
    private readonly ILogger<FileHandler> _logger;

    public FileHandler(IOptions<TargetSettings> targetOptions, IOptions<BackupSettings> backupOptions, ILogger<FileHandler> logger)
    {
        _targetSettings = targetOptions.Value;
        _backupSettings = backupOptions.Value;
        _logger = logger;
    }

    public bool IsAssemblyUpToDate(string version)
    {
        var assemblyPath = Path.Combine(_targetSettings.Path.GetFullPath(), $"{_targetSettings.Assembly}.dll");
        if (!Path.Exists(_targetSettings.Path.GetFullPath()) || !File.Exists(assemblyPath))
        {
            return false;
        }
        var assembly = AssemblyName.GetAssemblyName(assemblyPath);
        var assemblyVersion = assembly.Version?.ToString();
        _logger.LogInformation("Installed version: {installedVersion}", assemblyVersion);
        return string.Equals(version, assemblyVersion!.Substring(0, assemblyVersion.Length - 2), StringComparison.OrdinalIgnoreCase);
    }

    public async Task UpdateTarget(Stream stream)
    {
        var targetPathExists = Path.Exists(_targetSettings.Path.GetFullPath());
        if (targetPathExists)
        {
            Directory.Delete(_targetSettings.Path.GetFullPath(), true);
            while (targetPathExists = Path.Exists(_targetSettings.Path.GetFullPath()))
            {
                await Task.Delay(5);
            }
        }

        Directory.CreateDirectory(_targetSettings.Path.GetFullPath());

        ZipFile.ExtractToDirectory(stream, _targetSettings.Path.GetFullPath());
    }

    public void BackupFiles()
    {
        if (!_backupSettings.Enabled)
            return;

        _logger.LogInformation("Backing up files to /{backupPath}: {@files}", _backupSettings.Path, _backupSettings.Files);
        foreach (var file in _backupSettings.Files)
        {
            var fileDir = Path.Combine(_targetSettings.Path.GetFullPath(), file);

            if (!File.Exists(fileDir))
            {
                continue;
            }
            Directory.CreateDirectory(_backupSettings.Path.GetFullPath());
            File.Copy(fileDir, Path.Join(_backupSettings.Path.GetFullPath(), file), true);
        }
    }

    public void RestoreBackup()
    {
        if (!_backupSettings.Enabled)
            return;

        if (!Directory.Exists(_backupSettings.Path.GetFullPath()))
            return;

        _logger.LogInformation("Restoring backup from /{backupPath}", _backupSettings.Path.GetFullPath());

        foreach (var file in _backupSettings.Files)
        {
            File.Copy(Path.Join(_backupSettings.Path, file), Path.Join(_targetSettings.Path.GetFullPath(), file), true);
        }

        Directory.Delete(_backupSettings.Path.GetFullPath(), true);
    }
}
