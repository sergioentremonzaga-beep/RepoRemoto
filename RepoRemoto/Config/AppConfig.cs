namespace RepoRemoto.Config;

public class AppConfig
{
    public const string Position = "AppConfig";
    public int SyncIntervalSeconds { get; set; } = 60;
    public int CacheDurationMinutes { get; set; } = 5;
}