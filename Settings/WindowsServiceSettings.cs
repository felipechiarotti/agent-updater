public class WindowsServiceSettings
{
    public string ServiceName { get; set; } = string.Empty;
    public string TargetServiceName { get; set; } = string.Empty;
    public TimeSpan CheckUpdateInterval { get; set; } = TimeSpan.FromHours(6);
    public bool UseAsWindowsService { get; set; } = false;
}