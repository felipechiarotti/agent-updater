namespace Updater.Settings;
internal class BackupSettings
{
    public bool Enabled { get; set; }
    public string Path { get; set; } = string.Empty;
    public IEnumerable<string> Files { get; set; } = Enumerable.Empty<string>();
}
