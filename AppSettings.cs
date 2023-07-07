namespace yabm;

public class AppSettings
{
    public static readonly string BEATMAPS_PATH;
    public static readonly int CONCURRENCY;

    static AppSettings()
    {
        BEATMAPS_PATH = Environment.GetEnvironmentVariable("BEATMAPS_PATH") ?? "/Users/polina/Development/yabm-loader-rs/target/beatmaps";
        CONCURRENCY = int.TryParse(Environment.GetEnvironmentVariable("CONCURRENCY"), out var result) ? result : 1;
    }
}